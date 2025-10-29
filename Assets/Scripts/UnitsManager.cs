using System.Collections.Generic;
using UnityEngine;

public class UnitsManager : MonoBehaviour
{
    public static UnitsManager Instance;
    public static List<GameObject> AllUnits;
    public static Vector3 destination;
    public static Dictionary<int, Queue<int>> CurrentPath;
    public static HashSet<int> CalculatedChunks;
    public static uint groupId = 1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CalculatedChunks = new HashSet<int>();
        CurrentPath = new Dictionary<int, Queue<int>>();
        AllUnits = new(20);
    }
    void Update()
    {
        foreach (var unit in AllUnits)
        {
            Vector3 direction = ECSMovementAPI.RequestUnitDirection(unit.transform.position, groupId);
            unit.GetComponent<CharacterController>().Move(3 * Time.deltaTime * direction);
        }
    }
}
