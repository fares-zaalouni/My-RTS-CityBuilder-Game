using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct FfBestDirections : IComponentData
{
    public NativeArray<FfCellBestDirection> Cells;
    public byte AssignedGroup;    

}

public struct FfCellBestDirection : IComponentData
{
  public float3 BestDirection;
}