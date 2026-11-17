using Grid.New;
using Humans;
using JetBrains.Annotations;
using Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Rendering.VirtualTexturing;

namespace Assets.Scripts.Systems
{
    public partial class HumanSpawner : SystemBase
    {
        
        protected override void OnCreate()
        {
            RequireForUpdate<TestSpherePrefab>();
            RequireForUpdate<HumanPrefab>();
            RequireForUpdate<WorldComponent>();
        }

        protected override void OnUpdate()
        {
            // Spheres
            var rand = new Unity.Mathematics.Random(1); // Provide seed

            TestSpherePrefab prefab = SystemAPI.GetSingleton<TestSpherePrefab>();
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


            // people
            rand = new Unity.Mathematics.Random(1); // Provide seed

            HumanPrefab pref = SystemAPI.GetSingleton<HumanPrefab>();
            if(pref.count == 1)
            {
                CreateHuman(ref buffer, ref pref, pref.pos);
            }
            else
            {
                WorldComponent worldComponent = SystemAPI.GetSingleton<WorldComponent>();
                DynamicBuffer<LevelComponent> levels = SystemAPI.GetSingletonBuffer<LevelComponent>();
                int center = worldComponent.LargestLevelDimensionScaled / 2;
                int2 spawnRange = new(center - LevelComponent.CHUNK_DIMENSIONS, center + LevelComponent.CHUNK_DIMENSIONS);
                for (int i = 0; i < pref.count; i++)
                {
                    int yPos = rand.NextInt(0, 5);
                    int2 pos = default;
                    do
                    {
                        pos = rand.NextInt2(
                            new(spawnRange.x, spawnRange.x),
                            new(spawnRange.y, spawnRange.y));
                    } while (levels[yPos]
                        .renderGrid[Recalculation.LocalIndexFromWorldPos(
                            pos, 
                            levels[yPos].offsetsScaled, 
                            levels[yPos].dimensionsScaled)]
                        .TType != TileType.Road);

                    CreateHuman(ref buffer, ref pref, new(pos.x, yPos, pos.y));
                }
            }

            buffer.Playback(EntityManager);
            buffer.Dispose();

            Enabled = false;
        }

        private static void CreateHuman(ref EntityCommandBuffer buffer, ref HumanPrefab pref, int3 pos)
        {
            var ent = buffer.Instantiate(pref.entity);
            buffer.SetComponent<PathfindComponent>(ent, new()
            {
                currentWaypoint = -1,
                speed = pref.speed,
                position = pos
            });
            pos.y = 0;
            buffer.SetComponent<LocalTransform>(ent, new()
            {
                Position = pos + new float3(0.5f, 1, 0.5f),
                Scale = 1
            });
        }
    }
}
