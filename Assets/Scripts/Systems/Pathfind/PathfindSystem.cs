using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.Scripts.Systems.Pathing
{

    [BurstCompile]
    partial struct PathfindSetJob : IJobEntity
    {
        [ReadOnly] public DestinationComponent destination;
        [ReadOnly] public WorldComponent worldData;


        [BurstCompile]
        public void Execute(ref PathfindComponent humanPosition, ref DynamicBuffer<PositionNode> nodes)
        {
            if (humanPosition.position.y != destination.level)
                return;

            


            NativeList<int> positions = new NativeList<int>(20, Allocator.Temp);
            PathfindingNew pathfindingNew = new()
            {
                destination = destination.pos,
                start =
                    ((int)math.floor(humanPosition.position.z)) * worldData.LargestLevelDimensionScaled +
                    ((int)math.floor(humanPosition.position.x)),
                level = destination.level,
                path = worldData.path,
                gridWidth = worldData.LargestLevelDimensionScaled,
                offsetTable = worldData.offsets,
                positions = positions
            };

            pathfindingNew.Execute();
            nodes.Clear();
            if (positions.Length == 0)
            {
                humanPosition.currentWaypoint = -1;
                return;
            }
            nodes.Capacity = positions.Length - 1;
            humanPosition.currentWaypoint = 0;
            for (int i = positions.Length-2; i >= 0; i--)
            {
                int2 pos = new int2(positions[i] % worldData.LargestLevelDimensionScaled, positions[i] / worldData.LargestLevelDimensionScaled);
                nodes.Add(new() { position = new(pos.x, 1, pos.y)});
            }
        }
    }

    [BurstCompile]
    public partial struct PathfindSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestinationComponent>();
            state.RequireForUpdate<WorldComponent>();
            state.Enabled = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var comp = SystemAPI.GetSingleton<DestinationComponent>();
            var world = SystemAPI.GetSingleton<WorldComponent>();


            PathfindSetJob pathfindSetJob = new()
            {
                destination = comp,
                worldData = world
            };

            state.Dependency = pathfindSetJob.ScheduleByRef(state.Dependency);

            state.Enabled = false;
        }
    }
}
