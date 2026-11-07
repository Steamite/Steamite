using Humans;
using Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace Assets.Scripts.Systems.Human.JobFind
{
    public partial struct JobFindingSystem : ISystem
    {

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobOfferComponent>();
            state.RequireForUpdate<HumanJobData>();
            state.RequireForUpdate<WorldComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var offers = SystemAPI.GetSingleton<JobOfferComponent>();
            var world = SystemAPI.GetSingleton<WorldComponent>();

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
                job.ValueRW.targetPosition = offers.digJobs[i];
                job.ValueRW.jobType = JobType.Dig;
                offers.digJobs.RemoveAt(i);
            }
        }

    }
}
