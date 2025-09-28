using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    public static event Action<Vector3, List<Unit>> OnMoveTo;

    private int _TerrainLayerMask;
    private int _UnitsMask;
    public List<UnitGroup> Groups;
    private UnitGroup _SelectedUnitGroup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        Instance = this;
        Groups = new(10);
        _SelectedUnitGroup = new();
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
            _SelectedUnitGroup.Group.Add(unit);
            var unitRenderer = hitObject.GetComponent<Renderer>();
            if (unitRenderer != null)
            {
                unitRenderer.material.color = Color.green;
            }
        }
        else if (Physics.Raycast(ray, out var hitInfo2, 100f, _TerrainLayerMask) && _SelectedUnitGroup.Group.Count != 0)
        {
            OnMoveTo?.Invoke(hitInfo2.point, _SelectedUnitGroup.Group);
        }

    }


}
