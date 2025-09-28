using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public struct GridUtils
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GetCellFromPosition(float3 worldPos, in GridMeta grid)
  {
    float3 localPos = worldPos - grid.WorldPos;
    int x = (int)math.floor(localPos.x / grid.CellDiameter);
    int z = (int)math.floor(localPos.z / grid.CellDiameter);
    x = math.clamp(x, 0, grid.SizeX - 1);
    z = math.clamp(z, 0, grid.SizeZ - 1);
    return x + z * grid.SizeX;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float3 GetWorldPosFromCell(int cellIndex, in GridMeta grid)
  {
    int x = cellIndex % grid.SizeX;
    int z = cellIndex / grid.SizeZ;

    return new float3(x * grid.CellDiameter + grid.CellRadius, 0, z * grid.CellDiameter + grid.CellRadius);
  }
  
}
