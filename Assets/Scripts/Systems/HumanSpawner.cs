using Assets.Scripts.Systems.Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Systems
{
    public partial class HumanSpawner : SystemBase
    {
        
        protected override void OnCreate()
        {
            RequireForUpdate<HumanPrefab>();
            RequireForUpdate<PathfindPrefab>();
        }

        protected override void OnUpdate()
        {
            var rand = new Unity.Mathematics.Random(1); // Provide seed

            HumanPrefab prefab = SystemAPI.GetSingleton<HumanPrefab>();
            var buffer = new EntityCommandBuffer(Allocator.TempJob);

            for (int i = 0; i < prefab.count; i++)
            {
                var ent = buffer.Instantiate(prefab.entity);
                var b = buffer.SetBuffer<PositionNode>(ent);
                for (int j = 0; j < 20; j++)
                {
                    b.Add(new()
                    {
                        position = rand.NextFloat3(new(-50, -50, -50), new(50, 50, 50))
                    });
                }
            }

            rand = new Unity.Mathematics.Random(1); // Provide seed

            PathfindPrefab pref = SystemAPI.GetSingleton<PathfindPrefab>();
            for (int i = 0; i < pref.count; i++)
            {
                var ent = buffer.Instantiate(pref.entity);
                int3 pos = rand.NextInt3(new(64, 1, 64), new(90, 1, 90));
                buffer.SetComponent<PathfindingComponent>(ent, new()
                {
                    currentWaypoint = -1,
                    speed = pref.speed,
                    position = pos
                });
                buffer.SetComponent<LocalTransform>(ent, new()
                {
                    Position = pos + new float3(0.5f, 0, 0.5f),
                    Scale = 1
                });
            }


            buffer.Playback(EntityManager);
            buffer.Dispose();

            Enabled = false;
        }
    }
}
