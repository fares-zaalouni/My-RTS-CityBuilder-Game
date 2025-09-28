using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public partial struct GridInitializationSystem : ISystem
{
    private const ushort INITIAL_FF_COUNT = 1;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridMeta>();
    }
    public void OnUpdate(ref SystemState state)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        GridMeta grid = SystemAPI.GetSingleton<GridMeta>();
        for (int ff = 0; ff < INITIAL_FF_COUNT; ff++)
        {


            Entity entity = state.EntityManager.CreateEntity();

            FfStateData ffState = new FfStateData
            {
                State = FfStateData.FlowFieldSate.Ready,
                JobHandles = new NativeArray<JobHandle>(3, Allocator.Persistent)
            };

            FfGridData gridDynamic = new FfGridData
            {
                Cells = new NativeArray<FfCellData>(grid.GridSize, Allocator.Persistent)
            };

            FfBestCosts bestCosts = new FfBestCosts
            {
                Cells = new NativeArray<FfCellBestCost>(grid.GridSize, Allocator.Persistent)
            };

            FFOpenList openList = new FFOpenList
            {
                Heap = new NativeMinHeap(grid.GridSize, Allocator.Persistent)
            };

            for (int i = 0; i < grid.GridSize; i++)
            {
                gridDynamic.Cells[i] = new FfCellData
                {
                    Walkable = true,
                    Cost = 1,
                    GridPos = new int3(i % grid.SizeX, 0, i / grid.SizeZ)
                };
                bestCosts.Cells[i] = new FfCellBestCost
                {
                    BestCost = uint.MaxValue,
                };
                
            }
            FfDestination lastDestination = new FfDestination
            {
                Index = -1,
                OriginalCost = 1
            };

            FfCancelationToken cancelationToken = new FfCancelationToken
            {
                Token = new NativeReference<bool>(false, Allocator.Persistent)
            };
            state.EntityManager.AddComponentData(entity, gridDynamic);
            state.EntityManager.AddComponentData(entity, bestCosts);
            state.EntityManager.AddComponentData(entity, openList);
            state.EntityManager.AddComponentData(entity, ffState);
            state.EntityManager.AddComponentData(entity, lastDestination);
            state.EntityManager.AddComponentData(entity, cancelationToken);
            stopwatch.Stop();
            Debug.Log("creating flow field took: " + stopwatch.ElapsedMilliseconds + " ms");
            state.Enabled = false;
        }

    }
}
