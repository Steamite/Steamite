using Humans;
using Pathing;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.LightTransport;

namespace Assets.Scripts.Systems.Human.JobFind
{

    [BurstCompile]
    public partial struct FindJob : IJob
    {
        public EntityQuery query;

        public void Execute()
        {
            //query;
        }
        public void Execute(ref PathfindComponent path, ref DynamicBuffer<PositionNode> nodes, ref HumanJobData job, HumanIsFree _)
        {
            /*if (job.jobType != JobType.Free)
                return;

            if (offers.digJobs.Count == 0)
                break;

            return;
            int i = Pathing.Pathfind.FindPath(ref pathfind.ValueRW, nodes, offers.digJobs.AsArray(), world);
            if (i == -1)
                return;

            nodes.RemoveAt(nodes.Length - 1);
            int levelNumber = offers.digJobs[i] / world.largestGridSize;
            int onLevel = offers.digJobs[i] % world.largestGridSize;

            int2 levelPosition = new int2(
                onLevel % world.LargestLevelDimensionScaled,
                onLevel / world.LargestLevelDimensionScaled) - levels[levelNumber].levelOffsetScaled;

            job.ValueRW.targetLevel = levelNumber;
            job.ValueRW.targetPosition = levelPosition.x + (levelPosition.y * levels[levelNumber].levelDimensionsScaled);
            job.ValueRW.jobType = JobType.Dig;
            offers.digJobs.RemoveAt(i);*/
        }
    }


    [UpdateInGroup(typeof(HumanUpdateGroup)), UpdateAfter(typeof(JobCheckAvailabilitySystem))]
    [BurstCompile]
    public partial struct JobFindingSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobOffersComponent>();
            state.RequireForUpdate<WorldComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var offers = SystemAPI.GetSingleton<JobOffersComponent>();
            var world = SystemAPI.GetSingleton<WorldComponent>();
            var levels = SystemAPI.GetSingletonBuffer<GridLevelComponent>();

            if (offers.digJobs.Count == 0 && offers.buildingJobs.Count == 0)
                return;

            /*FindJob findJob = new()
            {

            };*/

            foreach ((var pathfind, var nodes, var job, var _) in SystemAPI.Query<
                RefRW<PathfindComponent>,
                DynamicBuffer<PositionNode>,
                RefRW<HumanJobData>,
                HumanIsFree>())
            {
                if (job.ValueRO.jobType != JobType.Free)
                    continue;

                if (offers.digJobs.Count == 0)
                    break;

                int i = Pathing.Pathfind.FindPath(ref pathfind.ValueRW, nodes, offers.digJobs.AsArray(), world);
                if (i == -1)
                    return;

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
