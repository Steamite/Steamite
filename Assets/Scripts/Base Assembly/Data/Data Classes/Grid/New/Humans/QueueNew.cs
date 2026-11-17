using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine.VFX;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans
{
    public struct PathNode
    {
        public int position;
        
        public int totalCost;
        public int costToReach;

        public int destinationIndex;
    }


    public struct Visited
    {
        public int cost;
        public int prevVisited;
    }
    public struct QueueNew : IDisposable
    {
        private static readonly ProfilerMarker k_Enque = new ProfilerMarker("Pathfind.3_Enque");
        private static readonly ProfilerMarker k_Pop = new ProfilerMarker("Pathfind.4_Pop");
        private static readonly ProfilerMarker k_Allocate = new ProfilerMarker("Pathfind.6_Allocate");

        NativeList<PathNode> queue;

        public NativeArray<Visited> tiles;
        public NativeBitArray visitedGrid;

        public int Len => queue.Length;

        public QueueNew(NativeList<PathNode> nodes, int worldSize, int expectedSize = 50)
        {
            k_Allocate.Begin();
            queue = new(expectedSize, Allocator.Temp);

            visitedGrid = new NativeBitArray(worldSize, Allocator.TempJob);
            tiles = new(worldSize, Allocator.Temp);
            k_Allocate.End();

            for (int i = 0; i < nodes.Count; i++)
            {
                Enqueue(nodes[i], -1);
            }


            nodes.Dispose();
        }

        public void Dispose()
        {
            visitedGrid.Dispose();
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
            k_Enque.Begin();
            
            if (visitedGrid.GetBits(pN.position) == 1)
            {
                k_Enque.End();
                return;
            }

            visitedGrid.Set(pN.position, true);

            tiles[pN.position] = new() { cost = pN.totalCost, prevVisited = prevPos};
            
            queue.Add(pN);
            
            BubbleUp(queue.Count - 1);
            k_Enque.End();
        }

        public PathNode Dequeue()
        {
            k_Pop.Begin();
            if (queue.Count > 0)
            {
                int len = queue.Count - 1;
                PathNode p = queue[0];
                queue[0] = queue[len];
                queue.RemoveAt(len);
                BubbleDown(0);
                k_Pop.End();
                return p;
            }
            k_Pop.End();
            return new PathNode() { position = -1};
        }
        
    }
}
