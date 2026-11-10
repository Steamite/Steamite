using Pathing;
using Unity.Burst;
using Unity.Entities;

namespace Assets.Scripts.Systems.Human.JobFind
{
    [UpdateInGroup(typeof(HumanUpdateGroup)), BurstCompile]
    public partial struct JobCheckAvailabilitySystem : ISystem
    {
        public void OnStart(ref SystemState state)
        {
            state.RequireForUpdate<JobOffersComponent>();
            state.RequireForUpdate<WorldComponent>();
        }

        /*public void OnUpdate(ref SystemState state)
        {
            // CheckRocks
            var offers = SystemAPI.GetSingleton<JobOffersComponent>();
            var world = SystemAPI.GetSingleton<WorldComponent>();
            
            offers.digJobs.AddRange(offers.impossibleToDig);

            // rocks have world position
            var offsets = world.offsets;

            for (int i = 0; i < offers.digJobs.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int worldIndex = offers.digJobs[i] + ;
                }
            }
        }*/
    }
}
