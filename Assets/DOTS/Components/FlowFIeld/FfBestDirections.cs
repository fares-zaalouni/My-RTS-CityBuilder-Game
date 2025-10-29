using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public struct FfBestDirections : IComponentData
{
    public NativeArray<FfCellBestDirection> Cells;
}

public struct FfCellBestDirection
{
  public float3 BestDirection;
}

public struct FfRequest : IComponentData
{
  public int ChunkIndex;
  public int NextChunkIndex;
  public int DestinationCellPos;
  public uint RequesterId;
}

public struct FfRequester : IComponentData
{
  public uint RequesterId;
}

public struct FfBestDirectionsStatus : IComponentData
{
  public JobHandle JobHandle;
  public Entity AssociatedSegmentedFlowFieldCalculationData;
  public int UsedChunkIndex;
  public bool IsReady()
  {
    if (JobHandle.IsCompleted)
    {
      JobHandle.Complete();
      return true;
    }
    return false;
  }
}