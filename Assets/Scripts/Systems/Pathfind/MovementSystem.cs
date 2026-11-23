using Humans;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pathing
{

    [BurstCompile]
    public partial struct PathfindMovingJob : IJobEntity
    {
        static readonly float3 offset = new float3(0.5f, 0, 0.5f);

        [BurstCompile]
        public void Execute(ref PathfindComponent path, ref LocalTransform transform, ref RemainingTime time, in DynamicBuffer<PositionNode> nodes, EnabledRefRW<RemainingTime> timeEnabled)
        {
            if (path.currentWaypoint == -1 || path.currentWaypoint >= nodes.Length)
                return;

            float sp = path.speed * time.remainingTime;
            float maxDistance = sp * sp;// math.pow(path.speed * DeltaTime, 2);

            float3 pos = transform.Position - offset;
            float3 waypoint = nodes[path.currentWaypoint].position;
            float2 dir = new(waypoint.x - pos.x, waypoint.z - pos.z);// waypoint - pos;// math.normalizesafe();


            float dist = /*math.sqrt(*/(dir.x * dir.x) + (dir.y * dir.y);//);

            if(dist < maxDistance)
            {
                // keep rest for the next movement
                time.remainingTime -= (maxDistance / sp);

                path.position = waypoint;
                transform.Position = new float3(waypoint.x, 1, waypoint.z) + offset;
                path.currentWaypoint++;
                if (path.currentWaypoint == nodes.Length)
                {
                    path.currentWaypoint = -1;
                    timeEnabled.ValueRW = false;
                }
            }
            else
            {
                dir = math.normalizesafe(dir) * sp;
                transform.Position += new float3(dir.x, 0, dir.y);
            }
        }
    }

    [UpdateInGroup(typeof(HumanUpdateGroup)), UpdateAfter(typeof(PathfindSystem))] 
    [BurstCompile]
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PathfindComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float time = GameTime.DeltaTime;
            if (time <= 0)
                return;

            PathfindMovingJob job = new()
            {
            };
            state.Dependency = job.ScheduleParallelByRef(state.Dependency);
        }
    }
}
