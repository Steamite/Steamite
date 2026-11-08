using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Humans;
using Pathing;
using Rendering;
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
            //var world = SystemAPI.GetComponent<WorldComponent>(ent);
            var levels = SystemAPI.GetBuffer<GridLevelComponent>(ent);

            foreach((var jobData, var pathing) in SystemAPI.Query<RefRW<HumanJobData>, RefRO<PathfindComponent>>())
            {
                if (jobData.ValueRO.jobType == JobType.Free)
                    continue;
                if (pathing.ValueRO.currentWaypoint != -1)
                    continue;

                if (jobData.ValueRO.jobType == JobType.Dig)
                {
                    var level = levels[jobData.ValueRO.targetLevel];
                    var grid = level.renderGrid;
                    
                    int localIndex = jobData.ValueRO.targetPosition;
                    var tile = grid[localIndex];
                    
                    tile.RemoveDurability(1);
                    if(tile.TType == TileType.Road)
                    {
                        DirtyGridUtils.SetDirty(
                            level.dirtyChunks,
                            level.levelDimensionsScaled,
                            level.levelDimensions,
                            localIndex);
                        jobData.ValueRW.jobType = JobType.Free;
                        jobData.ValueRW.targetPosition = -1;
                        // update roads
                        //world.path
                    }
                    grid[localIndex] = tile;
                }
            }
        }
    }
}
