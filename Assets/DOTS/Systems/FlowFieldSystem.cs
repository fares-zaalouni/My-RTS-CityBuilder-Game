
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


using Debug = UnityEngine.Debug;

[UpdateAfter(typeof(SelectionSystem))]
[UpdateAfter(typeof(GridInitializationSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
partial struct FlowFieldSystem : ISystem
{
    public static float FieldFlowTime;
    NativeArray<int3> Directions;
    EntityQuery _SegmentedFlowFieldQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridMeta>();
        
        _SegmentedFlowFieldQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<SegmentedFlowFieldCalculationData>()
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
        using (NativeArray<SegmentedFlowFieldCalculationData> flowFields =
               _SegmentedFlowFieldQuery.ToComponentDataArray<SegmentedFlowFieldCalculationData>(Allocator.TempJob))

        {
            
            foreach (var (clickCommand, entity) in SystemAPI.Query<RefRO<ClickCommand>>().WithEntityAccess())
            {
                ProcessClickCommand(flowFields[0], clickCommand.ValueRO, ref ecb, ref state);
                ecb.DestroyEntity(entity);
            }
        }        
        

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

    }

    public void ProcessClickCommand(SegmentedFlowFieldCalculationData segmentedFlowField, ClickCommand clickCommand, ref EntityCommandBuffer ecb, ref SystemState state)
    {
        GridMeta gridMeta = SystemAPI.GetSingleton<GridMeta>();
        SegmentedFlowFieldData gridData = SystemAPI.GetSingleton<SegmentedFlowFieldData>();

        int chunkPos = GridUtils.GetChunkFromPosition(clickCommand.Pos, gridMeta);

        Entity ffGridChunkDataEnity = gridData.ChunkEntities[chunkPos];
        FfGridChunkData ffGridChunkData = SystemAPI.GetComponent<FfGridChunkData>(ffGridChunkDataEnity);

        Entity segmentedFfCalculationDataEntity = segmentedFlowField.ChunkEntities[chunkPos];
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
            destIndex = GridUtils.GetCellInChunkFromPosition(clickCommand.Pos, gridMeta),
            Directions = Directions,
            GridMeta = gridMeta,
            NeighboursCosts = ffChunkNeighbours.Cells,
        }.Schedule(initChunkFf);
        
        Entity bestDestinationEntity = ecb.CreateEntity();
        FfBestDirections ffBestDirections = new FfBestDirections
        {
            Cells = new NativeArray<FfCellBestDirection>(gridMeta.CellsInChunk, Allocator.Persistent),
            ChunkPosition = chunkPos
        };
        ecb.AddComponent(bestDestinationEntity, ffBestDirections);

        JobHandle bestDirectionsInChunk = new CalculateBestDirectionJob
        {
            CellsBestCosts = ffChunkBestCosts.Cells,
            CellsBestDirections = ffBestDirections.Cells,
            Directions = Directions,
            GridMeta = gridMeta
        }.Schedule(gridMeta.CellsInChunk, 64, integrationFieldChunk);
    }

    public void OnDestroy(ref SystemState state)
    {
        Directions.Dispose();
    }
 
}
