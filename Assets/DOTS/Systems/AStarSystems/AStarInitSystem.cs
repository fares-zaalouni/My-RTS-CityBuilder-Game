using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
partial struct AStarInitSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridMeta>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        GridMeta gridMeta = SystemAPI.GetSingleton<GridMeta>();

        EntityManager entityManager = state.EntityManager;

        Entity astarDataEntity = entityManager.CreateEntity();

        NativeArray<AStarNode> nodes = new NativeArray<AStarNode>(gridMeta.ChunkNumber, Allocator.Persistent);
        AStarData astarData = new AStarData
        {
            Nodes = nodes,
            OpenList = new AStarNativeMinHeap(gridMeta.ChunkNumber, nodes, Allocator.Persistent),
            TouchedNodes = new NativeList<int>(Allocator.Persistent)
        };
        for (int i = 0; i < gridMeta.ChunkNumber; i++)
        {
            nodes[i] = new AStarNode
            {
                Index = i,
                IsInClosedSet = false,
                ParentIndex = -1,
                HeapIndex = -1
            };
        }
        entityManager.AddComponentData(astarDataEntity, astarData);
        state.Enabled = false;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
