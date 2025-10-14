

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
public partial struct InitFlowFieldJob : IJobParallelFor
{
    const float DIAGONAL_MULTIPLIER = 1.414f;

    public NativeArray<FfCellBestCost> BestCosts;
    [ReadOnly] public NativeArray<FfCellData> CellsData;
    [ReadOnly] public GridMeta GridMeta;
    public NativeArray<FfNeighboursCost> NeighboursCosts;
    [ReadOnly] public NativeArray<int3> Directions;

    [BurstCompile]
    public void Execute(int index)
    {

        BestCosts[index] = new FfCellBestCost { BestCost = uint.MaxValue };
        var cellData = CellsData[index];
        byte north = byte.MaxValue;
        byte east = byte.MaxValue;
        byte south = byte.MaxValue;
        byte west = byte.MaxValue;
        byte northEast = byte.MaxValue;
        byte southEast = byte.MaxValue;
        byte southWest = byte.MaxValue;
        byte northWest = byte.MaxValue;

        int northPos = -1;
        int eastPos = -1;
        int southPos = -1;
        int westPos = -1;
        int northEastPos = -1;
        int southEastPos = -1;
        int southWestPos = -1;
        int northWestPos = -1;

        foreach (var dir in Directions)
        {
            int3 neighborPos = cellData.GridPos + dir;
            if (neighborPos.x < 0 ||
                neighborPos.z < 0 ||
                neighborPos.x >= GridMeta.CellsInChunkRow ||
                neighborPos.z >= GridMeta.CellsInChunkRow)
                continue;


            int neighborIndex = neighborPos.x + neighborPos.z * GridMeta.CellsInChunkRow;
            var neighborCell = CellsData[neighborIndex];
            if (!neighborCell.Walkable)
                continue;
            if (dir.x == 0 && dir.z == 1)
            {
                north = neighborCell.Cost;
                northPos = neighborIndex;
            }
            else if (dir.x == 1 && dir.z == 0)
            {
                east = neighborCell.Cost;
                eastPos = neighborIndex;
            }
            else if (dir.x == 0 && dir.z == -1)
            {
                south = neighborCell.Cost;
                southPos = neighborIndex;
            }
            else if (dir.x == -1 && dir.z == 0)
            {
                west = neighborCell.Cost;
                westPos = neighborIndex;
            }
            else if (dir.x == 1 && dir.z == 1)
            {
                northEast = (byte)math.clamp(neighborCell.Cost * DIAGONAL_MULTIPLIER,
                        math.max(2, neighborCell.Cost + 1), 244);
                northEastPos = neighborIndex;
            }
            else if (dir.x == 1 && dir.z == -1)
            {
                southEast = (byte)math.clamp(neighborCell.Cost * DIAGONAL_MULTIPLIER,
                        math.max(2, neighborCell.Cost + 1), 244);
                southEastPos = neighborIndex;
            }
            else if (dir.x == -1 && dir.z == -1)
            {
                southWest = (byte)math.clamp(neighborCell.Cost * DIAGONAL_MULTIPLIER,
                        math.max(2, neighborCell.Cost + 1), 244);
                southWestPos = neighborIndex;
            }
            else if (dir.x == -1 && dir.z == 1)
            {
                northWest = (byte)math.clamp(neighborCell.Cost * DIAGONAL_MULTIPLIER,
                        math.max(2, neighborCell.Cost + 1), 244);
                northWestPos = neighborIndex;
            }


        }
        NeighboursCosts[index] = new FfNeighboursCost
        {
            North = north,
            East = east,
            South = south,
            West = west,
            NorthEast = northEast,
            SouthEast = southEast,
            SouthWest = southWest,
            NorthWest = northWest,
            NorthPos = northPos,
            EastPos = eastPos,
            SouthPos = southPos,
            WestPos = westPos,
            NorthEastPos = northEastPos,
            SouthEastPos = southEastPos,
            SouthWestPos = southWestPos,
            NorthWestPos = northWestPos
        };
    }
}
[BurstCompile]
public partial struct CalculateIntegrationFieldUniformJob : IJob
{
    public NativeArray<FfCellBestCost> CellsBestCosts;
    public GridMeta GridMeta;
    public int destIndex;
    [ReadOnly] public NativeArray<FfNeighboursCost> NeighboursCosts;
    [ReadOnly] public NativeArray<int3> Directions;


