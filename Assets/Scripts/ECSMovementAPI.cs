using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ECSMovementAPI : MonoBehaviour
{
    private static EntityQuery _GridQuery;
    private static EntityQuery _AStarPathQuery;
    private static EntityQuery _FlowFieldQuery;
    void Awake()
    {
        _GridQuery = World.DefaultGameObjectInjectionWorld
                                    .EntityManager
                                    .CreateEntityQuery(typeof(GridMeta));
        _AStarPathQuery = World.DefaultGameObjectInjectionWorld
                                    .EntityManager
                                    .CreateEntityQuery(typeof(AStarPath), typeof(AStarPathRequester));
        _FlowFieldQuery = World.DefaultGameObjectInjectionWorld
                                    .EntityManager
                                    .CreateEntityQuery(typeof(FfBestDirections), typeof(FfRequester), typeof(FfBestDirectionsStatus));
    }
    void OnEnable()
    {
        //UnitSelectionManager.OnMoveTo += RequestPath;
    }

    public static void RequestPath(Vector3 start, Vector3 end, uint groupId)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        GridMeta gridMeta = _GridQuery.GetSingleton<GridMeta>();
        var entity = entityManager.CreateEntity();
        entityManager.AddComponentData(entity, new AStarPathRequest
        {
            RequesterId = groupId,
            StartPosition = GridUtils.GetChunkFromPosition(start, gridMeta),
            EndPosition = GridUtils.GetChunkFromPosition(end, gridMeta)
        });
    }
    
    public static void RequestPath(Vector3 end, List<Unit> units, uint groupId)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        GridMeta gridMeta = _GridQuery.GetSingleton<GridMeta>();

        HashSet<int> uniquePositions = new HashSet<int>();
        foreach (var unit in units)
        {
            int chunkIndex = GridUtils.GetChunkFromPosition(unit.transform.position, gridMeta);
            uniquePositions.Add(chunkIndex);
        }

        int endChunkIndex = GridUtils.GetChunkFromPosition(end, gridMeta);
        foreach (var pos in uniquePositions)
        {
            var entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new AStarPathRequest
            {
                RequesterId = groupId,
                StartPosition = pos,
                EndPosition = endChunkIndex
            });
        }
    }

    public static Dictionary<int, Queue<int>> GetPath(uint groupId)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(AStarPath), typeof(AStarPathRequester));

        Dictionary<int, Queue<int>> paths = new Dictionary<int, Queue<int>>();
        using (var entities = query.ToEntityArray(Allocator.Temp))
        {
            foreach (var entity in entities)
            {
                var aStarPathRequester = entityManager.GetComponentData<AStarPathRequester>(entity);
                if (aStarPathRequester.RequesterId == groupId)
                {
                    var path = entityManager.GetComponentData<AStarPath>(entity).Path.AsArray().ToArray();
                    paths.Add(path[0], new Queue<int>(path));
                }
            }
        }
        return paths;
    }

    public static void RequestFlowField(int targetChunkIndex, int nextChunkIndex, int cellPos, uint groupId)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var entity = entityManager.CreateEntity();
        entityManager.AddComponentData(entity, new FfRequest
        {
            RequesterId = groupId,
            ChunkIndex = targetChunkIndex,
            NextChunkIndex = nextChunkIndex,
            DestinationCellPos = cellPos,
        });
    }
    
    public static Vector3 RequestUnitDirection(Vector3 worldPos, uint groupId)
    {
        var FfBestDirectionsEntities = _FlowFieldQuery.ToEntityArray(Allocator.Temp);
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        foreach (var entity in FfBestDirectionsEntities)
        {
            var requester = entityManager.GetComponentData<FfRequester>(entity);
            if (requester.RequesterId != groupId)
                continue;
            var status = entityManager.GetComponentData<FfBestDirectionsStatus>(entity);
            if (!status.IsReady())
                continue;

            int chunkIndex = GridUtils.GetChunkFromPosition(worldPos, _GridQuery.GetSingleton<GridMeta>());
            if (status.UsedChunkIndex != chunkIndex)
                continue;

            var bestDirections = entityManager.GetComponentData<FfBestDirections>(entity);
            GridMeta gridMeta = _GridQuery.GetSingleton<GridMeta>();

            int cellIndex = GridUtils.GetCellInChunkFromPosition(worldPos, gridMeta);

            var bestDirection = bestDirections.Cells[cellIndex].BestDirection;
            return new Vector3(bestDirection.x, 0, bestDirection.z);
        }
        FfBestDirectionsEntities.Dispose();
        return Vector3.zero;
    }
    void OnDisable()
    {
    }


}
