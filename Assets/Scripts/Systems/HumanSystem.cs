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
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HumanComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            MoveJob job = new()
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallelByRef(state.Dependency);
        }
    }
}
