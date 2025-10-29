
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;

[UpdateAfter(typeof(SelectionSystem))]
[UpdateAfter(typeof(GridInitializationSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
partial struct FlowFieldSystem : ISystem
{
    private uint _Frame;
    private uint _FramesBetweenUpdates;
    public static float FieldFlowTime;
    NativeArray<int3> Directions;
    EntityQuery _SegmentedFlowFieldQuery;
    EntityQuery _BestDirectionsStatusQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridMeta>();
        state.RequireForUpdate<AStarPath>();

        _Frame = 0;
        _FramesBetweenUpdates = 3;

        _SegmentedFlowFieldQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<SegmentedFlowFieldCalculationData>()
            .WithNone<Disabled>()
            .Build(ref state);

        _BestDirectionsStatusQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<FfBestDirectionsStatus>()
            .WithNone<Disabled>()
            .Build(ref state);


        Directions = new NativeArray<int3>(8, Allocator.Persistent);
        Directions[0] = new int3(0, 0, 1); // North
        Directions[1] = new int3(1, 0, 0); // East
        Directions[2] = new int3(0, 0, -1); // South
        Directions[3] = new int3(-1, 0, 0); // West
        Directions[4] = new int3(1, 0, 1); // NorthEast
        Directions[5] = new int3(1, 0, -1); // SouthEast
        Directions[6] = new int3(-1, 0, -1); // SouthWest
        Directions[7] = new int3(-1, 0, 1); // NorthWest
        Entity time = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(time, new FlowFieldTime { time = 0 });
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        using (NativeArray<FfBestDirectionsStatus> bestDirectionsStatuses =
                           _BestDirectionsStatusQuery.ToComponentDataArray<FfBestDirectionsStatus>(Allocator.TempJob))
        using (var entities = _SegmentedFlowFieldQuery.ToEntityArray(Allocator.TempJob))
        using (NativeArray<SegmentedFlowFieldCalculationData> flowFields =
               _SegmentedFlowFieldQuery.ToComponentDataArray<SegmentedFlowFieldCalculationData>(Allocator.TempJob))

        {
            if (_Frame % _FramesBetweenUpdates == 0)
            {
                for (int i = 0; i < flowFields.Length; i++)
                {
                    var flowField = flowFields[i];
                    var ffEntity = entities[i];

                    foreach (var bestDirectionStatus in bestDirectionsStatuses)
                    {
                        if (bestDirectionStatus.AssociatedSegmentedFlowFieldCalculationData != ffEntity)
                            continue;
                        if (!bestDirectionStatus.IsReady())
                            continue;

                        flowField.ActiveChunks.Remove(bestDirectionStatus.UsedChunkIndex);
                    }
                }
            }
            _Frame++;

            foreach (var (ffRequest, entity) in SystemAPI.Query<RefRO<FfRequest>>().WithEntityAccess())
            {
                for (int i = 0; i < flowFields.Length; i++)
                {
                    var flowField = flowFields[i];
                    var ffEntity = entities[i];
                     
                    if (!flowField.ActiveChunks.Contains(ffRequest.ValueRO.ChunkIndex))
                    {
                        ProcessClickCommand(
                                                ffEntity,
                                                flowField,
                                                ffRequest.ValueRO.ChunkIndex,
                                                ffRequest.ValueRO.DestinationCellPos,
                                                ffRequest.ValueRO.NextChunkIndex,
                                                ffRequest.ValueRO.RequesterId,
                                                ref ecb,
                                                ref state);
                        flowField.ActiveChunks.Add(ffRequest.ValueRO.ChunkIndex);
                        ecb.DestroyEntity(entity);
                        break;
                    }
                }
            }
        }        
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public void ProcessClickCommand(Entity segmentedFlowFieldEntity,
                                    SegmentedFlowFieldCalculationData segmentedFlowField,
                                    int chunkIndex,
                                    int destinationPos,
                                    int nextChunkIndex,
                                    uint requesterId,
                                    ref EntityCommandBuffer ecb,
                                    ref SystemState state)
    {
        GridMeta gridMeta = SystemAPI.GetSingleton<GridMeta>();
        SegmentedFlowFieldData gridData = SystemAPI.GetSingleton<SegmentedFlowFieldData>();

        Entity ffGridChunkDataEnity = gridData.ChunkEntities[chunkIndex];
        FfGridChunkData ffGridChunkData = SystemAPI.GetComponent<FfGridChunkData>(ffGridChunkDataEnity);
        
        Entity segmentedFfCalculationDataEntity = segmentedFlowField.ChunkEntities[chunkIndex];
        FfBestCosts ffChunkBestCosts = SystemAPI.GetComponent<FfBestCosts>(segmentedFfCalculationDataEntity);
        FfNeighboursCosts ffChunkNeighbours = SystemAPI.GetComponent<FfNeighboursCosts>(segmentedFfCalculationDataEntity);

        JobHandle initChunkFf = new InitFlowFieldJob()
        {
            BestCosts = ffChunkBestCosts.Cells,
            CellsData = ffGridChunkData.Cells,
            Directions = Directions,
            GridMeta = gridMeta,
            NeighboursCosts = ffChunkNeighbours.Cells
        }.Schedule(gridMeta.CellsInChunk, 64);

        JobHandle integrationFieldChunk = new CalculateIntegrationFieldUniformJob
        {
            CellsBestCosts = ffChunkBestCosts.Cells,
            destIndex = destinationPos,
            Directions = Directions,
            GridMeta = gridMeta,
            NeighboursCosts = ffChunkNeighbours.Cells,
        }.Schedule(initChunkFf);
        
        Entity bestDestinationEntity = ecb.CreateEntity();
        FfBestDirections ffBestDirections = new FfBestDirections
        {
            Cells = new NativeArray<FfCellBestDirection>(gridMeta.CellsInChunk, Allocator.Persistent),
        };
        FfRequester ffRequester = new FfRequester
        {
            RequesterId = requesterId
        };
        
        float3 directionToNextChunk = float3.zero;
        if (nextChunkIndex != -1)
        {
            int3 currentChunkPos = new int3(chunkIndex % gridMeta.ChunksInX, 0, chunkIndex / gridMeta.ChunksInZ);
            int3 nextChunkPos = new int3(nextChunkIndex % gridMeta.ChunksInX, 0, nextChunkIndex / gridMeta.ChunksInZ);
            directionToNextChunk = math.normalize(nextChunkPos - currentChunkPos);
            ffBestDirections.Cells[destinationPos] = new FfCellBestDirection { BestDirection = directionToNextChunk };
        }
        ffBestDirections.Cells[destinationPos] = new FfCellBestDirection { BestDirection = directionToNextChunk };

        JobHandle bestDirectionsInChunk = new CalculateBestDirectionJob
        {
            CellsBestCosts = ffChunkBestCosts.Cells,
            CellsBestDirections = ffBestDirections.Cells,
            Directions = Directions,
            GridMeta = gridMeta,
            DestinationIndex = destinationPos
        }.Schedule(gridMeta.CellsInChunk, 64, integrationFieldChunk);
        FfBestDirectionsStatus ffBestDirectionsStatus = new FfBestDirectionsStatus
        {
            JobHandle = bestDirectionsInChunk,
            UsedChunkIndex = chunkIndex,
            AssociatedSegmentedFlowFieldCalculationData = segmentedFlowFieldEntity
        };
        ecb.AddComponent(bestDestinationEntity, ffBestDirections);
        ecb.AddComponent(bestDestinationEntity, ffRequester);
        ecb.AddComponent(bestDestinationEntity, ffBestDirectionsStatus);
    }

    public void OnDestroy(ref SystemState state)
    {
        Directions.Dispose();
    }
 
}
