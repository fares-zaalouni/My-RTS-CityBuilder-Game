

using Unity.Collections;
using Unity.Entities;

public partial struct AStarPath : IComponentData
{
    public NativeArray<AStarNode> Path;
}