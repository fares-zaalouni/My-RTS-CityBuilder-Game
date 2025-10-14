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
        AStarData astarData = new AStarData
        {
            Nodes = new NativeArray<AStarNode>(gridMeta.ChunkNumber, Allocator.Persistent)
        };
        
        for (int i = 0; i < gridMeta.ChunkNumber; i++)
        {
            astarData.Nodes[i] = new AStarNode
            {
                Index = i,
                IsInClosedSet = false,
                ParentIndex = -1,
                HeapIndex = -1
            };
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
