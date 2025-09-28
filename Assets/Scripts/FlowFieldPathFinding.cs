using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FlowFieldPathFinding : MonoBehaviour
{
    public List<Unit> Units;

    public enum ActionA
    {
        Remove,
        Add
    }
    public static Queue<(Unit unit, ActionA action)> ActionQueue;
    public static void RegisterUnit(Unit unit, ActionA action)
    {
        ActionQueue ??= new(5);

        ActionQueue.Enqueue((unit, action));
    }
    public static void UnregisterUnit(Unit unit, ActionA action)
    {
        ActionQueue ??= new(5);

        ActionQueue.Enqueue((unit, action));
    }

    void OnEnable()
    {
        UnitSelectionManager.OnMoveTo += MoveTo;
        InputManager.OnRightClick += CancelPathFinding;
    }

    private void MoveTo(Vector3 pos, List<Unit> list)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entity = entityManager.CreateEntity();
        entityManager.AddComponentData(entity, new ClickCommand
        {
            Pos = pos,
            CameraPos = Camera.main.transform.position
        });
        Units = list;
    }

    void Awake()
    {
        Units = new(10);

    }

    void Start()
    {

    }

    void Update()
    {
        if (Units.Count == 0)
            return;
        RequestCurrentDirection();
    }

    void LateUpdate()
    {
        ProcessMovement();

        //ProcessUnitQueue();
    }

    private void ProcessUnitQueue()
    {
        while (ActionQueue.Count != 0)
        {
            var temp = ActionQueue.Dequeue();
            if (temp.action == ActionA.Add)
            {
                Units.Add(temp.unit);
            }
            else if (temp.action == ActionA.Remove)
            {
                Units.Remove(temp.unit);
            }
        }
    }
    private void RequestCurrentDirection()
    {
        for (int i = 0; i < Units.Count; i++)
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new UnitDirectionRequest
            {
                Index = i,
                WorldPos = Units[i].transform.position
            });
        }
    }

    private void ProcessMovement()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(UnitDirectionResponse));
        var entities = query.ToEntityArray(Allocator.Temp);
        var directions = query.ToComponentDataArray<UnitDirectionResponse>(Allocator.Temp);

        query = entityManager.CreateEntityQuery(typeof(FfDestination));
        //FfDestination query

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var directionComponent in directions)
        {
            float3 direction = directionComponent.Direction;
            if (direction.Equals(float3.zero))
            {
                continue;
            }
            Vector3 normalizedDirecion = new Vector3(direction.x, 0, direction.z).normalized;
            Unit unit = Units[directionComponent.Index];
            Vector3 velociy = Time.deltaTime * unit.MovementSpeed * normalizedDirecion;

            unit.transform.Translate(velociy);
            ecb.DestroyEntity(entities);
        }
        ecb.Playback(entityManager);
    }

    void OnDisable()
    {
        UnitSelectionManager.OnMoveTo -= MoveTo;
    }





    void CancelPathFinding(Vector2 vec)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(FfCancelationToken));
        var cancelationToken = query.ToComponentDataArray<FfCancelationToken>(Allocator.Temp);
        var reff = cancelationToken[0];
        reff.Token.Value = true;
    }
}
