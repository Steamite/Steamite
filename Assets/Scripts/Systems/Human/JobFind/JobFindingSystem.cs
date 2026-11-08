using Humans;
using Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Systems.Human.JobFind
{
    [BurstCompile]
    public partial struct JobFindingSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobOfferComponent>();
            state.RequireForUpdate<HumanJobData>();
            state.RequireForUpdate<WorldComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var offers = SystemAPI.GetSingleton<JobOfferComponent>();
            var ent = SystemAPI.GetSingletonEntity<WorldComponent>();
            var world = SystemAPI.GetComponent<WorldComponent>(ent);
            var levels = SystemAPI.GetBuffer<GridLevelComponent>(ent);

            if (offers.digJobs.Count == 0 && offers.buildingJobs.Count == 0)
                return;

            foreach ((var pathfind, var nodes, var job) in SystemAPI.Query<
                RefRW<PathfindComponent>, 
                DynamicBuffer<PositionNode>, 
                RefRW<HumanJobData>>())
            {
                if (job.ValueRO.jobType != JobType.Free)
                    continue;

                if (offers.digJobs.Count == 0)
                    break;

                int i = Pathing.Pathfind.FindPath(ref pathfind.ValueRW, nodes, offers.digJobs.AsArray(), world);
                if (i == -1)
                    continue;

                nodes.RemoveAt(nodes.Length - 1);
                int levelNumber = offers.digJobs[i] / world.largestGridSize;
                int onLevel = offers.digJobs[i] % world.largestGridSize;

                int2 levelPosition = new int2(
                    onLevel % world.LargestLevelDimensionScaled, 
                    onLevel / world.LargestLevelDimensionScaled) - levels[levelNumber].levelOffsetScaled;

                job.ValueRW.targetLevel = levelNumber;
                job.ValueRW.targetPosition = levelPosition.x + (levelPosition.y * levels[levelNumber].levelDimensionsScaled);
                job.ValueRW.jobType = JobType.Dig;
                offers.digJobs.RemoveAt(i);
            }
        }

    }
}
