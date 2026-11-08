using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Rendering
{
    public class ChunkRenderingLinker : MonoBehaviour, IRenderInit
    {
        [NonSerialized] public Entity cameraEntity;
        [SerializeField] Material worldMaterial;
        [SerializeField] int2 cameraSize;

        NativeArray<int> renderArray;


        public void Init(GridWorld world)
        {
            var combiner = new MeshCombinerPlus(GridLevel.chunkDimensions);

            int levelCount = world.LEVEL_COUNT;

            var chunkMeshes = new Mesh[levelCount][];

            NativeArray<int> renderArray = new(50, Allocator.Persistent);

            var entityWorld = World.DefaultGameObjectInjectionWorld;

            for (int i = 0; i < levelCount; i++)
            {
                var level = world.gridLevels[i];
                chunkMeshes[i] = combiner.GetLevelMeshes(level.levelDimensions, level.renderGrid);
            }

            int maxSize = world.LargestLevelDimension * GridLevel.chunkDimensions;
            CreateTextures(maxSize);

            var WorldOverlayGrids = new NativeArray<OverlayData>[levelCount];
            var markedTilesGrids = new NativeBitArray[levelCount];

            for (int i = 0; i < levelCount; i++)
            {
                WorldOverlayGrids[i] = new NativeArray<OverlayData>(maxSize * maxSize, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                markedTilesGrids[i] = new NativeBitArray(maxSize * maxSize, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            }

            /*
                        SceneRefs.World.AttachChange(OnChange);
                        ChangeLevel(levels[0]);
                        enabled = true;*/

            var renderParams = new RenderParams(worldMaterial)
            {
                shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true,
                worldBounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000))
            };

            CreateSystems(world, combiner, chunkMeshes, renderArray, entityWorld, renderParams);
        }

        private void CreateSystems(GridWorld world, MeshCombinerPlus combiner, Mesh[][] chunkMeshes, NativeArray<int> renderArray, World entityWorld, RenderParams renderParams)
        {
            var manager = entityWorld.EntityManager;
            cameraEntity = manager.CreateEntity();

            manager.AddComponentData<RenderingComponent>(cameraEntity, new()
            {
                chunkSize = GridLevel.chunkDimensions,
                toRenderCount = 0,
                toRenderIndexArray = renderArray
            });

            manager.AddComponentData<CameraPosition>(cameraEntity, new()
            {
                CameraMoved = true,
                cameraPosition = new(world.LargestLevelDimension / 2, world.LargestLevelDimension / 2),
                cameraSize = cameraSize,
            });

            manager.AddComponentData<CameraLevelComponent>(cameraEntity, new()
            {
                level = world.CurrentLevelIndex
            });

            world.AttachChange((oldI, newI) =>
            {
                manager.SetComponentData(cameraEntity, new CameraLevelComponent() { level = newI});
            });

            var dirty = entityWorld.GetExistingSystemManaged<ChunkDirtySystem>();
            dirty.chunkMeshes = chunkMeshes;
            dirty.combiner = combiner;
            

            var render = entityWorld.GetExistingSystemManaged<ChunkRenderSystem>();
            render.chunkMeshes = chunkMeshes;
            render.renderParams = renderParams;
        }

        void CreateTextures(int maxSize)
        {
            var gridTexture = new Texture2D(maxSize, maxSize, TextureFormat.RG16, false)
            {
                filterMode = FilterMode.Point
            };

            worldMaterial.SetFloat(Shader.PropertyToID("_Width"), maxSize);
            worldMaterial.SetFloat(Shader.PropertyToID("_Height"), maxSize);

            worldMaterial.SetTexture(Shader.PropertyToID("_DigTexture"), gridTexture);
        }


        private void OnDestroy()
        {
            if(renderArray.IsCreated)
                renderArray.Dispose();
        }

    }
}
