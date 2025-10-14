public struct AStarNode
{
    public uint GCost;
    public uint HCost;
    public readonly uint FCost => GCost + HCost;
    public int Index;
    public int ParentIndex;
    public bool IsInClosedSet;
    public int HeapIndex;
}