using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    [BurstCompile]
    public struct CreateMeshJob : IJobFor
    {
        static readonly float[] TILE_HEIGHT = 
        { 
            1, 
            1, 
            0, 
            1.2f, 

            0.2f, 
            0, 
            0, 
            0 
        };

        static readonly float2[] UV_MAPPING = { 
            new(0.1f, 0.4f), 
            new(0.1f, 0.6f), 
            new(0.6f, 0.1f), 
            new(1, 1),

            new(1, 1),
            new(0, 0),
            new(0, 0),
            new(1, 0) };


        [ReadOnly] public NativeArray<LevelGridTile> gridData;


        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction] 
        internal NativeArray<Vertex> vertexes;
        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction] 
        public NativeArray<int> triangles;

        [ReadOnly] public NativeArray<int> vertexOffsets;
        [ReadOnly] public NativeArray<int> triangleOffsets;

        [ReadOnly] public int chunkWidth;
        [ReadOnly] public int paddedWidth;
        [ReadOnly] public NativeArray<int> neighbourOffset;

        [ReadOnly] public float OreX;
        [ReadOnly] public float OreY;
        [ReadOnly] public float OreZ;

        public void Execute(int index)
        {
            int sVertexIndex = vertexOffsets[index];
            int sTriangleIndex = triangleOffsets[index];

            int x = index % chunkWidth;
            int y = index / chunkWidth;

            //int dataIndex = index + paddedWidth + 1 + (y * 2);
            int dataIndex = index + paddedWidth + 1 + (index / (chunkWidth)) * 2;

            LevelGridTile tile = gridData[dataIndex];
            int type = tile.Type;

            CreateTile(dataIndex, sVertexIndex, sTriangleIndex, x, y, type);
            sVertexIndex += 4;
            sTriangleIndex += 6;

            

            /*if (type == 1)
                CreateOre(type, sVertexIndex, sTriangleIndex, x, y);
            else */if (type == 2)
                CreateWalls(index, dataIndex, sVertexIndex, sTriangleIndex, x, y, type);
        }


        void CreateTile(int dataIndex, int vertexIndex, int sTriangleIndex, int x, int y, int val)
        {
            // TODO
            float yHeight = TILE_HEIGHT[val];

            vertexes[vertexIndex]   = new(new(x,     yHeight, y),       upNormal, UV_MAPPING[val]);
            vertexes[vertexIndex+1] = new(new(x + 1, yHeight, y),       upNormal, UV_MAPPING[val]);
            vertexes[vertexIndex+2] = new(new(x,     yHeight, y + 1),   upNormal, UV_MAPPING[val]);
            vertexes[vertexIndex+3] = new(new(x + 1, yHeight, y + 1),   upNormal, UV_MAPPING[val]);

            triangles[sTriangleIndex]   = vertexIndex;
            triangles[sTriangleIndex+1] = vertexIndex + 2;
            triangles[sTriangleIndex+2] = vertexIndex + 3;

            triangles[sTriangleIndex+3] = vertexIndex;
            triangles[sTriangleIndex+4] = vertexIndex + 3;
            triangles[sTriangleIndex+5] = vertexIndex + 1;
        }

        #region Walls

        static readonly float2x2[] TOP_VERTEX_POS =
        {
            new (
                new(0, 0),
                new(0, 1)
                ),
            new (
                new(1, 1),
                new(1, 0)
                ),
            new (
                new(0, 1),
                new(1, 1)
                ),
            new (
                new(0, 0),
                new(1, 0)
                ),
        };
        static readonly float3[] NORMALS =
        {
            new(1,0,0),
            new(-1,0,0),
            new(0,0,-1),
            new(0,0,1),
        };
        static readonly float3 upNormal = new(0, 1, 0);


        /// <summary>
        /// Only joins vertexes, doesn't create new ones.
        /// </summary>
        /// <param name="dataIndex"></param>
        /// <param name="sVertexIndex"></param>
        /// <param name="sTriangleIndex"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void CreateWalls(int index, int dataIndex, int sVertexIndex, int sTriangleIndex, int x, int y, int val)
        { 
            for (int i = 0; i < 4; i++)
            {

                if (CheckWall(index, dataIndex, sVertexIndex, sTriangleIndex, x, y, val, i)) 
                {
                    sVertexIndex += 2;
                    sTriangleIndex += 6;
                }
            }
            /*if (CheckWallL(index, dataIndex, sVertexIndex, sTriangleIndex, x, y, val))
            {
                sVertexIndex += 2;
                sTriangleIndex += 6;
            }
            if (CheckWallR(index, dataIndex, sVertexIndex, sTriangleIndex, x, y, val))
            {
                sVertexIndex += 2;
                sTriangleIndex += 6;
            }
            if (CheckWallT(index, dataIndex, sVertexIndex, sTriangleIndex, x, y, val))
            {
                sVertexIndex += 2;
                sTriangleIndex += 6;
            }
            CheckWallB(index, dataIndex, sVertexIndex, sTriangleIndex, x, y, val);*/
        }


        static readonly int4[] wallTriangles =
        {
            new(0, 0, 2, 1),
            //new(0, 0, 0, 0),
            new(3, 0, 1, 1),

            new(2, 0, 3, 1),
            new(1, 1, 0, 0),
        };
        private bool CheckWall(int index, int dataIndex, int sVertexIndex, int sTriangleIndex, int x, int z, int val, int i)
        {
            int neighbourIndex = dataIndex + neighbourOffset[i];

            if (gridData[neighbourIndex].Type < 2)
            {
                float2x2 offset = TOP_VERTEX_POS[i];

                int v = vertexOffsets[index];

                vertexes[sVertexIndex] = new Vertex(
                    new(x + offset.c0.x, 1, z + offset.c0.y),
                    NORMALS[i],
                    UV_MAPPING[val]);

                vertexes[sVertexIndex + 1] = new Vertex(
                    new(x + offset.c1.x, 1, z + offset.c1.y),
                    NORMALS[i], 
                    UV_MAPPING[val]);

                // create wall
                triangles[sTriangleIndex]       = v             + wallTriangles[i].x;
                triangles[sTriangleIndex + 1]   = sVertexIndex  + wallTriangles[i].y;
                triangles[sTriangleIndex + 2]   = v             + wallTriangles[i].z;

                triangles[sTriangleIndex + 3]   = v             + wallTriangles[i].z;
                triangles[sTriangleIndex + 4]   = sVertexIndex  + wallTriangles[i].y;
                triangles[sTriangleIndex + 5]   = sVertexIndex  + wallTriangles[i].w;
                return true;
            }
            return false;
        }

        /*
         *       2 3
         *       0 1
         * ---------------
         * 2 3 | 2 3 | 2 3
         * 0 1 | 0 1 | 0 1
         * ---------------
         *       2 3
         *       0 1
         */
        #endregion

        #region Ores

        void CreateOre(int type, int sVertexIndex, int sTriangleIndex, float x, float y)
        {
            x += 0.5f;
            y += 0.5f;

            if (type == 2)
                CreateThreeOre(sVertexIndex, sTriangleIndex, x, y);
            else
                CreateRequalOre(type, sVertexIndex, sTriangleIndex, x, y);
        }

        void CreateThreeOre(int sVertexIndex, int sTriangleIndex, float x, float y)
        {
            vertexes[sVertexIndex] = new Vertex() {   position =    { x = x - OreX,  y = 1,          z = y - OreZ},
                uv = {x = 0.9f,  y = 0} };
            vertexes[sVertexIndex+1] = new Vertex() { position =    { x = x,         y = 1,          z = y + OreZ }, 
                uv = { x = 0.9f, y = 0 } };
            vertexes[sVertexIndex+2] = new Vertex() { position =    { x = x + OreX,  y = 1,          z = y - OreZ },
                uv = { x = 0.9f, y = 0 } };
            vertexes[sVertexIndex+3] = new Vertex() { position =    { x = x,         y = 1 + OreY,   z = y}, 
                uv = { x = 0.9f, y = 0 } };


            triangles[sTriangleIndex] = sVertexIndex;
            triangles[sTriangleIndex+1] = sVertexIndex + 1;
            triangles[sTriangleIndex + 2] = sVertexIndex + 3;
            
            triangles[sTriangleIndex+3] = sVertexIndex + 1;
            triangles[sTriangleIndex+4] = sVertexIndex + 2;
            triangles[sTriangleIndex+5] = sVertexIndex + 3;
            
            triangles[sTriangleIndex+6] = sVertexIndex + 2;
            triangles[sTriangleIndex+7] = sVertexIndex;
            triangles[sTriangleIndex+8] = sVertexIndex + 3;
        }

        const float Radius = 0.3f;
        const float Height = 0.4f;
        void CreateRequalOre(int type, int sVertexIndex, int sTriangleIndex, float cX, float cZ)
        {
            float angle = (2 * Mathf.PI) / type;

            //float offset = MathF.Atan(angle) * Radius;
            float cos = math.cos(angle);
            float sin = math.sin(angle);

            float x = 0;
            float z = -Radius;

            vertexes[sVertexIndex] = new Vertex()
            {
                position = { x = cX, y = 1 + Height, z = cZ },
                uv = { x = 0.9f, y = 0.1f }
            };

            int vert = sVertexIndex + 1;

            for (int i = 0; i < type; i++)
            {
                vertexes[vert++] = new Vertex()
                {
                    position = { x = cX + x, y = 1, z = cZ + z},
                    uv = { x = 0.9f, y = 0.1f }
                };

                vertexes[vert++] = new Vertex()
                {
                    position = { x = cX + x, y = 1 + Height, z = cZ + z},
                    uv = { x = 0.9f, y = 0.1f }
                };

                var tempX = x * cos - z * sin;
                z = x * sin + z * cos;
                x = tempX;

                if(i < type - 1)
                {
                    triangles[sTriangleIndex++] = vert - 2;
                    triangles[sTriangleIndex++] = vert - 1;
                    triangles[sTriangleIndex++] = vert;

                    triangles[sTriangleIndex++] = vert;
                    triangles[sTriangleIndex++] = vert - 1;
                    triangles[sTriangleIndex++] = vert + 1;

                    triangles[sTriangleIndex++] = vert - 1;
                    triangles[sTriangleIndex++] = sVertexIndex;
                    triangles[sTriangleIndex++] = vert + 1;
                }
                else
                {
                    triangles[sTriangleIndex++] = vert - 2;
                    triangles[sTriangleIndex++] = vert - 1;
                    triangles[sTriangleIndex++] = sVertexIndex + 1;

                    triangles[sTriangleIndex++] = vert - 1;
                    triangles[sTriangleIndex++] = sVertexIndex + 2;
                    triangles[sTriangleIndex++] = sVertexIndex + 1;

                    triangles[sTriangleIndex++] = vert - 1;
                    triangles[sTriangleIndex++] = sVertexIndex;
                    triangles[sTriangleIndex++] = sVertexIndex + 2;
                }
            }
        }
        #endregion
    }
}
