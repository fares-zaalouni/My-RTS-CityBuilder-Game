

using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


[BurstCompile]
public partial struct AStarInitJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<AStarNode> Nodes;
    [ReadOnly]
    public NativeList<int> TouchedNodes;

    [BurstCompile]
    public void Execute(int index)
    {
        int nodeIndex = TouchedNodes[index];
        AStarNode node = Nodes[nodeIndex];
        
        node.IsInClosedSet = false;
        node.ParentIndex = -1;
        node.HeapIndex = -1;
        Nodes[nodeIndex] = node;
    }
}

[BurstCompile]
public partial struct AStarPathFindingJob : IJob
{
    public NativeList<int> ResultPath;
    public int StartNodeIndex;
    public int EndNodeIndex;
    public int GridSizeX;
    public int GridSizeZ;
    public AStarNativeMinHeap OpenList;
    public NativeArray<int> Neighbours;
    [ReadOnly]
    public NativeArray<int3> Directions;
    int _DirectionsCount;
    public NativeList<int> TouchedNodes;


    [BurstCompile]
    public void Execute()
    {
        TouchedNodes.Clear();
        _DirectionsCount = Directions.Length;

        NativeArray<AStarNode> Nodes = OpenList.NodesArray;

        AStarNode startNode = Nodes[StartNodeIndex];

        int3 endNodePos = GetGridPos(EndNodeIndex);

        OpenList.Enqueue(startNode.Index, startNode.FCost, startNode.HCost);
        TouchedNodes.Add(startNode.Index);
        while (!OpenList.IsEmpty)
        {
            int currentNodeIndex = OpenList.Dequeue();
            AStarNode currentNode = Nodes[currentNodeIndex];
            currentNode.IsInClosedSet = true;
            Nodes[currentNodeIndex] = currentNode;
            if (currentNodeIndex == EndNodeIndex)
            {
                RetracePath(StartNodeIndex, EndNodeIndex, Nodes);
                OpenList.Clear();
                return;
            }
                
            GetNeighbours(currentNodeIndex, Neighbours);
            foreach (var neighbourIndex in Neighbours)
            {
                if (neighbourIndex == -1)
                    continue;

                AStarNode neighbourNode = Nodes[neighbourIndex];
                if (neighbourNode.IsInClosedSet)
                    continue;

                int3 currentPos = GetGridPos(currentNodeIndex);
                int3 neighbourPos = GetGridPos(neighbourIndex);

                uint newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentPos, neighbourPos);
                bool isInOpen = OpenList.Contains(neighbourNode.HeapIndex, neighbourIndex);
                if (newMovementCostToNeighbour < neighbourNode.GCost || !isInOpen)
                {
                    neighbourNode.GCost = newMovementCostToNeighbour;
                    neighbourNode.HCost = GetDistance(neighbourPos, endNodePos);
                    neighbourNode.ParentIndex = currentNodeIndex;

                    if (!isInOpen)
                    {
                        OpenList.Enqueue(neighbourNode.Index, neighbourNode.FCost, neighbourNode.HCost);
                    }
                    else
                        OpenList.InsertAt(neighbourNode.HeapIndex, neighbourNode.FCost, neighbourNode.HCost);

                    Nodes[neighbourIndex] = neighbourNode;
                    TouchedNodes.Add(neighbourNode.Index);
                }
            }

        }
        
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
        int min = math.min(distX, distZ);
        int max = math.max(distX, distZ);
        return (uint)(14 * min + 10 * (max - min));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RetracePath(int startNodeIndex, int endNodeIndex, NativeArray<AStarNode> Nodes)
    {
        NativeList<int> path = new NativeList<int>(Allocator.Temp)
        {
            endNodeIndex
        };
        while (endNodeIndex != startNodeIndex)
        {
            path.Add(Nodes[endNodeIndex].ParentIndex);
            endNodeIndex = Nodes[endNodeIndex].ParentIndex;
        }

        while (path.Length > 0)
        {
            ResultPath.Add(path[path.Length - 1]);
            path.RemoveAtSwapBack(path.Length - 1);
        }
    }
    
}