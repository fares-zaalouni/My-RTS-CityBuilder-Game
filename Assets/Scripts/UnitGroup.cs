using System.Collections.Generic;
using UnityEngine;

public class UnitGroup
{
    public byte Id { get; private set; }
    public List<Unit> Group { get; private set; }
    private static Queue<byte> _AvailableIds = new Queue<byte>();
    private static byte NextId = 1;
    private static byte GetNextId => NextId++;

    public Color Color;
    public UnitGroup()
    {
        Id = _AvailableIds.Count != 0 ? _AvailableIds.Dequeue() : GetNextId;
        Group = new List<Unit>(10);
        Color = new Color(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255));
    }


    public void Release()
    {
        _AvailableIds.Enqueue(Id);
    }
}
