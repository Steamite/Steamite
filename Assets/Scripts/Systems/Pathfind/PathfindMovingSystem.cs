using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Humans;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Systems.Pathing
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

            float3 pos = transform.Position - offset;
            float3 waypoint = nodes[path.currentWaypoint].position;
            float3 dir = waypoint - pos;// math.normalizesafe();
            

            float dist = math.sqrt(dir.x* dir.x + dir.z* dir.z);

            if(dist < sp)
            {
                transform.Position = waypoint + offset;
                path.currentWaypoint++;
                if (path.currentWaypoint == nodes.Length)
                    path.currentWaypoint = -1;
            }
            else
            {
                dir = math.normalizesafe(dir) * sp;
                transform.Position += dir;
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
