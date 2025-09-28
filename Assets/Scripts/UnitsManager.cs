using System.Collections.Generic;
using UnityEngine;

public class UnitsManager : MonoBehaviour
{
    public static List<Unit> AllUnits;

    void Awake()
    {
        AllUnits = new(20);
    }

    
}
