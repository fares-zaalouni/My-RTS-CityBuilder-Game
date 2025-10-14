using System;
using Unity.Burst;
using Unity.Entities;

[UpdateAfter(typeof(AStarInitSystem))]
partial struct AStarPathFindingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
