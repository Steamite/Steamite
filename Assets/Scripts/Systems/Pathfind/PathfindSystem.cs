using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.Scripts.Systems.Pathing
{

    partial struct PathfindSetJob : IJobEntity
    {
        [ReadOnly] public DestinationComponent destination;
        public void Execute(ref PathfindComponent path, ref DynamicBuffer<PositionNode> nodes)
        {
            NativeArray<int> offsets = new NativeArray<int>(4, Allocator.Temp);
            offsets[0] = 1;
            offsets[1] = destination.width;
            offsets[2] = -1;
            offsets[3] = -destination.width;


            NativeList<int> positions = new NativeList<int>(10, Allocator.Temp);
            PathfindingNew pathfindingNew = new()
            {
                destination = destination.pos,
                start =
                    ((int)math.floor(path.position.z) - destination.offset) * destination.width +
                    ((int)math.floor(path.position.x) - destination.offset),
                grids = destination.grid,
                gridWidth = destination.width,
                offsetTable = offsets,
                positions = positions
            };

            pathfindingNew.Execute();
            nodes.Clear();
            if(positions.Length == 0)
            {
                path.currentWaypoint = -1;
                return;
            }

            path.currentWaypoint = 0;
            for (int i = positions.Length-2; i >= 0; i--)
            {
                int2 pos = new int2(positions[i] % destination.width, positions[i] / destination.width) + destination.offset;
                //int posFixed = pos.y * destination.width + pos.x * destination.width;
                nodes.Add(new() { position = new(pos.x, 1, pos.y)});
            }
        }
    }

    public partial struct PathfindSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestinationComponent>();
            state.Enabled = false;
        }

        public void OnUpdate(ref SystemState state)
        {
            var en = SystemAPI.GetSingletonEntity<DestinationComponent>();
            var comp = SystemAPI.GetComponent<DestinationComponent>(en);


            PathfindSetJob pathfindSetJob = new()
            {
                destination = comp
            };

            state.Dependency = pathfindSetJob.ScheduleByRef(state.Dependency);

            state.Enabled = false;
        }
    }
}
