using Unity.Entities;
using Unity.Mathematics;

public struct UnitDirectionRequest : IComponentData
{
  public float3 WorldPos;
  public int Index;
}
