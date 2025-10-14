using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities.UniversalDelegates;

[BurstCompile]
public struct AStarNativeMinHeap : IDisposable
{
    public NativeArray<int> Indices;
    public NativeArray<uint> FCosts;
    public NativeArray<uint> HCosts;
    public int Count;
    public NativeArray<AStarNode> NodesArray;


    public AStarNativeMinHeap(int capacity, NativeArray<AStarNode> nodesArray, Allocator allocator)
    {
        Indices = new NativeArray<int>(capacity, allocator);
        FCosts = new NativeArray<uint>(capacity, allocator);
        HCosts = new NativeArray<uint>(capacity, allocator);
        NodesArray = nodesArray;
        Count = 0;
    }

    public void Dispose()
    {
        if (Indices.IsCreated) Indices.Dispose();
        if (FCosts.IsCreated) FCosts.Dispose();
        if (HCosts.IsCreated) HCosts.Dispose();
    }

    public void Clear()
    {
        Count = 0;
    }

    [BurstCompile]
    public bool Contains(int heapIndex, int gridIndex)
    {
        return heapIndex != -1 && Indices[heapIndex] == gridIndex;
    }
    [BurstCompile]
    public void InsertAt(int heapIndex, uint fCost, uint hCost)
    {
        FCosts[heapIndex] = fCost;
        HCosts[heapIndex] = hCost;
        HeapifyUp(heapIndex);
        HeapifyDown(heapIndex);
        
    }
    [BurstCompile]
    public void Enqueue(int index, uint fCost, uint hCost)
    {
        int i = Count++;
        AStarNode node = NodesArray[index];
        node.HeapIndex = i;
        NodesArray[index] = node;
        Indices[i] = index;
        FCosts[i] = fCost;
        HCosts[i] = hCost;
        HeapifyUp(i);
    }

    [BurstCompile]
    public int Dequeue()
    {
        int result = Indices[0];
        Count--;
        if (Count > 0)
        {
            Indices[0] = Indices[Count];
            FCosts[0] = FCosts[Count];
            HCosts[0] = HCosts[Count];
            HeapifyDown(0);
            AStarNode node = NodesArray[Indices[0]];
            node.HeapIndex = 0;
            NodesArray[Indices[0]] = node;
        }
        return result;
    }

    public bool IsEmpty => Count == 0;

    [BurstCompile]
    private void HeapifyUp(int i)
    {
        int parent = (i - 1) / 2;
        while (i > 0 && Compare(i, parent) < 0)
        {
            Swap(i, parent);
            i = parent;
            parent = (i - 1) / 2;
        }
    }

    [BurstCompile]
    private void HeapifyDown(int i)
    {
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;

            if (left < Count && Compare(left, smallest) < 0) smallest = left;
            if (right < Count && Compare(right, smallest) < 0) smallest = right;

            if (smallest == i) break;

            Swap(i, smallest);
            i = smallest;
        }
    }

    [BurstCompile]
    private int Compare(int a, int b)
    {
        uint fA = FCosts[a];
        uint fB = FCosts[b];

        if (fA < fB) return -1;
        if (fA > fB) return 1;

        // Tie-breaker: smaller H-cost wins
        uint hA = HCosts[a];
        uint hB = HCosts[b];
        if (hA < hB) return -1;
        if (hA > hB) return 1;

        return 0;
    }

    [BurstCompile]
    private void Swap(int i, int j)
    {
        int tmpIndex = Indices[i];
        Indices[i] = Indices[j];
        Indices[j] = tmpIndex;

        uint tmpF = FCosts[i];
        FCosts[i] = FCosts[j];
        FCosts[j] = tmpF;

        uint tmpH = HCosts[i];
        HCosts[i] = HCosts[j];
        HCosts[j] = tmpH;

        var nodeI = NodesArray[Indices[i]];
        nodeI.HeapIndex = i;
        NodesArray[Indices[i]] = nodeI;

        var nodeJ = NodesArray[Indices[j]];
        nodeJ.HeapIndex = j;
        NodesArray[Indices[j]] = nodeJ;
    }
}
