using Pathing;
using Unity.Burst;
using Unity.Entities;

namespace Assets.Scripts.Systems.Human.JobFind
{
    [UpdateInGroup(typeof(HumanUpdateGroup)), BurstCompile]
    public partial struct JobCheckAvailabilitySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobOffersComponent>();
            state.RequireForUpdate<WorldComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // CheckRocks
            var offers = SystemAPI.GetSingleton<JobOffersComponent>();
            var world = SystemAPI.GetSingleton<WorldComponent>();

            offers.impossibleToDig.AddRange(offers.digJobs);
            offers.digJobs.Clear();
            offers.digRegions.Clear();
            // rocks have world position 30329
            //var offsets = world.offsets;

            for (int i = offers.impossibleToDig.Length - 1; i >= 0; i--)
            {
                int pos = offers.impossibleToDig[i];
                if(world.path[pos] > 0)
                {
                    offers.impossibleToDig.RemoveAt(i);
                    offers.digJobs.Add(pos);
                    offers.digRegions.Add(RegionChanges.GetAdjacentRegions(pos, world.offsets, world.regions));
                }
            }
        }
    }
}
