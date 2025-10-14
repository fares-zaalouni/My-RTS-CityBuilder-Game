using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
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

        Entity segmetedFlowFieldDataEntity = state.EntityManager.CreateEntity();
        SegmentedFlowFieldData segmetedFlowFieldData = new SegmentedFlowFieldData
        {
            ChunkEntities = new NativeArray<Entity>(grid.ChunkNumber, Allocator.Persistent)
        };
        
        Entity segmentedFlowFieldCalculationDataEntity = state.EntityManager.CreateEntity();
        SegmentedFlowFieldCalculationData segmentedFlowFieldCalculationData = new SegmentedFlowFieldCalculationData
        {
            ChunkEntities = new NativeArray<Entity>(grid.ChunkNumber, Allocator.Persistent)
        };
        
        for (int i = 0; i < grid.ChunkNumber; i++)
        {
            short ChunkPosX = (short)(i % grid.ChunksInX);
            short ChunkPosZ = (short)(i / grid.ChunksInZ);

            Entity chunkCellData = state.EntityManager.CreateEntity();
            segmetedFlowFieldData.ChunkEntities[i] = chunkCellData;

            Entity chunkCalculationData = state.EntityManager.CreateEntity();
            segmentedFlowFieldCalculationData.ChunkEntities[i] = chunkCalculationData;

            
            FfGridChunkData ffGridData = new FfGridChunkData
            {
                Cells = new NativeArray<FfCellData>(grid.CellsInChunk, Allocator.Persistent),
                ChunkPosX = ChunkPosX,
                ChunkPosZ = ChunkPosZ
            };

            for (int j = 0; j < grid.CellsInChunk; j++)
            {
                int globalCellPosX = j % grid.CellsInChunkRow;
                int globalCellPosZ = j / grid.CellsInChunkRow;
                ffGridData.Cells[j] = new FfCellData
                {
                    Cost = 1,
                    Walkable = true,
                    GridPos = new int3(globalCellPosX, 0, globalCellPosZ)
                };
            }

            FfBestCosts ffChunkBestCosts = new FfBestCosts
            {
                Cells = new NativeArray<FfCellBestCost>(grid.CellsInChunk, Allocator.Persistent)
            };

            FfNeighboursCosts ffChunkNeighboursCosts = new FfNeighboursCosts
            {
                Cells = new NativeArray<FfNeighboursCost>(grid.CellsInChunk * 8, Allocator.Persistent)
            };

            state.EntityManager.AddComponentData(chunkCellData, ffGridData);

            state.EntityManager.AddComponentData(chunkCalculationData, ffChunkBestCosts);
            state.EntityManager.AddComponentData(chunkCalculationData, ffChunkNeighboursCosts);
        }

        state.EntityManager.AddComponentData(segmetedFlowFieldDataEntity, segmetedFlowFieldData);
        state.EntityManager.AddComponentData(segmentedFlowFieldCalculationDataEntity, segmentedFlowFieldCalculationData);
        
        
        state.Enabled = false;
    }
}
