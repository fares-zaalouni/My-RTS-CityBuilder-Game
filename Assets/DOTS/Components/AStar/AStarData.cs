

using Unity.Collections;
using Unity.Entities;

public partial struct AStarData : IComponentData
{
    public NativeArray<AStarNode> Nodes;
    public AStarNativeMinHeap OpenList;
    public NativeList<int> TouchedNodes;
}