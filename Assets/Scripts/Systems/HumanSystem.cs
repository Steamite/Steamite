using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Humans;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.LightTransport;

namespace Assets.Scripts.Systems
{
    /*public struct MoveJob : IJobFor
    {

        public void Execute(int index)
        {

            float3 pos = buffer[human.ValueRO.currentWaypoint].position;
            float3 destination = math.lerp(transform.ValueRO.Position, pos, human.ValueRO.speed);
            transform.ValueRW.Position = destination;

            if (math.distancesq(destination, pos) < 0.1f)
            {
                int nextWay = human.ValueRW.currentWaypoint + 1;
                if (nextWay == buffer.Length)
                    nextWay = 0;
                human.ValueRW.currentWaypoint = nextWay;
            }
        }
    }*/
    public partial struct HumanSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (human, transform, buffer) in SystemAPI.Query<RefRW<HumanComponent>, RefRW<LocalTransform>, DynamicBuffer<PositionNode>>())
            {
                float3 pos = buffer[human.ValueRO.currentWaypoint].position;
                float3 destination = math.lerp(transform.ValueRO.Position, pos, human.ValueRO.speed);
                transform.ValueRW.Position = destination;

                if(math.distancesq(destination, pos) < 0.1f)
                {
                    int nextWay = human.ValueRW.currentWaypoint + 1;
                    if (nextWay == buffer.Length)
                        nextWay = 0;
                    human.ValueRW.currentWaypoint = nextWay;
                }
            }
        }
    }
}
