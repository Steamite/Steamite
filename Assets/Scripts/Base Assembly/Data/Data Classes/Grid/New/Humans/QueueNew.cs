using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.VFX;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans
{
    public struct PathNode
    {
        public int position;
        public int2 worldPosition;
        
        public int totalCost;
        public int costToReach;
    }


    public struct Visited
    {
        public int cost;
        public int prevVisited;
    }
    public struct QueueNew : IDisposable
    {
        NativeList<PathNode> queue;

        public NativeArray<Visited> tiles;

        public QueueNew(int startIndex, int2 worldPosition, int expectedSize, int heurestic, int worldSize)
        {
            queue = new(expectedSize, Allocator.Temp);
            tiles = new(worldSize, Allocator.Temp);


            Enqueue(new PathNode()
            {
                position = startIndex,
                worldPosition = worldPosition,
                totalCost = -1
            },
                -1);
        }

        public void Dispose()
        {
            queue.Dispose();
            tiles.Dispose();
        }

        void BubbleUp(int index)
        {
            int parent = index;
            while ((parent = (parent - 1) / 2) > -1)
            {
                PathNode parentNode = queue[parent];
                if (parentNode.totalCost <= queue[index].totalCost)
                {
                    break;
                }
                queue[parent] = queue[index];
                queue[index] = parentNode;
                index = parent;
            }
        }

        void BubbleDown(int index)
        {
            int parent = index;
            while ((parent = 1 + (parent * 2)) < queue.Count)
            {
                PathNode l = queue[parent];
                if (parent + 1 < queue.Count)
                {
                    PathNode r = queue[parent + 1];
                    if (l.totalCost > r.totalCost)
                    {
                        parent++;
                        l = r;
                    }
                }
                if (l.totalCost > queue[index].totalCost)
                {
                    break;
                }

                queue[parent] = queue[index];
                queue[index] = l;
                index = parent;
            }
        }

        public void Enqueue(PathNode pN, int prevPos)
        {
            if (tiles[pN.position].cost != 0) 
                return;

            tiles[pN.position] = new() { cost = pN.totalCost, prevVisited = prevPos};
            queue.Add(pN);
            
            BubbleUp(queue.Count - 1);
        }

        public PathNode Dequeue()
        {
            if (queue.Count > 0)
            {
                PathNode p = queue[0];
                queue[0] = queue[^1];
                queue.RemoveAt(queue.Count - 1);
                BubbleDown(0);
                return p;
            }
            return new PathNode() { position = -1};
        }
        
    }
}
