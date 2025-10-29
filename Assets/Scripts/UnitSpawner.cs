using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] GameObject UnitPrefab;
    [SerializeField] Transform ParentTransform;
    [SerializeField] float SpawnRadius = 3f;

    private EntityQuery _GridQuery;
    void Awake()
    {
        InputManager.OnSpawnUnit += SpawnUnit;
        InputManager.OnRightClick += RequestPath;
        InputManager.OnLeftClick += GetPath;
        InputManager.OnFlowFieldRequest += RequestNextFlowField;

        _GridQuery = World.DefaultGameObjectInjectionWorld
                                    .EntityManager
                                    .CreateEntityQuery(typeof(GridMeta));
    }
    void OnDestroy()
    {
        InputManager.OnSpawnUnit -= SpawnUnit;
        InputManager.OnRightClick -= RequestPath;
        InputManager.OnLeftClick -= GetPath;
        InputManager.OnFlowFieldRequest -= RequestNextFlowField;
    }

    void GetPath(Vector2 trash)
    {
        var path = ECSMovementAPI.GetPath(UnitsManager.groupId);
        UnitsManager.CurrentPath = path;
    }
    void RequestPath(Vector2 dest)
    {
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(dest), out hit, 100f, LayerMask.GetMask("Terrain")))
            return;

        Vector3 destination = new Vector3(hit.point.x, 0, hit.point.z);
        UnitsManager.destination = destination;
        ECSMovementAPI.RequestPath( destination,
                                    UnitsManager.AllUnits.Select(go => go.GetComponent<Unit>()).ToList(),
                                    UnitsManager.groupId);
    }

    void RequestNextFlowField()
    {
        if (UnitsManager.CurrentPath.Count == 0)
            return;

        var keys = UnitsManager.CurrentPath.Keys.ToList();
        HashSet<(int current, int next)> flowFieldsRequested = new HashSet<(int current, int next)>();

        foreach (var key in keys)
        {
            if (!UnitsManager.CurrentPath.TryGetValue(key, out var path))
                continue;

            if (path.Count == 0)
                continue;

            int currentChunkPosition = path.Dequeue();
            int nextChunkPosition = path.Count > 0 ? path.Peek() : -1;

            if (flowFieldsRequested.Contains((currentChunkPosition, nextChunkPosition)) ||
                UnitsManager.CalculatedChunks.Contains(currentChunkPosition))
                continue;

            if (nextChunkPosition != -1)
            {
                UnitsManager.CurrentPath.Remove(key);
                UnitsManager.CurrentPath.TryAdd(nextChunkPosition, path);
            }

            flowFieldsRequested.Add((currentChunkPosition, nextChunkPosition));
        }

        foreach (var (current, next) in flowFieldsRequested)
        {
            int cellPosition = GridUtils.ClumpCellToChunk(UnitsManager.destination, current, _GridQuery.GetSingleton<GridMeta>());
            ECSMovementAPI.RequestFlowField(current, next, cellPosition, UnitsManager.groupId);
            UnitsManager.CalculatedChunks.Add(current);
        }
    }
    
    void SpawnUnit()
    {
        Vector3 spawnPosition = transform.position + Random.insideUnitSphere * SpawnRadius;
        spawnPosition.y = 0.5f;
        UnitsManager.AllUnits.Add(Instantiate(UnitPrefab, spawnPosition, Quaternion.identity, ParentTransform));
    }
}
