using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Systems.Human.JobFind;
using Humans;
using Pathing;
using Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace Assets.Scripts.Systems.Human.Jobs
{
    [UpdateInGroup(typeof(HumanUpdateGroup)), UpdateAfter(typeof(MovingSystem))]
    public partial struct DigSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HumanJobData>();
            state.RequireForUpdate<WorldComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var worldComponent = SystemAPI.GetSingleton<WorldComponent>();
            var levels = SystemAPI.GetSingletonBuffer<LevelComponent>();
            var overlayBuffers = SystemAPI.GetSingletonBuffer<OverlayComponent>();

            var change = SystemAPI.GetSingletonRW<OverlayChangeComponent>();
/*
            foreach ((var jobData, var pathing) in SystemAPI.Query<RefRW<HumanJobData>, RefRO<PathfindComponent>>())
            {
                if (jobData.ValueRO.jobType == JobType.Free)
                    continue;
                if (pathing.ValueRO.currentWaypoint != -1)
                    continue;

                if (jobData.ValueRO.jobType == JobType.Dig)
                {
                    int levelIndex = jobData.ValueRO.targetLevel;
                    var level = levels[levelIndex];
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

                        // clear overlay
                        int world = Recalculation.WorldIndexFromLocal(localIndex, level.levelOffsetScaled, level.levelDimensionsScaled);
                        var overlay = overlayBuffers[levelIndex];
                        
                        overlay.overlayGrid[world] = overlay.overlayGrid[world].R(0);
                        overlay.markedTilesGrid.Set(world, false);

                        change.ValueRW.hasChange = true;

                        PathChanges.UpdatePath(
                            localIndex, 
                            levelIndex, 
                            tile, 
                            level.levelDimensionsScaled,
                            level.levelOffsetScaled, 
                            worldComponent.offsets,
                            level.neighbourOffsets, 
                            level.renderGrid,
                            worldComponent.path);
                    }
                    grid[localIndex] = tile;
                }
            }*/
        }
    }
}
