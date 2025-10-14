

using Unity.Collections;
using Unity.Entities;

public partial struct SegmentedFlowFieldData : IComponentData
{
    public NativeArray<Entity> ChunkEntities;
}

public partial struct SegmentedFlowFieldCalculationData : IComponentData
{
    public NativeArray<Entity> ChunkEntities;
}