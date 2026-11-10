using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Systems.Rendering;
using Pathing;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Rendering
{
    public class ChunkRenderingLinker : MonoBehaviour, IRenderInit, IOverlayUpdate
    {
        [NonSerialized] public Entity cameraEntity;
        [SerializeField] Material worldMaterial;
        [SerializeField] int2 cameraSize;
        [SerializeField] Texture2D gridTexture;

        MeshCombinerPlus combiner;


        public static ChunkRenderingLinker instance;
        public void Init(GridWorld world)
        {
            instance = this;
            combiner = new MeshCombinerPlus(GridLevel.chunkDimensions);

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
            CreateTextures(maxSize, levelCount, entityWorld, world);
            
            var renderParams = new RenderParams(worldMaterial)
            {
                shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true,
                worldBounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000))
            };

            AttachSystems(world, combiner, chunkMeshes, renderArray, entityWorld, renderParams);
        }

        void AttachSystems(GridWorld world, MeshCombinerPlus combiner, Mesh[][] chunkMeshes, NativeArray<int> renderArray, World entityWorld, RenderParams renderParams)
        {
            var manager = entityWorld.EntityManager;

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
                var camPos = manager.GetComponentDataRW<CameraPosition>(cameraEntity);// cameraEntity, new () { level = newI});
                camPos.ValueRW.CameraMoved = true;
            });

            var dirty = entityWorld.GetExistingSystemManaged<ChunkDirtySystem>();
            dirty.chunkMeshes = chunkMeshes;
            dirty.combiner = combiner;
            

            var render = entityWorld.GetExistingSystemManaged<ChunkRenderSystem>();
            render.chunkMeshes = chunkMeshes;
            render.renderParams = renderParams;

            var overlay = entityWorld.GetExistingSystemManaged<OverlayRenderSystem>();
            overlay.gridTexture = gridTexture;
        }

        void CreateTextures(int maxSize, int levelCount, World entityWorld, GridWorld world)
        {
            gridTexture = new Texture2D(maxSize, maxSize, TextureFormat.RG16, false)
            {
                filterMode = FilterMode.Point
            };

            worldMaterial.SetFloat(Shader.PropertyToID("_Width"), maxSize);
            worldMaterial.SetFloat(Shader.PropertyToID("_Height"), maxSize);

            worldMaterial.SetTexture(Shader.PropertyToID("_DigTexture"), gridTexture);


            var manager = entityWorld.EntityManager;

            cameraEntity = manager.CreateEntity();
            manager.AddComponentData<OverlayChangeComponent>(cameraEntity, new() { hasChange = true});
            DynamicBuffer<OverlayDataComponent> buffer = manager.AddBuffer<OverlayDataComponent>(cameraEntity);

            for (int i = 0; i < levelCount; i++)
            {
                buffer.Add(new()
                {
                    overlayGrid = world.gridLevels[i].overlayTiles,
                    markedTilesGrid = world.gridLevels[i].marked
                });
            }
        }

        void OnDestroy()
        {
            combiner?.Dispose();
        }

        public void MarkDirty()
        {
            var changes = World.DefaultGameObjectInjectionWorld.EntityManager
                .GetComponentDataRW<OverlayChangeComponent>(cameraEntity);

            changes.ValueRW.hasChange = true;
        }
    }

}
