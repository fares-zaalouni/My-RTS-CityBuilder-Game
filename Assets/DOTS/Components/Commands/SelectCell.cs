using Unity.Entities;
using Unity.Mathematics;

public struct ClickCommand : IComponentData
{
  public float3 CameraPos;
  public float3 Pos;
  public byte AssignedGroup;
}
