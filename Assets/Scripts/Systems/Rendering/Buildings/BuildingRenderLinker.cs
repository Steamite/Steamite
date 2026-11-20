using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using Unity.Entities;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.Systems.Rendering.Buildings
{
    public class BuildingRenderLinker : MonoBehaviour, IRenderInit
    {
        const int INCREASE_AMMOUNT = 5;
        [SerializeField] Material buildingMaterial;

        public void Init(GridWorld world)
        {
            UploadBuildingData(world.WorldBuildings.GetInitialRenderData());
        }

        public void UploadBuildingData(InitialRenderData data)
        {
            InitBatches(data.buildingGroups, out RenderBatch[] renderBatches);
            
            
            SetupArgBuffer(renderBatches.Length, out IndirectDrawIndexedArgs[] args, out GraphicsBuffer argsBuffer);


            FillInstaceBuffers(data.instanceData, renderBatches, out int[] renderIndexes);

            FillRenderIndex(data.buildings, renderIndexes);

            InitArgs(renderBatches, args, argsBuffer);

            BuildingRenderSystem buildingRenderSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<BuildingRenderSystem>();
            buildingRenderSystem.batches = renderBatches;
            buildingRenderSystem.args = args;
            buildingRenderSystem.argBuffer = argsBuffer;
            
            
            buildingRenderSystem.Enabled = true;
        }

        private void FillRenderIndex(BuildingNew[] buildings, int[] renderIndexes)
        {
            int j = 0;

            for (int i = 0; i < buildings.Length; i++)
            {
                if (buildings[i] == null)
                    continue;

                buildings[i].renderIndex = renderIndexes[j];
                j++;
            }
        }

        void InitBatches(BuildingGroupData[] buildingGroups, out RenderBatch[] renderBatches)
        {
            renderBatches = new RenderBatch[buildingGroups.Length + 1];
            renderBatches[0] = new();

            int offset = 1;
            int j = 1;
            for (int i = 0; i < buildingGroups.Length; i++)
            {
                renderBatches[j] = new(buildingGroups[i].mesh, buildingMaterial, buildingGroups[i].count, offset, i);
                offset += renderBatches[j].size;

                j++;
            }
        }

        void SetupArgBuffer(int length, out IndirectDrawIndexedArgs[] args, out GraphicsBuffer argBuffer)
        {
            args = new IndirectDrawIndexedArgs[length];

            argBuffer = new(GraphicsBuffer.Target.IndirectArguments,
                length,
                20);
        }

        /// <summary>
        /// Update all arguments.
        /// </summary>
        void InitArgs(RenderBatch[] renderBatches, IndirectDrawIndexedArgs[] args, GraphicsBuffer argBuffer)
        {
            for (int i = 1; i < renderBatches.Length; i++)
            {
                RenderBatch batch = renderBatches[i];

                args[i].indexCountPerInstance = batch.mesh.GetIndexCount(0);
                args[i].instanceCount = (uint)batch.numberOfItems;
                args[i].startIndex = batch.mesh.GetIndexStart(0);
                args[i].baseVertexIndex = batch.mesh.GetBaseVertex(0);
                
            }
            argBuffer.SetData(args);
        }

        void FillInstaceBuffers(BuildingRenderData[] buildings, RenderBatch[] renderBatches, out int[] renderIndexes)
        {
            renderIndexes = new int[buildings.Length];


            for (int i = 0; i < buildings.Length; i++)
            {
                int batchIndex = (int)buildings[i].meshIndex + 1;
                RenderBatch batch = renderBatches[batchIndex];

                batch.instanceData[batch.numberOfItems] = buildings[i].encodedLocalPosition;

                renderIndexes[i] = batch.startIndex + batch.numberOfItems;
                batch.numberOfItems++;
            }

            foreach (var item in renderBatches)
            {
                item.UpdateInstanceData();
            }
        }
    }
}
