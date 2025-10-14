

using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;

[BurstCompile]
public partial struct AStarPathFindingJob : IJob
{
    public NativeArray<AStarNode> Nodes;
    public int StartNodeIndex;
    public int EndNodeIndex;
    public int GridSizeX;
    public int GridSizeZ;
    public readonly NativeArray<int3> Directions;
    byte _DirectionsCount;

    [BurstCompile]
    public void Execute()
    {

       /* Directions = new NativeArray<int3>(8, Allocator.TempJob);
        Directions[0] = new int3(0, 0, 1); // North
        Directions[1] = new int3(1, 0, 0); // East
        Directions[2] = new int3(0, 0, -1); // South
        Directions[3] = new int3(-1, 0, 0); // West
        Directions[4] = new int3(1, 0, 1); // NorthEast
        Directions[5] = new int3(1, 0, -1); // SouthEast
        Directions[6] = new int3(-1, 0, -1); // SouthWest
        Directions[7] = new int3(-1, 0, 1); // NorthWest*/

        _DirectionsCount = (byte)Directions.Length;

        AStarNativeMinHeap openList = new AStarNativeMinHeap(Nodes.Count(), Nodes, Allocator.TempJob);
        NativeArray<int> neighbours = new NativeArray<int>(8, Allocator.TempJob);
        AStarNode startNode = Nodes[StartNodeIndex];
        AStarNode endNode = Nodes[EndNodeIndex];

        int3 endNodePos = GetGridPos(EndNodeIndex);

        openList.Enqueue(startNode.Index, startNode.FCost, startNode.HCost);
        while (!openList.IsEmpty)
        {
            int currentNodeIndex = openList.Dequeue();
            AStarNode currentNode = Nodes[currentNodeIndex];
            currentNode.IsInClosedSet = true;
            Nodes[currentNodeIndex] = currentNode;
            if (currentNodeIndex == EndNodeIndex)
                return;

            GetNeighbours(currentNodeIndex, neighbours);
            foreach (var neighbourIndex in neighbours)
            {
                if (neighbourIndex == -1)
                    continue;

                AStarNode neighbourNode = Nodes[neighbourIndex];
                if (neighbourNode.IsInClosedSet)
                    continue;

                int3 currentPos = GetGridPos(currentNodeIndex);
                int3 neighbourPos = GetGridPos(neighbourIndex);

                uint newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentPos, neighbourPos);
                bool isInOpen = openList.Contains(neighbourNode.HeapIndex, neighbourIndex);
                if (newMovementCostToNeighbour < neighbourNode.GCost || !isInOpen)
                {
                    neighbourNode.GCost = newMovementCostToNeighbour;
                    neighbourNode.HCost = GetDistance(neighbourPos, endNodePos);
                    neighbourNode.ParentIndex = currentNodeIndex;

                    if (!isInOpen)
                        openList.Enqueue(neighbourNode.Index, neighbourNode.FCost, neighbourNode.HCost);
                    else
                        openList.InsertAt(neighbourNode.HeapIndex, neighbourNode.FCost, neighbourNode.HCost);

                    Nodes[neighbourIndex] = neighbourNode;
                }
            }

        }
        openList.Dispose();
        neighbours.Dispose();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetNeighbours(int index, NativeArray<int> neighboursIndexes)
    {

        int3 pos = GetGridPos(index);

        for (int i = 0; i < _DirectionsCount; i++)
        {
            int3 neighbourPos = pos + Directions[i];
            if (neighbourPos.x < 0 || neighbourPos.x >= GridSizeX || neighbourPos.z < 0 || neighbourPos.z >= GridSizeZ)
            {
                neighboursIndexes[i] = -1;
                continue;
            }

            int neighbourIndex = neighbourPos.z * GridSizeX + neighbourPos.x;
            neighboursIndexes[i] = neighbourIndex;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int3 GetGridPos(int index)
    {
        int posX = index % GridSizeX;
        int posZ = index / GridSizeX;
        return new int3(posX, 0, posZ);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetDistance(int3 posA, int3 posB)
    {
        int distX = math.abs(posA.x - posB.x);
        int distZ = math.abs(posA.z - posB.z);

        if (distX > distZ)
            return (uint)(14 * distZ + 10 * distX);
        return (uint)(14 * distX + 10 * distZ); 
    }
    
}