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
    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float DeltaTime;
        public void Execute(ref HumanComponent human, ref LocalTransform transform, in DynamicBuffer<PositionNode> nodes)
        {
            float3 pos = nodes[human.currentWaypoint].position;
            float3 destination = transform.Position + 
                (math.normalize(pos - transform.Position) * (human.speed * DeltaTime));
            transform.Position = destination;

            if (math.distancesq(destination, pos) < 0.1f)
            {
                int nextWay = human.currentWaypoint + 1;
                if (nextWay == nodes.Length)
                    nextWay = 0;
                human.currentWaypoint = nextWay;
            }
        }
    }
    public partial struct HumanSystem : ISystem
    {
     //   EntityQuery query;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HumanComponent>();
       /*     query = SystemAPI.QueryBuilder()
                .WithAll<HumanComponent, LocalTransform, PositionNode>().Build();*/
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //NativeArray<HumanComponent> comps = query.ToComponentDataArray<HumanComponent>(Allocator.TempJob);
            MoveJob job = new()
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallelByRef(state.Dependency);
            //new MoveJob().ScheduleParallel(query);
            /*foreach (var (human, transform, buffer) in )
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
            }*/
        }
    }
}
