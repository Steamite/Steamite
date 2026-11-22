using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Buildings;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.Systems.Rendering.Buildings
{
    public class BuildingRenderLinker : MonoBehaviour, IRenderInit
    {
        [SerializeField] Material buildingMaterial;
        [SerializeField] Material blueprintMaterial;

        public void Init(GridWorld world)
        {
            BuildingRenderSystem system = UploadBuildingData(world.WorldBuildings.GetInitialRenderData(), world.levelArray);
            world.WorldBuildings.buildingRenderer = new(system.batches, system.args, system.argBuffer);

            world.AttachChange((LevelComponent level, int i, int newIndex) =>
            {
                Shader.SetGlobalInt("_SelectedLevel", newIndex);
            });
        }

        public BuildingRenderSystem UploadBuildingData(InitialRenderData data, NativeArray<LevelComponent> levels)
        {
            InitBatches(data.buildingGroups, out RenderBatch[] renderBatches);
            
            
            SetupArgBuffer(renderBatches.Length, out IndirectDrawIndexedArgs[] args, out GraphicsBuffer argsBuffer);


            FillInstaceBuffers(data.buildings, renderBatches, levels, out int[] renderIndexes);

            InitArgs(renderBatches, args, argsBuffer);

            BuildingRenderSystem buildingRenderSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<BuildingRenderSystem>();
            buildingRenderSystem.batches = renderBatches;
            buildingRenderSystem.args = args;
            buildingRenderSystem.argBuffer = argsBuffer;
            
            
            buildingRenderSystem.Enabled = true;
            return buildingRenderSystem;

        }

        void InitBatches(BuildingGroupData[] buildingGroups, out RenderBatch[] renderBatches)
        {
            renderBatches = new RenderBatch[buildingGroups.Length + 1];
            renderBatches[0] = new(blueprintMaterial);

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

        void FillInstaceBuffers(NativeArray<BuildingRenderData> buildings, RenderBatch[] renderBatches, NativeArray<LevelComponent> levels, out int[] renderIndexes)
        {
            renderIndexes = new int[buildings.Length];


            for (int i = 0; i < buildings.Length; i++)
            {
                var build = buildings[i];
                if (!build.IsValid)
                    continue;

                int batchIndex =    build.meshIndex + 1;
                uint position =     build.encodedLocalPosition;
                int y = (int)((position >> 20) & 0x7);


                RenderBatch batch = renderBatches[batchIndex];

                position = Recalculation.BuildingPosToFixedBuildingPos(position, levels[y].offsetsScaled);
                batch.instanceData[batch.numberOfItems] = position;

                build.renderIndex = (short)(batch.startIndex + batch.numberOfItems);
                buildings[i] = build;

                batch.numberOfItems++;
            }

            foreach (var item in renderBatches)
            {
                item.UpdateInstanceData();
            }
        }
    }
}
