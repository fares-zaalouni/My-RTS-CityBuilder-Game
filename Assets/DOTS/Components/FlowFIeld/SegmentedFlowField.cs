

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public partial struct SegmentedFlowFieldData : IComponentData
{
    public NativeArray<Entity> ChunkEntities;
}

public partial struct SegmentedFlowFieldCalculationData : IComponentData
{
    public NativeHashSet<int> ActiveChunks;
    public NativeArray<Entity> ChunkEntities;
    public NativeArray<JobHandle> ChunkJobs;
}