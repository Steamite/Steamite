using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans;
using Assets.Scripts.Systems.Human.JobFind;
using Humans;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pathing
{

    [BurstCompile]
    partial struct PathfindSetJob : IJobEntity
    {
        [ReadOnly] public DestinationComponent destination;
        [ReadOnly] public WorldComponent worldData;


        [BurstCompile]
        public void Execute(ref PathfindComponent humanPosition, ref DynamicBuffer<PositionNode> nodes, ref HumanJobData job)
        {
            if (job.jobType != JobType.Free)
                return;
            Pathfind.FindPath(ref humanPosition, nodes, destination.positions, worldData);
            
        }
    }

    public static class Pathfind
    {
        [BurstCompile]
        public static int FindPath(
            ref PathfindComponent humanPosition, 
            DynamicBuffer<PositionNode> nodes, 
            NativeArray<int> destinations, 
            WorldComponent worldData)
        {
            NativeList<int> positions = new NativeList<int>(20, Allocator.Temp);
            int yPos = ((int)math.floor(humanPosition.position.y));

            NativeReference<int> foundDestination = new NativeReference<int>(Allocator.TempJob);

            PathfindingNew pathfindingNew = new()
            {
                destination = yPos * worldData.largestGridSize +
                    ((int)math.floor(humanPosition.position.z)) * worldData.LargestLevelDimensionScaled +
                    ((int)math.floor(humanPosition.position.x)),

                positionsToStartFrom = destinations,

                level = yPos,
                path = worldData.path,
                gridWidth = worldData.LargestLevelDimensionScaled,
                gridSize = worldData.largestGridSize,
                offsetTable = worldData.offsets,
                positions = positions,
                closestDestination = foundDestination
            };

            pathfindingNew.Execute();
            nodes.Clear();

            if(foundDestination.Value == -1)
            { 
                humanPosition.currentWaypoint = -1;
                foundDestination.Dispose();
                return -1;
            }

            nodes.Capacity = positions.Length - 1;
            humanPosition.currentWaypoint = 0;
            for (int i = 1; i < positions.Length; i++)
            {
                int levelPosition = positions[i] % worldData.largestGridSize;
                int yPosition = positions[i] / worldData.largestGridSize;

                int2 pos = new int2(
                    positions[i] % worldData.LargestLevelDimensionScaled,
                    levelPosition / worldData.LargestLevelDimensionScaled);

                nodes.Add(new()
                {
                    position = new(
                    pos.x,
                    yPosition,
                    pos.y)
                });
            }

            int val = foundDestination.Value;
            foundDestination.Dispose();
            return val;
        }
    }



    [UpdateInGroup(typeof(HumanUpdateGroup)), UpdateAfter(typeof(JobFindingSystem))]
    [BurstCompile]
    public partial struct PathfindSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestinationComponent>();
            state.RequireForUpdate<WorldComponent>();
            state.Enabled = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var comp = SystemAPI.GetSingleton<DestinationComponent>();
            var world = SystemAPI.GetSingleton<WorldComponent>();


            PathfindSetJob pathfindSetJob = new()
            {
                destination = comp,
                worldData = world,
            };

            state.Dependency = pathfindSetJob.ScheduleParallelByRef(state.Dependency);

            state.Enabled = false;
        }
    }
}
