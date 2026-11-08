using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Humans;
using Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace Assets.Scripts.Systems.Human.Jobs
{
    public partial struct DigSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HumanJobData>();
            state.RequireForUpdate<WorldComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ent = SystemAPI.GetSingletonEntity<WorldComponent>();
            var world = SystemAPI.GetComponent<WorldComponent>(ent);
            var levels = SystemAPI.GetBuffer<GridLevelComponent>(ent);

            foreach(var jobData in SystemAPI.Query<RefRW<HumanJobData>>())
            {
                if (jobData.ValueRO.jobType == JobType.Free)
                    continue;

                if (jobData.ValueRO.jobType == JobType.Dig)
                {
                    var grid = levels[jobData.ValueRO.targetLevel].renderGrid;
                    var tile = grid[jobData.ValueRO.targetPosition];
                    
                    tile.RemoveDurability(1);
                    if(tile.TType == TileType.Road)
                    {
                        // update roads
                        //world.path
                    }
                    grid[jobData.ValueRO.targetPosition] = tile;
                }
            }
        }
    }
}
