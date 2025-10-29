

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public partial struct AStarPath : IComponentData
{
    public NativeList<int> Path;
}

public partial struct AStarPathRequest : IComponentData
{
    public int StartPosition;
    public int EndPosition;
    public uint RequesterId;
}


public partial struct AStarPathRequester : IComponentData
{
    public uint RequesterId;
}

public partial struct AStarPathStatus : IComponentData
{
    public bool PathFound;
    public bool IsDeprecated;
    public JobHandle PathJobHandle;

    public bool IsCompleted()
    {
        if (PathJobHandle.IsCompleted)
        {
            PathJobHandle.Complete();
            return true;
        }
        return false;
    }
}