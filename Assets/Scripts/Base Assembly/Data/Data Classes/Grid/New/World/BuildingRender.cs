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
        public uint numberOfItems;
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

        int totalCapacity;
        /// <summary>
        /// Instance buffer arguments (mesh and instance numbers)
        /// </summary>
        GraphicsBuffer.IndirectDrawIndexedArgs[] args;

        /// <summary>
        /// RenderData
        /// </summary>
        NativeArray<uint> data;

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
        public int[] UploadBuildingData(Mesh[] meshes, uint[] count, BuildingRenderData[] instanceData)
        {
            SetupArgBuffer(meshes.Length);

            InitBatches(meshes, count);

            return RealocateInstanceBuffer(instanceData);
        }

        void InitBatches(Mesh[] meshes, uint[] count)
        {
            CombineInstance[] combines = new CombineInstance[meshes.Length];

            renderBatches = new RenderBatch[meshes.Length];
            uint offset = 0;
            uint indexStart  = 0;
            uint vertexStart = 0;

            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh srcMesh = meshes[i];
                CombineInstance combineInstance = new()
                {
                    mesh = srcMesh,
                    transform = Matrix4x4.identity
                };
                combines[i] = combineInstance;

                renderBatches[i] = new()
                {
                    numberOfItems = count[i],
                    startIndex = offset
                };

                uint indexCount = srcMesh.GetIndexCount(0);

                args[i] = new GraphicsBuffer.IndirectDrawIndexedArgs()
                {
                    indexCountPerInstance = indexCount,
                    instanceCount = renderBatches[i].numberOfItems,
                    startIndex = indexStart,
                    baseVertexIndex = vertexStart,
                    startInstance = renderBatches[i].startIndex,
                };

                indexStart += indexCount;
                vertexStart += (uint)srcMesh.vertexCount;

                offset += math.min(count[i] * 2, 10); 
            }
            totalCapacity = (int)offset;

            mergedMesh = new()
            {
                indexFormat = IndexFormat.UInt32,
            };
            mergedMesh.CombineMeshes(combines, true, true);
            mergedMesh.UploadMeshData(true);

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
        /// Update all arguments after the specified element.
        /// </summary>
        /// <param name="startElement"></param>
        void UpdateArgs(int startElement = 0)
        {
            int startIndex;
            for (int i = startElement; i < renderBatches.Length; i++)
            {
                RenderBatch batch = renderBatches[i];

                startIndex = i * 5;
                args[i].instanceCount = batch.numberOfItems;
                args[i].startIndex = batch.startIndex;
            }
            argBuffer.SetData(args);
        }

        int[] RealocateInstanceBuffer(BuildingRenderData[] instanceData)
        {
            if(instanceBuffer != null && instanceBuffer.IsValid())
                instanceBuffer.Dispose();

            instanceBuffer = new(
                GraphicsBuffer.Target.Structured,
                totalCapacity,  
                sizeof(uint));

            int[] indexes = new int[instanceData.Length];

            data = new NativeArray<uint>(totalCapacity, Allocator.Persistent);
            for (int i = 0; i < instanceData.Length; i++)
            {
                int batchIndex = (int)instanceData[i].meshIndex;
                int index = (int)(renderBatches[batchIndex].startIndex + renderBatches[batchIndex].currentAssigned);
                data[index] = instanceData[i].position;

                indexes[i] = index;
                renderBatches[batchIndex].currentAssigned++;
            }

            instanceBuffer.SetData(data);
            propertyBlock.SetBuffer("_BuildingInstanceBuffer", instanceBuffer);

            return indexes;
        }

        public void UpdateBuilding(int index, BuildingRenderData data)
        {
            
        }

        public void CreateBuilding(BuildingRenderData data)
        {

            //UpdateArg(data.y);
        }

        public void RemoveBuilding(int index, int batch)
        {
            //UpdateArg(data.y)
        }


        public void Dispose()
        {
            if (instanceBuffer != null && instanceBuffer.IsValid())
                instanceBuffer.Dispose();
            if (argBuffer != null && argBuffer.IsValid())
                argBuffer.Dispose();

            if (data != null && data.IsCreated)
                data.Dispose();
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