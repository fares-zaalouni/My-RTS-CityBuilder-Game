using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    public static event Action<Vector3, Vector3, uint> OnMoveTo;

    private int _TerrainLayerMask;
    private int _UnitsMask;
    public List<UnitGroup> Groups;
    private UnitGroup _SelectedUnitGroup;

    void Awake()
    {
        if (Instance != null)
        {
            return;
        }
        Instance = this;
        Groups = new(10);
        _SelectedUnitGroup = new();
        Debug.Log("Selectted group id: " + _SelectedUnitGroup.Id);
    }
    void OnEnable()
    {
        GameManager.OnInGameLeftClick += SelectUnit;
    }


    void Start()
    {
        _TerrainLayerMask = LayerMask.GetMask("Terrain");
        _UnitsMask = LayerMask.GetMask("Units");
    }

    void Update()
    {

    }

    void OnDisable()
    {
        GameManager.OnInGameLeftClick -= SelectUnit;
    }

    private void SelectUnit(Vector2 mousePosition)
    {

        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, 100f, _UnitsMask))
        {

            GameObject hitObject = hitInfo.collider.gameObject;
            Unit unit = hitObject.GetComponent<Unit>();
            _SelectedUnitGroup.AddToGoup(unit);
            
        }
        else if (Physics.Raycast(ray, out var hitInfo2, 100f, _TerrainLayerMask) && _SelectedUnitGroup.Group.Count != 0)
        {
            OnMoveTo?.Invoke(_SelectedUnitGroup.Group[0].transform.position, hitInfo2.point, _SelectedUnitGroup.Id);
        }

    }


}
