using System.Runtime.CompilerServices;
using Unity.Entities;
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GetChunkFromPosition(float3 worldPos, in GridMeta grid)
  {
    float3 localPos = worldPos - grid.WorldPos;
    int chunkX = (int)math.floor(localPos.x / grid.ChunkDiameter);
    int chunkZ = (int)math.floor(localPos.z / grid.ChunkDiameter);
    chunkX = math.clamp(chunkX, 0, grid.ChunksInX - 1);
    chunkZ = math.clamp(chunkZ, 0, grid.ChunksInX - 1);

    return chunkX + chunkZ * grid.ChunksInX;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int  GetCellInChunkFromPosition(float3 worldPos, in GridMeta grid)
  {
    float3 localPos = worldPos - grid.WorldPos;
    int chunkX = (int)math.floor(localPos.x / grid.ChunkDiameter);
    int chunkZ = (int)math.floor(localPos.z / grid.ChunkDiameter);
    chunkX = math.clamp(chunkX, 0, grid.ChunksInX - 1);
    chunkZ = math.clamp(chunkZ, 0, grid.ChunksInX - 1);

    int x = (int)((localPos.x - chunkX * grid.ChunkDiameter) / grid.CellDiameter);
    int z = (int)((localPos.z - chunkZ * grid.ChunkDiameter) / grid.CellDiameter);

    x = math.clamp(x, 0, grid.CellsInChunkRow - 1);
    z = math.clamp(z, 0, grid.CellsInChunkRow - 1);
    
    return x + z * grid.CellsInChunkRow;
  }
  
}
