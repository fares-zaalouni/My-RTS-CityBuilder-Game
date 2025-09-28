
using Unity.Entities;
using Unity.Mathematics;

public struct UnitDirectionResponse : IComponentData
{
  public float3 Direction;
  public int Index;
}