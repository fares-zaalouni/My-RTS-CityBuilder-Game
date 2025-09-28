using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public struct FfStateData : IComponentData
{
  public enum FlowFieldSate
  {
    Waiting,
    Available,
    Calculating,
    Ready
  }

  public FlowFieldSate State;
  public NativeArray<JobHandle> JobHandles;
}


public struct FfCellData : IComponentData
{
  public bool Walkable;
  public byte Cost;
  public int3 GridPos;
}

public struct FfGridData : IComponentData
{
  public NativeArray<FfCellData> Cells;
}

public struct FfCellBestCost : IComponentData
{
  public uint BestCost;
}

public struct FfBestCosts : IComponentData
{
  public NativeArray<FfCellBestCost> Cells;
}

public struct FFOpenList : IComponentData
{
  public NativeMinHeap Heap;
}

public struct FfCancelationToken : IComponentData
{
  public NativeReference<bool> Token;
}