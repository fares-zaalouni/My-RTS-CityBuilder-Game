using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
public enum FlowFieldState
  {
    Waiting,
    Available,
    Calculating,
    Ready
  }
public struct FfMetaData : IComponentData
{
  

  //public FlowFieldState State;
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
  public NativeArray<NativeArray<FfCellData>> Cells;
}

public struct FfCellBestCost : IComponentData
{
  public uint BestCost;
}

public struct FfBestCosts : IComponentData
{
  public NativeArray<FfCellBestCost> Cells;
}

public struct FfOpenList : IComponentData
{
  public NativeMinHeap Heap;
}

public struct FfNeighboursCost
{
  public int NorthPos;
  public int EastPos;
  public int SouthPos;
  public int WestPos;
  public int NorthEastPos;
  public int SouthEastPos;
  public int SouthWestPos;
  public int NorthWestPos;

  public byte North;
  public byte East;
  public byte South;
  public byte West;
  public byte NorthEast;
  public byte SouthEast;
  public byte SouthWest;
  public byte NorthWest;
}

public struct FfNeighboursCosts : IComponentData
{
  public NativeArray<FfNeighboursCost> Cells;
}