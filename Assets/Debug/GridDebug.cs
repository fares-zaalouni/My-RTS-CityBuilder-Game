
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
          typeof(SegmentedFlowFieldCalculationData));

        if (query.CalculateEntityCount() == 0)
            return;
        SegmentedFlowFieldCalculationData segmentedFlowField = query.GetSingleton<SegmentedFlowFieldCalculationData>();

        query = entityManager.CreateEntityQuery(
          typeof(FfBestDirections));
        if (query.CalculateEntityCount() != 0)
        {
            using (NativeArray<FfBestDirections> bestDirections =
                    query.ToComponentDataArray<FfBestDirections>(Allocator.TempJob))
            {
                
                for(int i = 0; i < gridMeta.CellsInChunk; i++)
                {
                    int chunkX = bestDirections[0].ChunkPosition % gridMeta.ChunksInX;
                    int chunkZ = bestDirections[0].ChunkPosition / gridMeta.ChunksInZ;
                    int cellInChunkX = i % gridMeta.CellsInChunkRow;
                    int cellInChunkZ = i / gridMeta.CellsInChunkRow;
                    float3 posCell = new float3
                    {
                        x = gridMeta.WorldPos.x + chunkX * gridMeta.ChunkDiameter + cellInChunkX * gridMeta.CellDiameter + gridMeta.CellRadius,
                        y = 0,
                        z = gridMeta.WorldPos.z + chunkZ * gridMeta.ChunkDiameter + cellInChunkZ * gridMeta.CellDiameter + gridMeta.CellRadius
                    };
                    Gizmos.DrawRay(posCell, bestDirections[0].Cells[i].BestDirection);
                }
            }
        }
        for (int i = 0; i < gridMeta.ChunkNumber; i++)
        {
            int x = i % gridMeta.ChunksInX;
            int z = i / gridMeta.ChunksInX;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(
                x * (gridMeta.ChunkDiameter + 0.25f) + gridMeta.WorldPos.x + gridMeta.ChunkRadius,
                0,
                z * (gridMeta.ChunkDiameter + 0.25f) + gridMeta.WorldPos.z + gridMeta.ChunkRadius)
                , new Vector3(gridMeta.ChunkDiameter + 0.25f, 0, gridMeta.ChunkDiameter + 0.25f));

            Gizmos.color = Color.red;
           
            for (int j = 0; j < gridMeta.CellsInChunk; j++)
            {
                int x2 = j % gridMeta.CellsInChunkRow;
                int z2 = j / gridMeta.CellsInChunkRow;
                Gizmos.DrawWireCube(new Vector3(
                x2 * gridMeta.CellDiameter + gridMeta.WorldPos.x + x * (gridMeta.ChunkDiameter + 0.25f) + gridMeta.CellRadius,
                0,
                z2 * gridMeta.CellDiameter + gridMeta.WorldPos.z + z * (gridMeta.ChunkDiameter + 0.25f) + gridMeta.CellRadius),
                new Vector3(gridMeta.CellDiameter, 0, gridMeta.CellDiameter));
            }
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
