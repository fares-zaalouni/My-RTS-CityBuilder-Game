using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateAfter(typeof(FlowFieldSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
partial struct FlowFieldToMonoBridgeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitDirectionRequest>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ffState = SystemAPI.GetSingleton<FfStateData>();
        if (ffState.State != FfStateData.FlowFieldSate.Ready)
            return;
        var bestDirections = SystemAPI.GetSingleton<FfBestDirections>();
        var gridMeta = SystemAPI.GetSingleton<GridMeta>();

        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var jobHandle = new TreatDirectionRequest
        {
            BestDirections = bestDirections,
            GridMeta = gridMeta,
            ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.Schedule(state.Dependency);
        state.Dependency = jobHandle;

    }


    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile] 
    public partial struct TreatDirectionRequest : IJobEntity
    {
        public FfBestDirections BestDirections;
        public GridMeta GridMeta;
        public EntityCommandBuffer.ParallelWriter ECB;

        [BurstCompile]
        void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, in UnitDirectionRequest directionRequest)
        {
            int index = GridUtils.GetCellFromPosition(directionRequest.WorldPos, GridMeta);
            if (MathUtils.HasNaN(BestDirections.Cells[index].BestDirection))
                return;
            var responseEntity = ECB.CreateEntity(chunkIndex);
            ECB.AddComponent(chunkIndex, responseEntity, new UnitDirectionResponse
            {
                Index = directionRequest.Index,
                Direction = BestDirections.Cells[index].BestDirection
            });

            ECB.DestroyEntity(chunkIndex, entity);
        }
    }
}
