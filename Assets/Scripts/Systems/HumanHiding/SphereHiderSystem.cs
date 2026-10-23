using Assets.Scripts.Systems.Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Systems.HumanHiding
{

    [BurstCompile]
    public partial struct SphereHiderSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SphereHiderComponent>();
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SphereHiderComponent component = SystemAPI.GetSingleton<SphereHiderComponent>();

            foreach((var pos, var trans) in SystemAPI.Query<RefRO<PathfindComponent>, RefRW<LocalTransform>>())
            {
                int y = (int)math.floor(pos.ValueRO.position.y);
                if (y != component.currentLevel)
                    trans.ValueRW.Scale = 0;
                else
                    trans.ValueRW.Scale = 1;
            }

            state.Enabled = false;
        }
    }
}
