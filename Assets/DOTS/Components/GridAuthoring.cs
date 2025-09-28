using Unity.Collections;
using Unity.Entities;
using UnityEngine;

using Unity.Mathematics;

public class GridAuthoring : MonoBehaviour
{
    [SerializeField] Vector3 WorldSize;
    [SerializeField] float CellRadius;

    public class Baker : Baker<GridAuthoring>
    {
        public override void Bake(GridAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            
            AddComponent(entity, new GridMeta
            (
                authoring.transform.position,
                authoring.WorldSize,
                authoring.CellRadius          
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

    public readonly int GridSize => SizeX * SizeZ;

    public GridMeta(float3 worldPos, float3 worldSize, float cellRadius)
    {
        WorldPos = worldPos;
        WorldSize = worldSize;
        CellRadius = cellRadius;
        CellDiameter = cellRadius * 2;
        SizeX = Mathf.RoundToInt(WorldSize.x / CellDiameter);
        SizeZ = Mathf.RoundToInt(WorldSize.z / CellDiameter);
    }
}



