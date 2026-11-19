using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public NativeArray<int> startingPositions;
        public NativeList<uint> startingPositionsRegions;

        public int destination;

        public NativeReference<int> closestDestination;
        /// <summary>
        /// -1, -width, 1, width
        /// </summary>
        [ReadOnly] public NativeArray<int> offsetTable;
        public static readonly FixedList64Bytes<int3> offsetTableVec = new FixedList64Bytes<int3>()
        {
            new int3(-1,    0,     0),
            new int3(0,     0,     -1),
            new int3(1,     0,     0),
            new int3(0,     0,     1)
        };

        public NativeList<int> positions;

        private static readonly ProfilerMarker k_Region = new ProfilerMarker("Pathfind.1_RegionSearch");
        private static readonly ProfilerMarker k_QueueStart = new ProfilerMarker("Pathfind.2_QueueInit");
        private static readonly ProfilerMarker k_Search = new ProfilerMarker("Pathfind.5_Search");
        

        public NativeList<PathNode> TestRegions(int3 destinationPosition)
        {
            NativeList<PathNode> nodes = new NativeList<PathNode>(startingPositions.Length, Allocator.Temp);

            for (int i = 0; i < startingPositions.Length; i++)
            {
                int startPos = startingPositions[i];
                if (regions[startPos] == regions[destination])
                {
                    int3 worldPos = new(startPos % gridWidth, startPos / gridSize, startPos / gridWidth);

                    // + math.abs(worldPos.y - destinationPosition.y);
                    nodes.Add(new PathNode()
                    {
                        position = startPos,
                        costToReach = 0,
                        totalCost = Recalculation.Distance(worldPos, destinationPosition),
                        destinationIndex = i
                    });
                }
            }
            return nodes;
        }

        NativeList<PathNode> TestRockRegions(int3 destinationPosition)
        {
            NativeList<PathNode> nodes = new NativeList<PathNode>(startingPositions.Length, Allocator.Temp);

            int reg = regions[destination];

            for (int i = 0; i < startingPositions.Length; i++)
            {
                uint region = startingPositionsRegions[i];
                for (int j = 0; j < 4; j++)
                {
                    if((region & 0xF3) == reg)
                    {
                        int startPos = startingPositions[i];
                        int3 worldPos = new(startPos % gridWidth, startPos / gridSize, startPos / gridWidth);

                        nodes.Add(new PathNode()
                        {
                            position = startPos,
                            costToReach = 0,
                            totalCost = Recalculation.Distance(worldPos, destinationPosition),
                            destinationIndex = i
                        });

                        break;
                    }
                    region >>= 8;
                }
                /*
                int startPos = startingPositions[i];
                if (startingPositionsRegions[i] == regions[destination])
                {
                    int3 worldPos = new(startPos % gridWidth, startPos / gridSize, startPos / gridWidth);

                    // + math.abs(worldPos.y - destinationPosition.y);
                    nodes.Add(new PathNode()
                    {
                        position = startPos,
                        costToReach = 0,
                        totalCost = Recalculation.Distance(worldPos, destinationPosition),
                        destinationIndex = i
                    });
                }*/
            }
            return nodes;
        }

        [BurstCompile]
        public void Execute()
        {
            int level = destination / gridSize;
            int3 destinationPosition = new(destination % gridWidth, level, (destination / gridWidth) - (256 * level));

            k_Region.Begin();
            NativeList<PathNode> nodes;
            if (startingPositionsRegions.IsCreated == false)
                nodes = TestRegions(destinationPosition);
            else
                nodes = TestRockRegions(destinationPosition);

            k_Region.End();
            if (nodes.Length == 0)
            {
                nodes.Dispose();
                closestDestination.Value = -1;
                return;
            }

            k_QueueStart.Begin();
            QueueNew queue = new(nodes, path.Length);
            k_QueueStart.End();

            PathNode node;


            k_Search.Begin();
            bool found = false;
            while ((node = queue.Dequeue()).position != -1)
            {
                int pos = node.position;
                
                if (pos == destination)
                {
                    found = true;
                    break;
                }
                short mapNode = path[pos];
                int3 worldPos = new(pos % gridWidth, pos / gridSize, (pos / gridWidth) % gridWidth);

                for (int i = 0; i < 4; i++)
                {
                    if ((mapNode & 0x1) == 1)
                    {
                        int checkPos = node.position + offsetTable[i];

                        int3 tmpPos = worldPos + offsetTableVec[i];

                        int costToReach = node.costToReach + 1;

                        PathNode newNode = new()
                        {
                            position = checkPos,
                            costToReach = costToReach,
                            totalCost = costToReach + Recalculation.Distance(tmpPos, destinationPosition),
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
                            costToReach = node.costToReach + 1,
                            totalCost = node.totalCost + 1,
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
