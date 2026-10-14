using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.XR;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{
    struct RenderBatch
    {
        public uint startIndex;
        public uint currentAssigned;
    }

    public class BuildingRender : IDisposable, ICustomRenderer
    {
        /// <summary>
        /// The Data for compute shader
        /// </summary>
        GraphicsBuffer instanceBuffer;

        /// <summary>
        /// Smaller command buffer (used for ordering the render)
        /// </summary>
        GraphicsBuffer argBuffer;

        RenderBatch[] renderBatches;

        private RenderParams renderParams;
        private MaterialPropertyBlock propertyBlock;

        int instanceAllocated;
        int instanceNumber;
        /// <summary>
        /// Instance buffer arguments (mesh and instance numbers)
        /// </summary>
        GraphicsBuffer.IndirectDrawIndexedArgs[] args;

        /// <summary>
        /// RenderData
        /// </summary>
        NativeArray<uint> instanceData;

        Mesh mergedMesh;

        public BuildingRender(Material buildingMaterial)
        {
            propertyBlock = new();

            propertyBlock.SetFloat("_CellSize", 1);
            propertyBlock.SetFloat("_HeightY", 1);

            renderParams = new(buildingMaterial) 
            { 
                matProps = propertyBlock, 
                worldBounds = new Bounds(Vector3.zero, new Vector3(100000, 10000, 100000)) 
            };

        }
        public int[] UploadStartingBuildingData(Mesh[] meshes, uint[] count, BuildingRenderData[] inputData)
        {
            SetupArgBuffer(meshes.Length);

            var indexes = CombineMesh(meshes);
            InitBatchesCombineMesh(indexes, count);

            return AllocateInstaceBuffer(inputData);
        }

        uint2[] CombineMesh(Mesh[] meshes)
        {
            CombineInstance[] combines = new CombineInstance[meshes.Length];
            uint2[] meshCounts = new uint2[meshes.Length];
            for (int i = 0; i < meshes.Length; i++)
            {

                Mesh srcMesh = meshes[i];
                CombineInstance combineInstance = new()
                {
                    mesh = srcMesh,
                    transform = Matrix4x4.identity
                };
                combines[i] = combineInstance;
                meshCounts[i] = new(srcMesh.GetIndexCount(0), (uint)srcMesh.vertexCount);
            }

            // MERGE MESH
            mergedMesh = new()
            {
                indexFormat = IndexFormat.UInt32,
            };
            mergedMesh.CombineMeshes(combines, true, true);
            mergedMesh.UploadMeshData(true);

            return meshCounts;
        }

        void InitBatchesCombineMesh(uint2[] meshData, uint[] count)
        {
            renderBatches = new RenderBatch[meshData.Length];
            uint usedCount = 0;
            uint indexStart  = 0;
            uint vertexStart = 0;

            for (int i = 0; i < meshData.Length; i++)
            {
                uint objectsInBatch = count[i];
                // BATCH
                var batch = new RenderBatch()
                {
                    currentAssigned = 0,
                    startIndex = usedCount
                };
                renderBatches[i] = batch;

                // ARGS
                uint indexCount = meshData[i].x;
                uint vertexCount = meshData[i].y;

                args[i] = new GraphicsBuffer.IndirectDrawIndexedArgs()
                {
                    indexCountPerInstance = indexCount,
                    instanceCount = objectsInBatch,
                    startIndex = indexStart,
                    baseVertexIndex = vertexStart,
                    startInstance = batch.startIndex,
                };

                indexStart += indexCount;
                vertexStart += vertexCount;

                usedCount += objectsInBatch; 
            }
            instanceAllocated = (int)math.max(usedCount, 20);


            // SETS the args
            argBuffer.SetData(args);
        }

        void SetupArgBuffer(int length)
        {
            args = new GraphicsBuffer.IndirectDrawIndexedArgs[length];

            if (argBuffer != null && argBuffer.IsValid())
                argBuffer.Dispose();

            argBuffer = new(GraphicsBuffer.Target.IndirectArguments,
                length,
                20);
        }

        /// <summary>
        /// Update all arguments after the specified element (move left or right).
        /// </summary>
        /// <param name="startElement"></param>
        void UpdateArgs(uint startElement, bool add)
        {
            if(add)
                args[startElement].instanceCount++;
            else
                args[startElement].instanceCount--;

            uint initIndex = args[startElement].startIndex;

            for (uint i = startElement; i < renderBatches.Length; i++)
            {
                args[i].startIndex = initIndex;
                initIndex += args[i].instanceCount;
            }

            argBuffer.SetData(args);
        }

        int[] AllocateInstaceBuffer(BuildingRenderData[] renderData)
        {
            if(instanceBuffer != null && instanceBuffer.IsValid())
                instanceBuffer.Dispose();

            instanceBuffer = new(
                GraphicsBuffer.Target.Structured,
                instanceAllocated,  
                sizeof(uint));

            int[] indexes = new int[renderData.Length];

            instanceData = new NativeArray<uint>(instanceAllocated, Allocator.Persistent);
            for (int i = 0; i < renderData.Length; i++)
            {
                int batchIndex = (int)renderData[i].meshIndex;
                int index = (int)(renderBatches[batchIndex].startIndex + renderBatches[batchIndex].currentAssigned);
                instanceData[index] = renderData[i].position;

                indexes[i] = index;
                renderBatches[batchIndex].currentAssigned++;
            }

            instanceBuffer.SetData(instanceData);
            propertyBlock.SetBuffer("_BuildingInstanceBuffer", instanceBuffer);

            return indexes;
        }

        public void UpdateBuilding(int index, BuildingRenderData building)
        {
            
        }

        public void CreateBuilding(BuildingRenderData building)
        {
            UpdateArgs(building.meshIndex, true);
            if(instanceNumber == instanceAllocated - 1)
            {
                NativeArray<uint> biggerData = new NativeArray<uint>(instanceAllocated * 2, Allocator.Persistent);
                biggerData.CopyFrom(instanceData);
                instanceData.Dispose();
                instanceData = biggerData;
            }
        }

        public void RemoveBuilding(int index)
        {
            //UpdateArg(data.y)
        }


        public void Dispose()
        {
            if (instanceBuffer != null && instanceBuffer.IsValid())
                instanceBuffer.Dispose();
            if (argBuffer != null && argBuffer.IsValid())
                argBuffer.Dispose();

            if (instanceData != null && instanceData.IsCreated)
                instanceData.Dispose();
        }


        public void OnLevelChange(int levelOffsetScaled)
        {
            propertyBlock.SetInt("_LevelOffset", levelOffsetScaled);
        }

        public void Render(int2 position, int2 range)
        {
            Graphics.RenderMeshIndirect(
                renderParams,
                mergedMesh,
                argBuffer,
                1,
                0);
        }
    }
}