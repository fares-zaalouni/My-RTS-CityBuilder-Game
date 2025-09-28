using System;
using Unity.Burst;
using Unity.Collections;


[BurstCompile]
public struct NativeMinHeap : IDisposable
{
    public NativeArray<int> Indices;
    public NativeArray<uint> Keys; // store the current best costs
    public int Count;

    public NativeMinHeap(int capacity, Allocator allocator)
    {
        Indices = new NativeArray<int>(capacity, allocator);
        Keys = new NativeArray<uint>(capacity, allocator);
        Count = 0;
    }

    public void Dispose()
    {
        if (Indices.IsCreated) Indices.Dispose();
        if (Keys.IsCreated) Keys.Dispose();
    }

    public void Clear()
    {
        Count = 0;
    }

    [BurstCompile]
    public void Enqueue(int index, uint key)
    {
        int i = Count++;
        Indices[i] = index;
        Keys[i] = key;
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
            Keys[0] = Keys[Count];
            HeapifyDown(0);
        }
        return result;
    }

    public bool IsEmpty => Count == 0;
    [BurstCompile]
    private void HeapifyUp(int i)
    {
        int parent = (i - 1) / 2;
        while (i > 0 && Keys[i] < Keys[parent])
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

            if (left < Count && Keys[left] < Keys[smallest]) smallest = left;
            if (right < Count && Keys[right] < Keys[smallest]) smallest = right;

            if (smallest == i) break;

            Swap(i, smallest);
            i = smallest;
        }
    }
    [BurstCompile]
    private void Swap(int i, int j)
    {
        int tmpIndex = Indices[i];
        Indices[i] = Indices[j];
        Indices[j] = tmpIndex;

        uint tmpKey = Keys[i];
        Keys[i] = Keys[j];
        Keys[j] = tmpKey;
    }
}

