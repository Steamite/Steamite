using Humans;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace Assets.Scripts.Systems.Human.JobFind
{
    [UpdateInGroup(typeof(HumanUpdateGroup)), UpdateBefore(typeof(JobCheckAvailabilitySystem))]
    public partial struct JobCancelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobOffersComponent>();
            state.RequireForUpdate<JobCancelComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var offers = SystemAPI.GetSingleton<JobOffersComponent>();
            var cancels = SystemAPI.GetSingleton<JobCancelComponent>();
            

            var humans = SystemAPI.GetComponentLookup<HumanJobData>();
            var times = SystemAPI.GetComponentLookup<RemainingTime>();
            var pathfind = SystemAPI.GetComponentLookup<PathfindComponent>();
            var positionNodes = SystemAPI.GetBufferLookup<PositionNode>();

            offers.impossibleToDig.AddRange(offers.digJobs);
            offers.digJobs.Clear();
            offers.digRegions.Clear();

            foreach (var position in cancels.positionsToCancel)
            {
                bool found = false;
                for (int i = 0; i < offers.impossibleToDig.Count; i++)
                {
                    if (offers.impossibleToDig[i] == position)
                    {
                        found = true;
                        offers.impossibleToDig.RemoveAt(i);
                        break;
                    }
                }
                if (found)
                    continue;
                
                for (int i = 0; i < offers.assignedJobs.Count; i++)
                {
                    if (offers.assignedJobs[i].position == position)
                    {
                        var ent = offers.assignedJobs[i].assignHuman;
                        
                        humans.SetComponentEnabled(ent, false);
                        humans.GetRefRW(ent).ValueRW.Clear();
                        times.GetEnabledRefRW<RemainingTime>(ent).ValueRW = false;



                        var rw = pathfind.GetRefRW(ent);
                        var nodes = positionNodes[ent];

                        if (rw.ValueRO.currentWaypoint > -1)
                        {
                            var node = nodes[rw.ValueRO.currentWaypoint];
                            nodes.Clear();

                            nodes.Add(node);
                            rw.ValueRW.currentWaypoint = 0;
                        }
                        else
                            nodes.Clear();

                        offers.assignedJobs.RemoveAt(i);

                        break;
                    }
                }

            }
            cancels.positionsToCancel.Clear();
        }
    }
}
