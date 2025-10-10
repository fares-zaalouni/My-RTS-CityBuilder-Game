

using Unity.Collections;
using Unity.Entities;

public partial struct SegmentedFlowField : IComponentData
{
    public NativeArray<FfBestCosts> FfBestCostsChunk;
    public NativeArray<FfNeighboursCosts> FfNeighboursChunk;
    public NativeArray<FfMetaData> FfMetaDataChunk;
    public FlowFieldState FfState;
}