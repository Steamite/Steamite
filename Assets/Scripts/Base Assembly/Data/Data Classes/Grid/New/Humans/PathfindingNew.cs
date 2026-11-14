using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.AI.Navigation.LowLevel;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans
{
    [BurstCompile]
    public struct PathfindingNew : IJob
    {
        public NativeArray<short> path;
        public int gridWidth;
        public int gridSize;
        public NativeArray<int> positionsToStartFrom;
        public int destination;

        public NativeReference<int> closestDestination;
        /// <summary>
        /// -1, -width, 1, width
        /// </summary>
        [ReadOnly] public NativeArray<int> offsetTable;
        public static readonly int2[] offsetTableVec = 
        { 
            new(-1, 0),
            new(0, -1),
            new(1, 0),
            new(0, 1)
        };

        public NativeList<int> positions;
        public int level;

        [BurstCompile]
        public void Execute()
        {
            int2 destinationPosition = new(destination % gridWidth, destination / gridWidth);


            
            QueueNew queue = new(positionsToStartFrom, destinationPosition, level, path.Length, gridWidth);
            PathNode node;

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
                }/*
                else if (node.costToReach >= 2_000_000)
                {
                    node = new() { position = -1 };
                    break;
                }*/

                short mapNode = path[pos];
                for (int i = 0; i < offsetTable.Length; i++)
                {
                    int checkPos = node.position + offsetTable[i];

                    int2 posWorld = node.worldPosition + offsetTableVec[i];
                    int costToReach = math.select(
                        2_000_000, 
                        node.costToReach + 1,
                        (mapNode & 0x1) == 1);
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
