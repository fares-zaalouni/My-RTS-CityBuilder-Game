using Unity.Entities;


public struct FfDestination : IComponentData
{
    public int Index;
    public byte OriginalCost;
    public byte AssignedGroup;
}
