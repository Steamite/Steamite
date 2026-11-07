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
        public float DeltaTime;
        static readonly float3 offset = new float3(0.5f, 0, 0.5f);

        [BurstCompile]
        public void Execute(ref PathfindComponent path, ref LocalTransform transform, in DynamicBuffer<PositionNode> nodes)
        {
            if (path.currentWaypoint == -1 || path.currentWaypoint >= nodes.Length)
                return;

            float sp = path.speed * DeltaTime;
            float maxDistance = sp * sp;// math.pow(path.speed * DeltaTime, 2);

            float3 pos = transform.Position - offset;
            float3 waypoint = nodes[path.currentWaypoint].position;
            float2 dir = new(waypoint.x - pos.x, waypoint.z - pos.z);// waypoint - pos;// math.normalizesafe();


            float dist = /*math.sqrt(*/(dir.x * dir.x) + (dir.y * dir.y);//);

            if(dist < maxDistance)
            {
                path.position = waypoint;
                transform.Position = new float3(waypoint.x, 1, waypoint.z) + offset;
                path.currentWaypoint++;
                if (path.currentWaypoint == nodes.Length)
                    path.currentWaypoint = -1;
            }
            else
            {
                dir = math.normalizesafe(dir) * sp;
                transform.Position += new float3(dir.x, 0, dir.y);
            }
        }
    }
    [UpdateAfter(typeof(PathfindSystem))] 
    [BurstCompile]
    public partial struct PathfindMovingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PathfindComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PathfindMovingJob job = new()
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            state.Dependency = job.ScheduleByRef(state.Dependency);
        }
    }
}
