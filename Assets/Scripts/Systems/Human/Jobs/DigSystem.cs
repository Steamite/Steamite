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
            var world = SystemAPI.GetSingleton<WorldComponent>();
            foreach(var jobData in SystemAPI.Query<RefRW<HumanJobData>>())
            {
                if (jobData.ValueRO.jobType == JobType.Free)
                    continue;

                if (jobData.ValueRO.jobType == JobType.Dig)
                {
                    // Remove rock
                }
            }
        }
    }
}
