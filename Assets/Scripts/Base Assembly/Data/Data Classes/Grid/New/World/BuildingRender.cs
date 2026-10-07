using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{
    struct RenderBatch
    {
        public Mesh mesh;
        public uint startIndex;
        public uint size;
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
        private RenderParams renderParamsSingle;
        private MaterialPropertyBlock propertyBlock;

        int totalCapacity;
        NativeArray<uint> args;
        /// <summary>
        /// RenderData
        /// </summary>
        NativeArray<uint> data;

        public BuildingRender(Material buildingMaterial, Material tempMaterial)
        {
            propertyBlock = new();
            renderParams = new(buildingMaterial) { matProps = propertyBlock, worldBounds = new Bounds(Vector3.zero, new Vector3(100000, 10000, 100000)) };/*
            {
                matProps = propertyBlock,
                material = buildingMaterial,
                shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true,
                worldBounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000))
            };*/
            renderParamsSingle = new(tempMaterial);
        }
        public int[] UploadBuildingData(Mesh[] meshes, uint[] count, uint2[] instanceData)
        {
            InitBatches(meshes, count);

            SetupArgBuffer();

            return RealocateInstanceBuffer(instanceData);
        }

        void InitBatches(Mesh[] meshes, uint[] count)
        {
            renderBatches = new RenderBatch[meshes.Length];
            uint offset = 0;
            for (int i = 0; i < meshes.Length; i++)
            {
                renderBatches[i] = new()
                {
                    mesh = meshes[i],
                    numberOfItems = count[i],
                    size = math.min(count[i] * 2, 10),
                    startIndex = offset
                };
                offset += renderBatches[i].size; 
            }
            totalCapacity = (int)offset;
        }

        void SetupArgBuffer()
        {
            if(argBuffer != null && argBuffer.IsValid())
                argBuffer.Dispose();

            argBuffer = new(
                GraphicsBuffer.Target.IndirectArguments, 
                renderBatches.Length * 5, 
                sizeof(uint));

            args = new NativeArray<uint>(renderBatches.Length * 5, Allocator.Persistent);
            UpdateArgs();

            propertyBlock.SetFloat("_CellSize", 1);
            propertyBlock.SetFloat("_HeightY", 1);
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
                args[startIndex + 0] = batch.mesh.GetIndexCount(0);
                args[startIndex + 1] = batch.numberOfItems;
                args[startIndex + 2] = batch.mesh.GetIndexStart(0);
                args[startIndex + 3] = batch.mesh.GetBaseVertex(0);
                args[startIndex + 4] = batch.startIndex;
            }
            argBuffer.SetData(args, startElement*5, startElement*5, (renderBatches.Length - startElement)*5);
        }

        /// <summary>
        /// Update only one argument, Used when an item is added but the collection doesn't need to be resized.
        /// </summary>
        /// <param name="startElement"></param>
        void UpdateArg(int startElement)
        {
            int startIndex = startElement * 5;

            RenderBatch batch = renderBatches[startElement];

            args[startIndex + 0] = batch.mesh.GetIndexCount(0);
            args[startIndex + 1] = batch.numberOfItems;
            args[startIndex + 2] = batch.mesh.GetIndexStart(0);
            args[startIndex + 3] = batch.mesh.GetBaseVertex(0);
            args[startIndex + 4] = batch.startIndex;

            argBuffer.SetData(args, startElement*5, startElement*5, 5);
        }


        int[] RealocateInstanceBuffer(uint2[] instanceData)
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
                int batchIndex = (int)instanceData[i].y;
                int index = (int)(renderBatches[batchIndex].startIndex + renderBatches[batchIndex].currentAssigned);
                data[index] = instanceData[i].x;

                indexes[i] = index;
                renderBatches[batchIndex].currentAssigned++;
            }

            instanceBuffer.SetData(data);
            propertyBlock.SetBuffer("_BuildingInstanceBuffer", instanceBuffer);

            return indexes;
        }

        public void UpdateBuilding(int index, uint data)
        {
            
        }

        public void CreateBuilding(uint2 data)
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

            if (args != null && args.IsCreated)
                args.Dispose();
            if (data != null && data.IsCreated)
                data.Dispose();
        }


        public void OnLevelChange(int levelOffsetScaled)
        {
            propertyBlock.SetInt("_LevelOffset", levelOffsetScaled);
        }
        public void Render(int2 position, int2 range)
        {
            for (int i = 0; i < renderBatches.Length; i++)
            {
                /*for (int j = 0; j < renderBatches[i].numberOfItems; j++)
                {
                    Graphics.RenderMesh(renderParamsSingle, renderBatches[i].mesh, 0, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));
                }*/
                if (renderBatches[i].numberOfItems == 0)
                    continue;
                
                Graphics.RenderMeshIndirect(renderParams, renderBatches[i].mesh, argBuffer, 1, i*5*sizeof(uint));
            }
        }
    }
}
