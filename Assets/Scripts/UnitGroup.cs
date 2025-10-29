using System.Collections.Generic;
using UnityEngine;

public class UnitGroup
{
    public byte Id { get; private set; }
    public List<Unit> Group { get; private set; }
    public List<int> PathIndices {get; set; }
    private static Queue<byte> _AvailableIds = new Queue<byte>();
    private static byte _NextId = 1;
    private static byte _GetNextId => _NextId++;

    public Color Color;
    public UnitGroup()
    {
        Id = _AvailableIds.Count != 0 ? _AvailableIds.Dequeue() : _GetNextId;
        Group = new List<Unit>(10);
        Color = Random.ColorHSV();
    }

    public void AddToGoup(Unit unit)
    {
        Group.Add(unit);
        var unitRenderer = unit.GetComponent<Renderer>();
        if (unitRenderer != null)
        {
            unitRenderer.material.color = Color;
        }
    }
    public void Release()
    {
        _AvailableIds.Enqueue(Id);
    }
}
