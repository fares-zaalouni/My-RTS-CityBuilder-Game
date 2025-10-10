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
   

    private void ProcessMovement()
    {
        
    }

    void OnDisable()
    {
        UnitSelectionManager.OnMoveTo -= MoveTo;
    }


}
