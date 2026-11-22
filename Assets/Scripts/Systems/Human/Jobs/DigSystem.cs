using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Paths;
using Assets.Scripts.Systems.Human.JobFind;
using Grid.New;
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

            state.RequireForUpdate<JobCancelComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float time = GameTime.DeltaTime;
            if (time <= 0)
                return;

            var worldComponent = SystemAPI.GetSingleton<WorldComponent>();
            var levels = SystemAPI.GetSingletonBuffer<LevelComponent>();
            var overlayBuffers = SystemAPI.GetSingletonBuffer<OverlayComponent>();

            var change = SystemAPI.GetSingletonRW<OverlayChangeComponent>();

            var jobCancel = SystemAPI.GetSingleton<JobCancelComponent>();

            foreach ((var jobData, var pathing, var isFree) in SystemAPI.Query<
                RefRW<HumanJobData>, RefRO<PathfindComponent>, EnabledRefRW<HumanJobData>>())
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

                    tile.RemoveDurability(GameTime.DeltaTime);
                    if (tile.TType == TileType.Road)
                    {
                        // clear overlay
                        int world = Recalculation.WorldIndexFromLocalIndex(localIndex, level.offsetsScaled, level.dimensionsScaled);
                        var overlay = overlayBuffers[levelIndex];

                        overlay.overlayGrid[world] = overlay.overlayGrid[world].R(0);
                        overlay.markedTilesGrid.Set(world, false);

                        change.ValueRW.hasChange = true;

                        /// cancel the position and get freed in the <see cref="JobCancelSystem.OnUpdate(ref SystemState)"/>
                        jobCancel.positionsToCancel.Add(world);
                    }
                    WorldChanges.SetTileRuntime(localIndex, levelIndex, tile, ref worldComponent, levels.AsNativeArray());
                }
            }
        }
    }
}
