using Humans;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Game.Time
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true), ]
    public partial struct TimeSystem : ISystem
    {
        
        public void OnUpdate(ref SystemState state)
        {
            float delta = SystemAPI.Time.DeltaTime * GameTime.ScaleTime;
            GameTime.SetDeltaTime(delta);

            Debug.Log(delta);
            if (GameTime.ScaleTime == 0)
                return;

            foreach((var time, var enable) in 
                SystemAPI.Query<RefRW<RemainingTime>, EnabledRefRO<RemainingTime>>().WithPresent<RemainingTime>())
            {
                if(enable.ValueRO == false)
                {
                    time.ValueRW.remainingTime = delta;
                }
                else
                {
                    time.ValueRW.remainingTime += delta;
                }
            }
            
        }
    }
}
