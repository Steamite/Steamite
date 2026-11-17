using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.AI.Navigation.LowLevel;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans
{
    [BurstCompile]
    public struct PathfindingNew : IJob
    {
        public NativeArray<short> path;
        public NativeArray<byte> regions;

        public int gridWidth;
        public int gridSize;
        public NativeArray<int> positionsToStartFrom;
        public int destination;

        public NativeReference<int> closestDestination;
        /// <summary>
        /// -1, -width, 1, width
        /// </summary>
        [ReadOnly] public NativeArray<int> offsetTable;
        public static readonly FixedList64Bytes<int2> offsetTableVec = new FixedList64Bytes<int2>()
        { 
            new int2(-1, 0),
            new int2(0, -1),
            new int2(1, 0),
            new int2(0, 1)
        };

        public NativeList<int> positions;

        private static readonly ProfilerMarker k_Init = new ProfilerMarker("Pathfind.1_Reallocate");
        private static readonly ProfilerMarker k_Search = new ProfilerMarker("Pathfind.2_Search");


        public NativeList<PathNode> TestRegions(NativeArray<int> startPositions, NativeArray<byte> regions, byte destinationRegion, int width, int2 destinationPosition)
        {
            NativeList<PathNode> nodes = new NativeList<PathNode>(startPositions.Length, Allocator.Temp);

            for (int i = 0; i < startPositions.Length; i++)
            {
                int startPos = startPositions[i];
                if (regions[startPos] == destinationRegion)
                {
                    int2 worldPos = new(startPos % width, startPos / width);

                    int level = startPos / (width ^ 2);

                    int distance = math.abs(worldPos.x - destinationPosition.x) + math.abs(worldPos.y - destinationPosition.y);
                    nodes.Add(new PathNode()
                    {
                        position = startPos,
                        worldPosition = worldPos,
                        costToReach = 0,
                        totalCost = distance,
                        levelIndex = level,
                        destinationIndex = i
                    });
                }
            }
            return nodes;
        }

        [BurstCompile]
        public void Execute()
        {
            int size = (gridWidth * gridWidth);
            int level = destination / size;
            int2 destinationPosition = new(destination % gridWidth, (destination / gridWidth) - (256 * level));

            k_Init.Begin();
            var nodes = TestRegions(positionsToStartFrom, regions, regions[destination], gridWidth, destinationPosition);
            if(nodes.Length == 0)
            {
                nodes.Dispose();
                closestDestination.Value = -1;
                k_Init.End();
                return;
            }
            QueueNew queue = new(nodes, destinationPosition, level, path.Length, gridWidth, regions, regions[destination]);
            PathNode node;

            k_Init.End();

            k_Search.Begin();
            bool found = false;
            while ((node = queue.Dequeue()).position != -1)
            {
                int pos = node.position;

                if (node.costToReach >= 2_000_000)
                {
                    node = new() { position = -1 };
                    break;
                } else if (pos == destination)
                {
                    found = true;
                    break;
                }

                short mapNode = path[pos];
                for (int i = 0; i < offsetTable.Length; i++)
                {
                    int checkPos = node.position + offsetTable[i];

                    int2 posWorld = node.worldPosition + offsetTableVec[i];
                    if ((mapNode & 0x1) == 1)
                    {
                        int costToReach = node.costToReach + 1;
                        int2 dist = math.abs(posWorld - destinationPosition);

                        PathNode newNode = new()
                        {
                            position = checkPos,
                            worldPosition = posWorld,
                            costToReach = costToReach,
                            totalCost = costToReach + dist.x + dist.y,
                            levelIndex = node.levelIndex,
                            destinationIndex = node.destinationIndex
                        };
                        queue.Enqueue(newNode, node.position);
                    }
                    mapNode >>= 0x1;
                }
                
                int iters = math.select(0, 2, (mapNode & 0x3) > 0);
                for (int i = 0; i < iters; i++)
                {
                    if((mapNode & 0x1) == 1) 
                    {
                        PathNode newNode = new()
                        {
                            position = pos + math.select(gridSize, -gridSize, i == 0),
                            worldPosition = node.worldPosition,
                            costToReach = node.costToReach + 1,
                            totalCost = node.totalCost + 1,
                            levelIndex = node.levelIndex + math.select(1, -1, i == 0),
                            destinationIndex = node.destinationIndex
                        };
                        queue.Enqueue(newNode, node.position);
                    }
                    mapNode >>= 0x1;
                }

            }
            k_Search.End();

            if(found == true)
            {
                closestDestination.Value = node.destinationIndex;

                int toVisit = node.position;
                while (toVisit != -1)
                {
                    positions.Add(toVisit);
                    toVisit = queue.tiles[toVisit].prevVisited;
                }
            }
            else
                closestDestination.Value = -1;

            queue.Dispose();
        }
    }
}
