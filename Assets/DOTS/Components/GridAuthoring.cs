using Unity.Collections;
using Unity.Entities;
using UnityEngine;

using Unity.Mathematics;

public class GridAuthoring : MonoBehaviour
{
    [SerializeField] Vector3 WorldSize;
    [SerializeField] float CellRadius;
    [SerializeField] short ChunkRadius;

    public class Baker : Baker<GridAuthoring>
    {
        public override void Bake(GridAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new GridMeta
            (
                authoring.transform.position,
                authoring.WorldSize,
                authoring.CellRadius,
                authoring.ChunkRadius          
            ));
        }
    }

}

public struct GridMeta : IComponentData
{
    public readonly float3 WorldPos;
    public readonly float3 WorldSize;

    public readonly float CellRadius;
    public readonly float CellDiameter;
    public readonly int SizeX;
    public readonly int SizeZ;
    public readonly short ChunkRadius;
    public readonly short ChunkDiameter;
    public readonly short ChunksInX;
    public readonly short ChunksInZ;

    public readonly int GridSize => SizeX * SizeZ;
    public readonly int ChunkNumber => ChunksInX * ChunksInZ;
    public readonly int CellsInChunk => (int) math.square(ChunkDiameter / CellDiameter);
    public readonly int CellsInChunkRow => (int) (ChunkDiameter / CellDiameter);

    public GridMeta(float3 worldPos, float3 worldSize, float cellRadius, short chunkRadius)
    {
        WorldPos = worldPos;
        WorldSize = worldSize;
        CellRadius = cellRadius;
        CellDiameter = cellRadius * 2;
        SizeX = Mathf.RoundToInt(WorldSize.x / CellDiameter);
        SizeZ = Mathf.RoundToInt(WorldSize.z / CellDiameter);

        ChunkRadius = chunkRadius;
        ChunkDiameter = (short)(chunkRadius * 2);
        ChunksInX = (short)Mathf.RoundToInt(WorldSize.x / ChunkDiameter);
        ChunksInZ = (short)Mathf.RoundToInt(WorldSize.z / ChunkDiameter);

        Debug.Log("Chunk size:  " + ChunkNumber);
    }
}



