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
        GridMeta grid = SystemAPI.GetSingleton<GridMeta>();

        Entity entity = state.EntityManager.CreateEntity();

        FfGridData gridDynamic = new FfGridData
        {
            Cells = new NativeArray<NativeArray<FfCellData>>(grid.ChunkNumber, Allocator.Persistent)
        };
        for (int i = 0; i < grid.ChunkNumber; i++)
        {
            gridDynamic.Cells[i] = new NativeArray<FfCellData>(grid.CellsInChunk, Allocator.Persistent);
        }

        Entity flowFieldChunk = state.EntityManager.CreateEntity();

        SegmentedFlowField segmentedFlowField = new SegmentedFlowField
        {
            FfBestCostsChunk = new NativeArray<FfBestCosts>(grid.ChunkNumber, Allocator.Persistent),
            FfNeighboursChunk = new NativeArray<FfNeighboursCosts>(grid.ChunkNumber, Allocator.Persistent),
            FfMetaDataChunk = new NativeArray<FfMetaData>(grid.ChunkNumber, Allocator.Persistent),
        };
        


        for (int i = 0; i < grid.ChunkNumber; i++)
        {
            var row = gridDynamic.Cells[i];
            for(int j = 0; j < grid.CellsInChunk; j++ )
            {
                int chunkX = j % grid.CellsInChunkRow; 
                int chunkZ = j / grid.CellsInChunkRow; 
                row[j] = new FfCellData
                {
                    Walkable = true,
                    Cost = 1,
                    GridPos = new int3(chunkX, 0, chunkZ)
                };
            }
            
        }

        for (int i = 0; i < grid.ChunkNumber; i++)
        {
            segmentedFlowField.FfMetaDataChunk[i] = new FfMetaData
            {
                JobHandles = new NativeArray<JobHandle>(3, Allocator.Persistent),
                //State = FlowFieldState.Available
            };

            segmentedFlowField.FfBestCostsChunk[i] = new FfBestCosts
            {
                Cells = new NativeArray<FfCellBestCost>(grid.CellsInChunk, Allocator.Persistent)
            };

            segmentedFlowField.FfNeighboursChunk[i] = new FfNeighboursCosts
            {
                Cells = new NativeArray<FfNeighboursCost>(grid.CellsInChunk * 8, Allocator.Persistent)
            };
        }
        state.EntityManager.AddComponentData(entity, gridDynamic);

        state.EntityManager.AddComponentData(flowFieldChunk, segmentedFlowField);
        
        /*for (int ff = 0; ff < INITIAL_FF_COUNT; ff++)
        {



            FfStateData ffState = new FfStateData
            {
                State = FfStateData.FlowFieldSate.Ready,
                JobHandles = new NativeArray<JobHandle>(3, Allocator.Persistent)
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
            
        }*/
        state.Enabled = false;
    }
}
