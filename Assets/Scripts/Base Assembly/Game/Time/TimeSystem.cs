using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Scripts.Base_Assembly.Game.Time
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true), ]
    public partial struct TimeSystem : ISystem
    {/*
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeComponent>();
            GameTime.SetDeltaTime(0);
        }*/
        
        public void OnUpdate(ref SystemState state)
        {
            float delta = SystemAPI.Time.DeltaTime * GameTime.ScaleTime;
            GameTime.SetDeltaTime(delta);
        }
    }
}
