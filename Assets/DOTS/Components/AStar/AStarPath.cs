

using Unity.Collections;
using Unity.Entities;

public partial struct AStarPath : IComponentData
{
    public NativeList<int> Path;
}