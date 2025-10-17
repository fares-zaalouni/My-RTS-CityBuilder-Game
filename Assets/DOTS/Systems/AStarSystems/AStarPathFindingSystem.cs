using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(AStarInitSystem))]
partial struct AStarPathFindingSystem : ISystem
{
    private NativeArray<int3> _Directions;
    NativeArray<int> Neighbours;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridMeta>();
        state.RequireForUpdate<AStarData>();
        _Directions = new NativeArray<int3>(8, Allocator.Persistent);
        _Directions[0] = new int3(0, 0, 1); // North
        _Directions[1] = new int3(1, 0, 0); // East
        _Directions[2] = new int3(0, 0, -1); // South
        _Directions[3] = new int3(-1, 0, 0); // West
        _Directions[4] = new int3(1, 0, 1); // NorthEast
        _Directions[5] = new int3(1, 0, -1); // SouthEast
        _Directions[6] = new int3(-1, 0, -1); // SouthWest
        _Directions[7] = new int3(-1, 0, 1); // NorthWest
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        AStarPath aStarPath = new AStarPath
        {
            Path = new NativeList<int>(Allocator.Persistent)
        };
        
        EntityManager entityManager = state.EntityManager;
        GridMeta gridMeta = SystemAPI.GetSingleton<GridMeta>();
        AStarData aStarData = SystemAPI.GetSingleton<AStarData>();
        if(!Neighbours.IsCreated)
        {
            Neighbours = new NativeArray<int>(8, Allocator.Persistent);
        }
            
        Entity aStarPathEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(aStarPathEntity, aStarPath);
        JobHandle initJob = new AStarInitJob
        {
            Nodes = aStarData.Nodes,
            TouchedNodes = aStarData.TouchedNodes
        }.Schedule(aStarData.TouchedNodes.Length, 64);

        JobHandle findPathJob = new AStarPathFindingJob
        {
            ResultPath = aStarPath.Path,
            StartNodeIndex = 0,
            EndNodeIndex = 14,
            GridSizeX = gridMeta.ChunksInX,
            GridSizeZ = gridMeta.ChunksInZ,
            OpenList = aStarData.OpenList,
            Neighbours = Neighbours,
            Directions = _Directions,
            TouchedNodes = aStarData.TouchedNodes
        }.Schedule(initJob);
        findPathJob.Complete();
        state.Enabled = false;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        if (_Directions.IsCreated)
            _Directions.Dispose();
        if (Neighbours.IsCreated)
            Neighbours.Dispose();

    }
}
