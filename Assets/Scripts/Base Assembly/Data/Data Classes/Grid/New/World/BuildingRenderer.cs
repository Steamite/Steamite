using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.XR;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{

    public struct BuildingGroupData
    {
        public Mesh mesh;
        public float2 meshSizeHalved;
        public uint count;
    }

    struct RenderBatch
    {
        public Mesh mesh;
        public int size;
        public int startIndex;
        public uint numberOfItems;
    }

    public class BuildingRenderer : IDisposable, ICustomRenderer
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
        NativeArray<uint> instanceData;
        NativeArray<float2> batchGroupData;
        GraphicsBuffer batchGroupBuffer;


        public BuildingRenderer(Material buildingMaterial)
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

        public int[] UploadBuildingData(BuildingGroupData[] buildingGroups, BuildingRenderData[] instanceData)
        {
            SetupArgBuffer(buildingGroups.Length);

            InitBatches(buildingGroups);

            var renderIndexes = RealocateInstanceBuffer(instanceData);

            UpdateArgs(0);

            return renderIndexes;
        }

        void InitBatches(BuildingGroupData[] meshes)
        {
            renderBatches = new RenderBatch[meshes.Length];
            
            batchGroupData = new NativeArray<float2>(meshes.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            batchGroupBuffer = new(GraphicsBuffer.Target.Structured,
                meshes.Length,
                8);

            int offset = 1;
            for (int i = 0; i < meshes.Length; i++)
            {
                renderBatches[i].mesh = meshes[i].mesh;
                renderBatches[i].size = (int)math.max(meshes[i].count * 2, INCREASE_AMMOUNT);
                renderBatches[i].startIndex = offset;

                batchGroupData[i] = new(meshes[i].meshSizeHalved.x, meshes[i].meshSizeHalved.y);


                offset += renderBatches[i].size;
            }

            batchGroupBuffer.SetData(batchGroupData);
            propertyBlock.SetBuffer("_BatchGroupBuffer", batchGroupBuffer);
            
            totalCapacity = offset;
        }

        void SetupArgBuffer(int length)
        {
            args = new GraphicsBuffer.IndirectDrawIndexedArgs[length+1];

            if (argBuffer != null && argBuffer.IsValid())
                argBuffer.Dispose();

            argBuffer = new(GraphicsBuffer.Target.IndirectArguments,
                length+1,
                20);
        }

        /// <summary>
        /// Update all arguments after the specified element.
        /// </summary>
        /// <param name="startElement"></param>
        void UpdateArgs(int startElement = 0)
        {
            for (int i = startElement; i < renderBatches.Length; i++)
            {
                RenderBatch batch = renderBatches[i];

                args[i+1].indexCountPerInstance = batch.mesh.GetIndexCount(0);
                args[i+1].instanceCount = batch.numberOfItems;
                args[i+1].startIndex = batch.mesh.GetIndexStart(0);
                args[i+1].baseVertexIndex = batch.mesh.GetBaseVertex(0);
            }
            argBuffer.SetData(args);//, startElement+1, startElement+1, args.Length - (startElement+1));
        }

        int[] RealocateInstanceBuffer(BuildingRenderData[] buildings)
        {
            if (instanceBuffer != null && instanceBuffer.IsValid())
                instanceBuffer.Dispose();

            instanceBuffer = new(
                GraphicsBuffer.Target.Structured,
                totalCapacity,
                sizeof(uint));

            int[] indexes = new int[buildings.Length];

            instanceData = new NativeArray<uint>(totalCapacity, Allocator.Persistent);
            for (int i = 0; i < buildings.Length; i++)
            {
                int batchIndex = (int)buildings[i].meshIndex;
                int index = (int)(renderBatches[batchIndex].startIndex + renderBatches[batchIndex].numberOfItems);
                instanceData[index] = buildings[i].position;

                indexes[i] = index;
                renderBatches[batchIndex].numberOfItems++;
            }

            instanceBuffer.SetData(instanceData);
            propertyBlock.SetBuffer("_BuildingInstanceBuffer", instanceBuffer);

            return indexes;
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

            if (instanceData != null && instanceData.IsCreated)
                instanceData.Dispose();

            if (batchGroupData != null && batchGroupData.IsCreated)
                batchGroupData.Dispose();
        }



        int selectedBlueprint = -1;
        public void CreateBlueprint(BuildingRenderData renderData)
        {
            selectedBlueprint = (int)renderData.meshIndex;
            args[0] = args[selectedBlueprint + 1];
            args[0].instanceCount = 1;

            instanceData[0] = renderData.position;

            argBuffer.SetData(args, 0, 0, 1);
            instanceBuffer.SetData(instanceData, 0, 0, 1);
        }

        public void MoveBlueprint(uint pos)
        {
            instanceData[0] = pos;
            instanceBuffer.SetData(instanceData, 0, 0, 1);
        }

        const int INCREASE_AMMOUNT = 5;
        public void PlaceBlueprint(bool canContinue)
        {
            int dataIndex = (int)(renderBatches[selectedBlueprint].startIndex + renderBatches[selectedBlueprint].numberOfItems);
            renderBatches[selectedBlueprint].numberOfItems++;
            
            HandleInstanceRealoc(selectedBlueprint);
            UpdateArgCount(selectedBlueprint);

            instanceData[dataIndex] = instanceData[0];
            instanceBuffer.SetData(instanceData, dataIndex, dataIndex, 1);
            if(!canContinue)
                selectedBlueprint = -1;
        }

        void UpdateArgCount(int batchIndex)
        {
            int argIndex = batchIndex + 1;
            args[argIndex].instanceCount = renderBatches[batchIndex].numberOfItems;
            argBuffer.SetData(args, argIndex, argIndex, 1);
        }

        void HandleInstanceRealoc(int index)
        {
            if (renderBatches[index].numberOfItems == renderBatches[index].size)
            {
                renderBatches[index].size += INCREASE_AMMOUNT;
                totalCapacity += INCREASE_AMMOUNT;

                NativeArray<uint> tempInstanceData = new NativeArray<uint>(
                    totalCapacity, 
                    Allocator.Persistent,
                    NativeArrayOptions.ClearMemory);

                // Copy data that were not moved
                int firstCopy = renderBatches[index].startIndex + (int)renderBatches[index].numberOfItems;
                for (int i = 0; i < firstCopy; i++)
                {
                    tempInstanceData[i] = instanceData[i];
                }

                // Move start Indexes
                for (int i = index+1; i < renderBatches.Length; i++)
                {
                    renderBatches[i].startIndex += INCREASE_AMMOUNT;
                }

                // Copy data that was moved
                int end = renderBatches[^1].startIndex + (int)renderBatches[^1].numberOfItems;
                
                for (int i = renderBatches[index+1].startIndex; i < end; i++)
                {
                    tempInstanceData[i] = instanceData[i-INCREASE_AMMOUNT];
                }

                instanceData.Dispose();
                instanceData = tempInstanceData;

                var tempBuffer = new GraphicsBuffer(
                    GraphicsBuffer.Target.Structured,
                    totalCapacity,
                    sizeof(uint));
                tempBuffer.SetData(instanceData);
                
                // TODO move the building parts after the the data, and update batche data, reuse alloc function!
                propertyBlock.SetBuffer("_BuildingInstanceBuffer", tempBuffer);
                instanceBuffer.Dispose();
                instanceBuffer = tempBuffer;
            }
        }

        public void DestroyBlueprint()
        {
            selectedBlueprint = -1;
        }

        public void OnLevelChange(int levelOffsetScaled)
        {
            propertyBlock.SetInt("_LevelOffset", levelOffsetScaled);
        }

        public void Render(int2 position, int2 range)
        {
            if (selectedBlueprint > -1)
            {
                propertyBlock.SetInt("_Offset", 0);
                propertyBlock.SetInt("_GroupIndex", selectedBlueprint);
                Graphics.RenderMeshIndirect(renderParams, renderBatches[selectedBlueprint].mesh, argBuffer, 1);
            }

            for (int i = 0; i < renderBatches.Length; i++)
            {
                if (renderBatches[i].numberOfItems == 0)
                    continue;
                propertyBlock.SetInt("_Offset", renderBatches[i].startIndex);
                propertyBlock.SetInt("_GroupIndex", i);
                Graphics.RenderMeshIndirect(renderParams, renderBatches[i].mesh, argBuffer, 1, i + 1);
            }
        }
    }
}