    [BurstCompile]
    public void Execute()
    {
        NativeQueue<int> OpenList = new NativeQueue<int>(Allocator.Persistent);
        CellsBestCosts[destIndex] = new FfCellBestCost { BestCost = 0 };
        OpenList.Enqueue(destIndex);
        while (!OpenList.IsEmpty())
        {
            int currentIndex = OpenList.Dequeue();
            var currentCellBestCost = CellsBestCosts[currentIndex].BestCost;

            FfNeighboursCost neighboursCost = NeighboursCosts[currentIndex];

            byte northCost = neighboursCost.North;
            byte eastCost = neighboursCost.East;
            byte southCost = neighboursCost.South;
            byte westCost = neighboursCost.West;
            byte northEastCost = neighboursCost.NorthEast;
            byte southEastCost = neighboursCost.SouthEast;
            byte southWestCost = neighboursCost.SouthWest;
            byte northWestCost = neighboursCost.NorthWest;

            if (northCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.NorthPos;
                uint newCost = currentCellBestCost + neighboursCost.North;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex);
                }
            }
            if (eastCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.EastPos;
                uint newCost = currentCellBestCost + neighboursCost.East;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex);
                }
            }
            if (southCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.SouthPos;
                uint newCost = currentCellBestCost + neighboursCost.South;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex);
                }
            }
            if (westCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.WestPos;
                uint newCost = currentCellBestCost + neighboursCost.West;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex);
                }
            }
            if (northEastCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.NorthEastPos;
                uint newCost = currentCellBestCost + neighboursCost.NorthEast;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex);
                }
            }
            if (southEastCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.SouthEastPos;
                uint newCost = currentCellBestCost + neighboursCost.SouthEast;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex);
                }
            }
            if (southWestCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.SouthWestPos;
                uint newCost = currentCellBestCost + neighboursCost.SouthWest;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex);
                }
            }
            if (northWestCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.NorthWestPos;
                uint newCost = currentCellBestCost + neighboursCost.NorthWest;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex);
                }
            }
        }
        OpenList.Dispose();
    }
}

[BurstCompile]
public partial struct CalculateIntegrationFieldJob : IJob
{
    public NativeArray<FfCellBestCost> CellsBestCosts;
    public NativeMinHeap OpenList;
    public GridMeta GridMeta;
    public int destIndex;
    [ReadOnly] public NativeArray<FfNeighboursCost> NeighboursCosts;
    [ReadOnly] public NativeArray<int3> Directions;


    [BurstCompile]
    public void Execute()
    {
        CellsBestCosts[destIndex] = new FfCellBestCost { BestCost = 0 };
        OpenList.Enqueue(destIndex, 0);
        while (!OpenList.IsEmpty)
        {
            int currentIndex = OpenList.Dequeue();
            var currentCellBestCost = CellsBestCosts[currentIndex].BestCost;

            FfNeighboursCost neighboursCost = NeighboursCosts[currentIndex];
            byte northCost = neighboursCost.North;
            byte eastCost = neighboursCost.East;
            byte southCost = neighboursCost.South;
            byte westCost = neighboursCost.West;
            byte northEastCost = neighboursCost.NorthEast;
            byte southEastCost = neighboursCost.SouthEast;
            byte southWestCost = neighboursCost.SouthWest;
            byte northWestCost = neighboursCost.NorthWest;

            if (northCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.NorthPos;
                uint newCost = currentCellBestCost + neighboursCost.North;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex, newCost);
                }
            }
            if (eastCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.EastPos;
                uint newCost = currentCellBestCost + neighboursCost.East;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex, newCost);
                }
            }
            if (southCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.SouthPos;
                uint newCost = currentCellBestCost + neighboursCost.South;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex, newCost);
                }
            }
            if (westCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.WestPos;
                uint newCost = currentCellBestCost + neighboursCost.West;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex, newCost);
                }
            }
            if (northEastCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.NorthEastPos;
                uint newCost = currentCellBestCost + neighboursCost.NorthEast;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex, newCost);
                }
            }
            if (southEastCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.SouthEastPos;
                uint newCost = currentCellBestCost + neighboursCost.SouthEast;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex, newCost);
                }
            }
            if (southWestCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.SouthWestPos;
                uint newCost = currentCellBestCost + neighboursCost.SouthWest;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex, newCost);
                }
            }
            if (northWestCost != byte.MaxValue)
            {
                int neighborIndex = neighboursCost.NorthWestPos;
                uint newCost = currentCellBestCost + neighboursCost.NorthWest;
                if (newCost < CellsBestCosts[neighborIndex].BestCost)
                {
                    CellsBestCosts[neighborIndex] = new FfCellBestCost { BestCost = newCost };
                    OpenList.Enqueue(neighborIndex, newCost);
                }
            }
        }

    }
}
[BurstCompile]
public partial struct CalculateBestDirectionJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<FfCellBestCost> CellsBestCosts;
    [ReadOnly] public NativeArray<int3> Directions;
    public NativeArray<FfCellBestDirection> CellsBestDirections;
    public GridMeta GridMeta;

    [BurstCompile]
    public void Execute(int index)
    {
        var cellBestCost = CellsBestCosts[index];
        if (cellBestCost.BestCost == uint.MaxValue)
        {
            CellsBestDirections[index] = new FfCellBestDirection { BestDirection = float3.zero };
            return;
        }

        int3 cellPos = new int3(index % GridMeta.CellsInChunkRow, 0, index / GridMeta.CellsInChunkRow);

        float3 bestDirection = float3.zero;
        uint lowestCost = cellBestCost.BestCost;

        foreach (var dir in Directions)
        {
            int3 neighborPos = cellPos + dir;
            if (neighborPos.x < 0 || neighborPos.x >= GridMeta.CellsInChunkRow || neighborPos.z < 0 || neighborPos.z >= GridMeta.CellsInChunkRow)
                continue;

            int neighborIndex = neighborPos.x + neighborPos.z * GridMeta.CellsInChunkRow;
            var neighborBestCost = CellsBestCosts[neighborIndex];

            if (neighborBestCost.BestCost < lowestCost)
            {
                lowestCost = neighborBestCost.BestCost;
                bestDirection = dir;
            }
        }

        CellsBestDirections[index] = new FfCellBestDirection { BestDirection = bestDirection };
    }
}

