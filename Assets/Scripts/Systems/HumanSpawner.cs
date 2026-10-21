using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    [DisableAutoCreation]
    public partial class HumanSpawner : SystemBase
    {
        
        protected override void OnCreate()
        {
            RequireForUpdate<HumanPrefab>();
        }
        protected override void OnUpdate()
        {
            HumanPrefab prefab = SystemAPI.GetSingleton<HumanPrefab>();

            var buffer = new EntityCommandBuffer(Allocator.TempJob);

            for (int i = 0; i < 5000; i++)
            {
                var ent = EntityManager.Instantiate(prefab.entity);
                var b = EntityManager.GetBuffer<PositionNode>(ent);
                /*buffer.AddComponent(ent, new HumanComponent() { currentWaypoint = 0, speed = 0.1f});
                buffer.AddComponent<LocalTransform>(ent);
                var b = buffer.AddBuffer<PositionNode>(ent);*/
                for (int j = 0; j < 20; j++)
                {
                    b.Add(new() 
                    { 
                        position = new(
                            UnityEngine.Random.Range(-50, 50), 
                            UnityEngine.Random.Range(-50, 50), 
                            UnityEngine.Random.Range(-50, 50))
                    });
                }
            }

            buffer.Playback(EntityManager);
            buffer.Dispose();

            Enabled = false;
        }
    }
}
