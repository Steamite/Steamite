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
using Unity.Profiling;

namespace Pathing
{

    [BurstCompile]
    [WithDisabled(typeof(HumanJobData))]
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
    [BurstCompile]

    public static class Pathfind
    {
        private static readonly ProfilerMarker k_InitMarker = new ProfilerMarker("Pathfind.0_Init");
        private static readonly ProfilerMarker k_FinishMarker = new ProfilerMarker("Pathfind.9_OpenSetPop");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="humanPosition"></param>
        /// <param name="nodes"></param>
        /// <param name="destinations">Global world positions.</param>
        /// <param name="worldData"></param>
        /// <returns></returns>
        [BurstCompile]
        public static int FindPath(
            ref PathfindComponent humanPosition, 
            in DynamicBuffer<PositionNode> nodes, 
            in NativeArray<int> destinations, 
            in WorldComponent worldData,
            in NativeList<uint> digRegions = default)
        {
            k_InitMarker.Begin();
            NativeList<int> positions = new NativeList<int>(20, Allocator.Temp);
            int yPos = ((int)math.floor(humanPosition.position.y));

            NativeReference<int> foundDestination = new NativeReference<int>(Allocator.TempJob);

            PathfindingNew pathfindingNew = new()
            {
                destination = yPos * worldData.largestGridSize +
                    ((int)math.floor(humanPosition.position.z)) * worldData.LargestLevelDimensionScaled +
                    ((int)math.floor(humanPosition.position.x)),

                startingPositions = destinations,
                startingPositionsRegions = digRegions,
                path = worldData.path,
                regions = worldData.regions,
                gridWidth = worldData.LargestLevelDimensionScaled,
                gridSize = worldData.largestGridSize,
                offsetTable = worldData.offsets,
                positions = positions,
                closestDestination = foundDestination
            };
            k_InitMarker.End();

            pathfindingNew.Execute();
            return EndSearch(ref humanPosition, nodes, worldData, positions, foundDestination);
        }

        static int EndSearch(ref PathfindComponent humanPosition, in DynamicBuffer<PositionNode> nodes, WorldComponent worldData, NativeList<int> positions, NativeReference<int> foundDestination)
        {
            k_FinishMarker.Begin();
            nodes.Clear();

            if (foundDestination.Value == -1)
            {
                humanPosition.currentWaypoint = -1;
                foundDestination.Dispose();
                k_FinishMarker.End();

                return -1;
            }

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
            k_FinishMarker.End();
            return val;
        }
/*
        public static int FindPath(
            ref PathfindComponent humanPosition,
            in DynamicBuffer<PositionNode> nodes,
            in NativeArray<int> destinations,
            in WorldComponent worldData, 
            NativeList<uint> digRegions)
        {
            k_InitMarker.Begin();
            NativeList<int> positions = new NativeList<int>(20, Allocator.Temp);
            int yPos = ((int)math.floor(humanPosition.position.y));

            NativeReference<int> foundDestination = new NativeReference<int>(Allocator.TempJob);

            PathfindingNew pathfindingNew = new()
            {
                destination = yPos * worldData.largestGridSize +
                    ((int)math.floor(humanPosition.position.z)) * worldData.LargestLevelDimensionScaled +
                    ((int)math.floor(humanPosition.position.x)),

                startingPositions = destinations,
                startingPositionsRegions = digRegions,
                path = worldData.path,
                regions = worldData.regions,
                gridWidth = worldData.LargestLevelDimensionScaled,
                gridSize = worldData.largestGridSize,
                offsetTable = worldData.offsets,
                positions = positions,
                closestDestination = foundDestination
            };
            k_InitMarker.End();

            pathfindingNew.Execute();

            return EndSearch(ref humanPosition, nodes, worldData, positions, foundDestination);
        }*/

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
