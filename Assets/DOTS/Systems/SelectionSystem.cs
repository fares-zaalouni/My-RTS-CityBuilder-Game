
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
partial struct SelectionSystem : ISystem
{/*
    private short _NextGroupId;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _NextGroupId = 1;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (clickCommand, entity) in SystemAPI.Query<RefRO<ClickCommand>>().WithEntityAccess())
        {
            ProcessClickCommand(clickCommand.ValueRO, ref state);
            ecb.DestroyEntity(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void ProcessClickCommand(ClickCommand command, ref SystemState state)
    {

        GridMeta gridMeta = SystemAPI.GetSingleton<GridMeta>();

        int index = GridUtils.GetCellFromPosition(command.Pos, gridMeta);
        
        foreach (var (gridDynamic, ffStateData, lastDestination) in SystemAPI.Query<
        RefRW<FfGridData>,
        RefRW<FfStateData>,
        RefRW<FfDestination>>())
        {

            if (index < 0 || index >= gridDynamic.ValueRO.Cells.Length)
                return;
            if (lastDestination.ValueRO.Index == index)
                return;
            if (lastDestination.ValueRO.Index != -1)
            {
                var lastCell = gridDynamic.ValueRO.Cells[lastDestination.ValueRO.Index];
                lastCell.Cost = lastDestination.ValueRO.OriginalCost;
                gridDynamic.ValueRW.Cells[lastDestination.ValueRO.Index] = lastCell;
            }

            lastDestination.ValueRW.Index = index;
            lastDestination.ValueRW.OriginalCost = gridDynamic.ValueRO.Cells[index].Cost;
            lastDestination.ValueRW.AssignedGroup = command.AssignedGroup;

            var cell = gridDynamic.ValueRO.Cells[index];
            cell.Cost = 0;
            gridDynamic.ValueRW.Cells[index] = cell;
            if (ffStateData.ValueRO.State == FfStateData.FlowFieldSate.Ready)
                ffStateData.ValueRW.State = FfStateData.FlowFieldSate.Available;
        }

    }
   
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }*/
}
