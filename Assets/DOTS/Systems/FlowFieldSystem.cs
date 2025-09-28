using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using Debug = UnityEngine.Debug;

[UpdateAfter(typeof(SelectionSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
partial struct FlowFieldSystem : ISystem
{

    public static float FieldFlowTime;
    NativeArray<FfNeighboursCost> NeighboursCosts;
    NativeArray<int3> Directions;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridMeta>();
        state.RequireForUpdate<FfGridData>();
        state.RequireForUpdate<FfBestCosts>();
        state.RequireForUpdate<FfStateData>();
        state.RequireForUpdate<FfDestination>();
        //stopwatch = Stopwatch.StartNew();

        Directions = new NativeArray<int3>(8, Allocator.Persistent);
        Directions[0] = new int3(0, 0, 1); // North
        Directions[1] = new int3(1, 0, 0); // East
        Directions[2] = new int3(0, 0, -1); // South
        Directions[3] = new int3(-1, 0, 0); // West
        Directions[4] = new int3(1, 0, 1); // NorthEast
        Directions[5] = new int3(1, 0, -1); // SouthEast
        Directions[6] = new int3(-1, 0, -1); // SouthWest
        Directions[7] = new int3(-1, 0, 1); // NorthWest

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        GridMeta gridMeta = SystemAPI.GetSingleton<GridMeta>();

        if (!NeighboursCosts.IsCreated || NeighboursCosts.Length != gridMeta.GridSize)
        {
            if (NeighboursCosts.IsCreated)
                NeighboursCosts.Dispose();
            NeighboursCosts = new NativeArray<FfNeighboursCost>(gridMeta.GridSize, Allocator.Persistent);
        }
        foreach (var (ffDynamic, ffBestCosts, ffOpenList, ffState, destination, ffCancelationToken) in SystemAPI.Query<
        RefRO<FfGridData>,
        RefRW<FfBestCosts>,
        RefRW<FFOpenList>,
        RefRW<FfStateData>,
        RefRO<FfDestination>,
        RefRO<FfCancelationToken>
        >())
        {
            if (ffState.ValueRO.State == FfStateData.FlowFieldSate.Available)
            {
                var entityManager = state.EntityManager;
                Entity entity = entityManager.CreateEntity();
                FfBestDirections ffBestDirections = new FfBestDirections
                {
                    Cells = new NativeArray<FfCellBestDirection>(),
                    AssignedGroup = destination.ValueRO.AssignedGroup
                };
                entityManager.AddComponentData(entity, ffBestDirections);
                ffState.ValueRW.State = FfStateData.FlowFieldSate.Calculating;
                var initJob = new InitFlowFieldJob
                {
                    BestCosts = ffBestCosts.ValueRW.Cells,
                    GridMeta = gridMeta,
                    CellsData = ffDynamic.ValueRO.Cells,
                    NeighboursCosts = NeighboursCosts,
                    Directions = Directions,
                    CancelationToken = ffCancelationToken.ValueRO.Token
                }.Schedule(ffDynamic.ValueRO.Cells.Length, 64);

                var integrationJob = new CalculateIntegrationFieldJob
                {
                    CellsData = ffDynamic.ValueRO.Cells,
                    CellsBestCosts = ffBestCosts.ValueRW.Cells,
                    GridMeta = gridMeta,
                    Destination = destination.ValueRO,
                    OpenList = ffOpenList.ValueRW.Heap,
                    NeighboursCosts = NeighboursCosts,
                    Directions = Directions,
                    CancelationToken = ffCancelationToken.ValueRO.Token
                }.Schedule(initJob);

                var directionJob = new CalculateBestDirectionJob
                {
                    Directions = Directions,
                    CellsBestCosts = ffBestCosts.ValueRW.Cells,
                    CellsBestDirections = ffBestDirections.Cells,
                    GridMeta = gridMeta,
                    CancelationToken = ffCancelationToken.ValueRO.Token
                }.Schedule(ffBestCosts.ValueRW.Cells.Length, 64, integrationJob);
                
                //Registring job handles          
                ffState.ValueRW.JobHandles[0] = initJob; 
                ffState.ValueRW.JobHandles[1] = integrationJob; 
                ffState.ValueRW.JobHandles[2] = directionJob;

                ffState.ValueRW.State = FfStateData.FlowFieldSate.Waiting;

            }
            
            if (ffState.ValueRO.JobHandles[2].IsCompleted)
            {
                ffState.ValueRO.JobHandles[2].Complete();
                ffState.ValueRW.State = FfStateData.FlowFieldSate.Ready;

            }

            
        }
    }

    public void OnDestroy(ref SystemState state)
    {
        if (NeighboursCosts.IsCreated)
            NeighboursCosts.Dispose();
        if (Directions.IsCreated)
            Directions.Dispose();

        foreach (var (ffDynamic, ffBestCosts, ffOpenList, ffState, ffCancelationToken) in SystemAPI.Query<
        RefRW<FfGridData>,
        RefRW<FfBestCosts>,
        RefRW<FFOpenList>,
        RefRW<FfStateData>,
        RefRW<FfCancelationToken>
        >())
        {
            ffDynamic.ValueRW.Cells.Dispose();
            ffBestCosts.ValueRW.Cells.Dispose();
            ffOpenList.ValueRW.Heap.Dispose();
            ffState.ValueRW.JobHandles.Dispose();
            ffCancelationToken.ValueRW.Token.Dispose();
        }
    }

    
    
}
