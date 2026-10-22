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
        public NativeArray<LevelGridTile> grids;
        public int gridWidth;
        public int start;
        public int destination;
        /// <summary>
        /// 1, width, -1, -width
        /// </summary>
        [ReadOnly] public NativeArray<int> offsetTable;
        public static readonly int2[] offsetTableVec = 
        { 
            new(1, 0),
            new(0, 1),
            new(-1, 0),
            new(0, -1)
        };

        public NativeList<int> positions;

        public void Execute()
        {
            int2 sPos = new(start % gridWidth, start / gridWidth);
            int2 dPos = new(destination % gridWidth, destination / gridWidth);


            int2 expectedSize = math.abs(dPos - sPos) + 2;
            int expectedTileCount = expectedSize.x * expectedSize.y;

            
            QueueNew queue = new(start, sPos, expectedTileCount, 0, gridWidth * gridWidth);
            PathNode node = default;
            bool found = false;
            while ((node = queue.Dequeue()).position != -1)
            {
                int pos = node.position;
                int2 posWorld = node.worldPosition;


                if (pos == destination)
                {
                    found = true;
                    break;
                }
                else if (node.costToReach >= 2_000_000)
                {
                    node = new() { position = -1 };
                    break;
                }

                for (int i = 0; i < offsetTable.Length; i++)
                {
                    pos = node.position + offsetTable[i];

                    posWorld = node.worldPosition + offsetTableVec[i];
                    int costToReach = math.select(
                        2_000_000, 
                        node.costToReach + 1, 
                        grids[pos].TType == TileType.Road || grids[pos].TType == TileType.Empty);
                    int2 dist = math.abs(posWorld - dPos);

                    PathNode newNode = new()
                    {
                        position = pos,
                        worldPosition = posWorld,
                        costToReach = costToReach,
                        totalCost = costToReach + dist.x + dist.y,
                    };
                    queue.Enqueue(newNode, node.position);
                }
            }
            
            if(found == true)
            {
                int toVisit = node.position;
                while (toVisit != -1)
                {
                    positions.Add(toVisit);
                    toVisit = queue.tiles[toVisit].prevVisited;
                }
            }
            queue.Dispose();
        }
    }
}
