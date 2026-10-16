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
        public Mesh mesh;
        public int size;
        public int startIndex;
        public uint numberOfItems;
        public uint currentAssigned;
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

        Mesh mergedMesh;

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
        public int[] UploadBuildingData(Mesh[] meshes, uint[] count, BuildingRenderData[] instanceData)
        {
            SetupArgBuffer(meshes.Length);

            InitBatches(meshes, count);
            
            UpdateArgs(0);

            return RealocateInstanceBuffer(instanceData);
        }

        void InitBatches(Mesh[] meshes, uint[] count)
        {
            renderBatches = new RenderBatch[meshes.Length];
            int offset = 1;
            for (int i = 0; i < meshes.Length; i++)
            {
                renderBatches[i] = new()
                {
                    mesh = meshes[i],
                    numberOfItems = count[i],
                    size = (int)math.max(count[i] * 2, 10),
                    startIndex = offset
                };
                offset += renderBatches[i].size;
            }
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
                int index = (int)(renderBatches[batchIndex].startIndex + renderBatches[batchIndex].currentAssigned);
                instanceData[index] = buildings[i].position;

                indexes[i] = index;
                renderBatches[batchIndex].currentAssigned++;
            }

            instanceBuffer.SetData(instanceData);
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

            if (instanceData != null && instanceData.IsCreated)
                instanceData.Dispose();
        }



        int selectedBlueprint = -1;
        internal void CreateBlueprint(BuildingRenderData renderData)
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
                Graphics.RenderMeshIndirect(renderParams, renderBatches[selectedBlueprint].mesh, argBuffer, 1);
            }

            for (int i = 0; i < renderBatches.Length; i++)
            {
                if (renderBatches[i].numberOfItems == 0)
                    continue;
                propertyBlock.SetInt("_Offset", renderBatches[i].startIndex);
                Graphics.RenderMeshIndirect(renderParams, renderBatches[i].mesh, argBuffer, 1, i + 1);
            }
        }

    }
}