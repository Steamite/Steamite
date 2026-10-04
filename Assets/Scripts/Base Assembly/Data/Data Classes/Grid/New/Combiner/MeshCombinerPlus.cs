using Assets.Scripts;
using NUnit.Framework.Internal;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Vertex
    {
        public float3 position;
        public float3 normal;
        public float2 uv;

        public Vertex(float3 _position, float3 _normal, float2 _uv)
        {
            position = _position;
            normal = _normal;
            uv = _uv;
        }
    }

    public class MeshCombinerPlus : IDisposable
    {
        float OreSize = 0.5f;

        NativeArray<int> _vertexCounts;
        NativeArray<int> _triangleCounts;
        NativeArray<int> _vertexOffsets;
        NativeArray<int> _triangleOffsets;
        NativeArray<LevelGridTile> _chunkData;
        NativeArray<int> _neighbourIndexOffsets;

        int2 worldSize;
        int2 levelDimensions;
        int2 chunkDimensions;
        
        readonly int chunkSize;
        readonly int chunkSizeWithPadding;

        readonly int batchSize;
        /// <summary>
        /// Use the highest possible levelDimension
        /// </summary>
        /// <param name="chunkDimension"></param>
        public MeshCombinerPlus(int2 chunkDimension, int batchSize = -1)
        {
            this.batchSize = batchSize;
            this.chunkDimensions = chunkDimension;

            chunkSize = chunkDimension.x * chunkDimension.y;
            chunkSizeWithPadding = (chunkDimension.x + 2) * (chunkDimension.y + 2);
            
            ClearArrays();

            _chunkData = new NativeArray<LevelGridTile>(chunkSizeWithPadding, Allocator.Persistent);

            _vertexCounts = new NativeArray<int>(chunkSize, Allocator.Persistent);
            _triangleCounts = new NativeArray<int>(chunkSize, Allocator.Persistent);


            _vertexOffsets = new NativeArray<int>(chunkSize, Allocator.Persistent);
            _triangleOffsets = new NativeArray<int>(chunkSize, Allocator.Persistent);
            _neighbourIndexOffsets = new NativeArray<int>(4, Allocator.Persistent);
            _neighbourIndexOffsets[0] = -1;
            _neighbourIndexOffsets[1] = 1;
        }

        private void ClearArrays()
        {
            if (_vertexCounts.IsCreated)
                _vertexCounts.Dispose();
            if (_vertexOffsets.IsCreated)
                _vertexOffsets.Dispose();
            if (_triangleCounts.IsCreated)
                _triangleCounts.Dispose();
            if (_triangleOffsets.IsCreated)
                _triangleOffsets.Dispose();
            if (_chunkData.IsCreated)
                _chunkData.Dispose();
            if (_neighbourIndexOffsets.IsCreated)
                _neighbourIndexOffsets.Dispose();
        }

        public Mesh[] GetLevelMeshes(int2 levelDimension, NativeArray<LevelGridTile> levelData)
        {
            levelDimensions = levelDimension;
            worldSize = new(levelDimensions.x * chunkDimensions.x, levelDimensions.y * chunkDimensions.y);
            int startX = -1;
            int startY = -1;

            Mesh[] meshes = new Mesh[levelDimensions.x * levelDimensions.y];
            int subMeshIndex = 0;

            for (int chunkY = 0; chunkY < levelDimensions.y; chunkY++)
            {
                for (int chunkX = 0; chunkX < levelDimensions.x; chunkX++)
                {

                    meshes[subMeshIndex++] = ComposeChunk(new int2(startX, startY), levelData);

                    startX += chunkDimensions.x;
                }

                startX = -1;
                startY += chunkDimensions.y;
            }
            return meshes;
        }

        public Mesh GetNewMesh(int2 chunkPosition, NativeArray<LevelGridTile> levelData)
        {
            return ComposeChunk(chunkPosition * chunkDimensions, levelData);
        }

        Mesh ComposeChunk(int2 startPosition, NativeArray<LevelGridTile> levelData)
        {
            FillChunkData(startPosition, _chunkData, levelData);

            int2 sums = PreCountMesh(_chunkData);
            return WriteMesh(_chunkData, sums);
        }

        void FillChunkData(int2 start, NativeArray<LevelGridTile> chunkData, NativeArray<LevelGridTile> levelData)
        {
            int chunkIndex = 0;
            
            for (int i = 0; i < chunkDimensions.y + 2; i++)
            {
                int worldY = start.y + i;
                int lineStart = worldY * worldSize.x;
                for (int j = 0; j < chunkDimensions.x + 2; j++)
                {
                    int worldX = start.x + j;

                    if (worldX < 0 || worldY < 0 || worldX >= worldSize.x || worldY >= worldSize.y)
                        chunkData[chunkIndex] = LevelGridTile.OutsideWall;
                    else
                        chunkData[chunkIndex] = levelData[lineStart + worldX];

                    chunkIndex++;
                }
            }
        }

        int2 PreCountMesh(NativeArray<LevelGridTile> chunkData)
        {
            CountJob job = new()
            {
                gridData = chunkData,

                chunkWidth = chunkDimensions.x,
                paddedWidth = chunkDimensions.x + 2,
                vertexes = _vertexCounts,
                triangles = _triangleCounts
            };

            JobHandle dependencyHandle = default;
            JobHandle countHandle;
            if (batchSize == -1)
            {
                job.Run(chunkSize);
            }
            else
            {
                countHandle = job.ScheduleParallelByRef(chunkSize, batchSize, dependencyHandle);
                countHandle.Complete();
            }



            int vertexSum = 0;
            int triangleSum = 0;
            for (int i = 0; i < chunkSize; i++)
            {
                _vertexOffsets[i] = vertexSum;
                _triangleOffsets[i] = triangleSum;

                vertexSum += _vertexCounts[i];
                triangleSum += _triangleCounts[i];
            }

            return new(vertexSum, triangleSum);
        }

        Mesh WriteMesh(NativeArray<LevelGridTile> chunkData, int2 sums)
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            meshData.SetVertexBufferParams(sums.x,
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
            meshData.SetIndexBufferParams(sums.y, UnityEngine.Rendering.IndexFormat.UInt32);


            _neighbourIndexOffsets[2] = chunkDimensions.x + 2;
            _neighbourIndexOffsets[3] = -(chunkDimensions.x + 2);

            CreateMeshJob meshJob = new()
            {
                OreX = OreSize / 2,
                OreY = 3 / 4f * OreSize,
                OreZ = (MathF.Sqrt(3) / 2f) * OreSize / 2,

                gridData = chunkData,
                vertexOffsets = _vertexOffsets,
                triangleOffsets = _triangleOffsets,
                vertexes = meshData.GetVertexData<Vertex>(),
                triangles = meshData.GetIndexData<int>(),
                chunkWidth = chunkDimensions.x,
                paddedWidth = chunkDimensions.x + 2,
                neighbourOffset = _neighbourIndexOffsets
            };

            JobHandle dependencyHandle = default;

            JobHandle meshCreationHandle;
            if (batchSize == -1)
            {
                meshJob.Run(chunkSize);
            }
            else
            {
                meshCreationHandle = meshJob.ScheduleParallelByRef(chunkSize, batchSize, dependencyHandle);
                meshCreationHandle.Complete();
            }

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, sums.y));

            Mesh mesh = new Mesh();
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh,
                MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);
           /* mesh.RecalculateNormals();
            mesh.RecalculateTangents();*/

            mesh.bounds = new Bounds
            (
                new Vector3(chunkDimensions.x / 2f, 0, chunkDimensions.y / 2f),
                new Vector3(chunkDimensions.x, 1f, chunkDimensions.y)
            );

            return mesh;
        }
/*
        public Mesh RegenerateChunk(int2 position, NativeArray<LevelGridData> levelData)
        {
            ComposeChunk(position, levelData, out Mesh mesh);
            return mesh;
        }*/

        public void Dispose()
        {
            ClearArrays();
        }
    }
}