
using System.Linq;
using Unity.Entities;
using Unity.Jobs;
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
    [SerializeField] bool GameMode = true;
    [SerializeField] DisplayMode displayMode = DisplayMode.Walkable;
    [SerializeField] bool displayGrid = true;

    
    void OnDrawGizmos()
    {
        if (!Active)
            return;
        if (GameMode)
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
            typeof(FfBestCosts),
            typeof(FfBestDirections),
            typeof(FfGridData),
            typeof(FfStateData)
            );
        if (query.CalculateEntityCount() == 0)
            return;

        FfGridData gridDynamic = query.GetSingleton<FfGridData>();
        FfBestCosts bestCosts = query.GetSingleton<FfBestCosts>();
        FfBestDirections bestDirections = query.GetSingleton<FfBestDirections>();
        FfStateData flowFieldState = query.GetSingleton<FfStateData>();


        if (flowFieldState.JobHandles[2].IsCompleted)
                flowFieldState.JobHandles[2].Complete();
            else
                return;
        query = entityManager.CreateEntityQuery(
            typeof(GridMeta));

        if (query.CalculateEntityCount() == 0)
            return;

        GridMeta gridMeta = query.GetSingleton<GridMeta>();

        for (int x = 0; x < gridMeta.GridSize; x++)
        {
            Gizmos.color = gridDynamic.Cells[x].Walkable ? Color.white : Color.red;
            Gizmos.DrawWireCube(
                new Vector3(
                    (x % gridMeta.SizeX * gridMeta.CellDiameter) + gridMeta.WorldPos.x + gridMeta.CellRadius,
                    0,
                    (x / gridMeta.SizeX * gridMeta.CellDiameter) + gridMeta.WorldPos.z + gridMeta.CellRadius),
                new Vector3(gridMeta.CellDiameter, 0.1f, gridMeta.CellDiameter)
            );

#if UNITY_EDITOR
            Handles.Label(
                new Vector3(
                    (x % gridMeta.SizeX * gridMeta.CellDiameter) + gridMeta.WorldPos.x + gridMeta.CellRadius,
                    0,
                    (x / gridMeta.SizeX * gridMeta.CellDiameter) + gridMeta.WorldPos.z + gridMeta.CellRadius),

                displayMode == DisplayMode.Cost ? gridDynamic.Cells[x].Cost.ToString()
                : displayMode == DisplayMode.BestCost ? bestCosts.Cells[x].BestCost.ToString()
                : ""
            );
            Handles.color = Color.green;
            //Handles.Label(new Vector3(64, 20, 64), $"State: {gridDynamic.State}");
#endif
            if (displayMode == DisplayMode.BestDirection)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(
                    new Vector3(
                        (x % gridMeta.SizeX * gridMeta.CellDiameter) + gridMeta.WorldPos.x + gridMeta.CellRadius,
                        0,
                        (x / gridMeta.SizeX * gridMeta.CellDiameter) + gridMeta.WorldPos.z + gridMeta.CellRadius),
                    new Vector3(bestDirections.Cells[x].BestDirection.x, 0, bestDirections.Cells[x].BestDirection.z) * gridMeta.CellRadius
                );
            }
        }
    }
    public Material lineMaterial;
    void OnRenderObject()
    {
        if (!Active)
            return;
        if (lineMaterial == null) return;
        if(!displayGrid)
            return;
        if(!GameMode)
            return;
        lineMaterial.SetPass(0);
        
        GL.Begin(GL.LINES);

        GL.Color(Color.green);

        if (World.DefaultGameObjectInjectionWorld == null)
            return;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (entityManager == null)
            return;

        var query = entityManager.CreateEntityQuery(
            typeof(FfBestCosts),
            typeof(FfBestDirections),
            typeof(FfGridData),
            typeof(FfStateData)
            );
        if (query.CalculateEntityCount() == 0)
            return;

        FfGridData gridDynamic = query.GetSingleton<FfGridData>();
        FfBestCosts bestCosts = query.GetSingleton<FfBestCosts>();
        FfBestDirections bestDirections = query.GetSingleton<FfBestDirections>();
        FfStateData flowFieldState = query.GetSingleton<FfStateData>();

        query = entityManager.CreateEntityQuery(
            typeof(GridMeta));

        if (query.CalculateEntityCount() == 0)
            return;
        GridMeta gridMeta = query.GetSingleton<GridMeta>();

        GL.PushMatrix();

        for (int x = 0; x < gridMeta.GridSize; x++)
        {
            // Cell center
            Vector3 center = new Vector3(
                (x % gridMeta.SizeX * gridMeta.CellDiameter) + gridMeta.WorldPos.x + gridMeta.CellRadius,
                0,
                (x / gridMeta.SizeX * gridMeta.CellDiameter) + gridMeta.WorldPos.z + gridMeta.CellRadius
            );

            // Color: walkable or not
            Color cellColor = gridDynamic.Cells[x].Walkable ? Color.green : Color.red;
            GL.Color(cellColor);

            // Draw a wire cube (top face square only for simplicity)
            float half = gridMeta.CellDiameter * 0.5f;
            Vector3[] corners =
            {
                center + new Vector3(-half, 0, -half),
                center + new Vector3( half, 0, -half),
                center + new Vector3( half, 0,  half),
                center + new Vector3(-half, 0,  half),
            };

            for (int i = 0; i < 4; i++)
            {
                GL.Vertex(corners[i]);
                GL.Vertex(corners[(i + 1) % 4]);
            }

            // Draw best direction ray if needed
            if (displayMode == DisplayMode.BestDirection)
            {
                GL.Color(Color.blue);
                Vector3 dir = new Vector3(
                    bestDirections.Cells[x].BestDirection.x,
                    0,
                    bestDirections.Cells[x].BestDirection.z
                ) * gridMeta.CellRadius;

                GL.Vertex(center);
                GL.Vertex(center + dir);
            }
        }

        GL.End();
        GL.PopMatrix();
    }
  void OnGUI()
  {
    GUI.color = Color.black;
    GUI.Label(new Rect(10, 10, 150, 200), "FlowField took: " + FlowFieldSystem.FieldFlowTime + " ms");
  }
}
