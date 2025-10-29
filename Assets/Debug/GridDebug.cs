
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

public class GridDebug : MonoBehaviour
{
    private enum DisplayMode
    {
        None,
        Walkable,
        Cost,
        BestDirection,
        BestCost
    }
    [SerializeField] bool Active;
    [SerializeField] DisplayMode displayMode = DisplayMode.Walkable;
    [SerializeField] bool displayGrid = true;
    [SerializeField] float ChunkOffset = 0.25f;
    [SerializeField] bool DisplayPath = true;

    void OnDrawGizmos()
    {
        if (!Active)
            return;
        if (!displayGrid)
            return;
        // World might not exist yet (outside play mode)
        if (World.DefaultGameObjectInjectionWorld == null)
            return;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (entityManager == null)
            return;


        var query = entityManager.CreateEntityQuery(
          typeof(GridMeta));
        if (query.CalculateEntityCount() == 0)
            return;
        GridMeta gridMeta = query.GetSingleton<GridMeta>();

        

        query = entityManager.CreateEntityQuery(
            typeof(FfBestDirectionsStatus),
            typeof(FfBestDirections));
        if (query.CalculateEntityCount() != 0)
        {
            using (NativeArray<FfBestDirections> bestDirections =
                    query.ToComponentDataArray<FfBestDirections>(Allocator.TempJob))
            using (NativeArray<FfBestDirectionsStatus> bestDirectionsStatus =
                    query.ToComponentDataArray<FfBestDirectionsStatus>(Allocator.TempJob))
            {
                for (int i = 0; i < bestDirections.Length; i++)
                {
                    for (int j = 0; j < gridMeta.CellsInChunk; j++)
                    {
                        int chunkX = bestDirectionsStatus[i].UsedChunkIndex % gridMeta.ChunksInX;
                        int chunkZ = bestDirectionsStatus[i].UsedChunkIndex / gridMeta.ChunksInZ;
                        int cellInChunkX = j % gridMeta.CellsInChunkRow;
                        int cellInChunkZ = j / gridMeta.CellsInChunkRow;
                        float3 posCell = new float3
                        {
                            x = gridMeta.WorldPos.x + chunkX * gridMeta.ChunkDiameter + cellInChunkX * gridMeta.CellDiameter + gridMeta.CellRadius,
                            y = 0,
                            z = gridMeta.WorldPos.z + chunkZ * gridMeta.ChunkDiameter + cellInChunkZ * gridMeta.CellDiameter + gridMeta.CellRadius
                        };
                        Gizmos.DrawRay(posCell, bestDirections[i].Cells[j].BestDirection);
                    }
                }
            }
        }
        for (int i = 0; i < gridMeta.ChunkNumber; i++)
        {
            int x = i % gridMeta.ChunksInX;
            int z = i / gridMeta.ChunksInX;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(
                x * (gridMeta.ChunkDiameter + ChunkOffset) + gridMeta.WorldPos.x + gridMeta.ChunkRadius,
                0,
                z * (gridMeta.ChunkDiameter + ChunkOffset) + gridMeta.WorldPos.z + gridMeta.ChunkRadius)
                , new Vector3(gridMeta.ChunkDiameter + ChunkOffset, 0, gridMeta.ChunkDiameter + ChunkOffset));

            Gizmos.color = Color.red;

            for (int j = 0; j < gridMeta.CellsInChunk; j++)
            {
                int x2 = j % gridMeta.CellsInChunkRow;
                int z2 = j / gridMeta.CellsInChunkRow;
                Gizmos.DrawWireCube(new Vector3(
                x2 * gridMeta.CellDiameter + gridMeta.WorldPos.x + x * (gridMeta.ChunkDiameter + ChunkOffset) + gridMeta.CellRadius,
                0,
                z2 * gridMeta.CellDiameter + gridMeta.WorldPos.z + z * (gridMeta.ChunkDiameter + ChunkOffset) + gridMeta.CellRadius),
                new Vector3(gridMeta.CellDiameter, 0, gridMeta.CellDiameter));
            }
        }
        if (!DisplayPath)
            return;
        query = entityManager.CreateEntityQuery(
          typeof(AStarPath));

        if (query.CalculateEntityCount() == 0)
            return;
        AStarPath path = query.GetSingleton<AStarPath>();
        
        for (var i = 0; i < path.Path.Length; i++)
        {
            int indexA = path.Path[i];
            int chunkAX = indexA % gridMeta.ChunksInX;
            int chunkAZ = indexA / gridMeta.ChunksInX;

            float centerAX = chunkAX * gridMeta.ChunkDiameter + gridMeta.WorldPos.x + gridMeta.ChunkRadius;
            float centerAZ = chunkAZ * gridMeta.ChunkDiameter + gridMeta.WorldPos.z + gridMeta.ChunkRadius;
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(new float3(centerAX, 0, centerAZ), new float3(gridMeta.ChunkDiameter, gridMeta.ChunkDiameter / 2, gridMeta.ChunkDiameter));
        }

    }

    void OnGUI()
    {
        var query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(
                typeof(FlowFieldTime));

        if (query.CalculateEntityCount() == 0)
            return;

        FlowFieldTime time = query.GetSingleton<FlowFieldTime>();
        GUI.color = Color.black;
        GUI.Label(new Rect(10, 100, 150, 200), "FlowField took: " + time.time + " ms");
    }
}
