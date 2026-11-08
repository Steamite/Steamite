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
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(ChunkCullSystem))]
    public partial class ChunkDirtySystem : SystemBase
    {
        public Mesh[][] chunkMeshes;
        public MeshCombinerPlus combiner;
        //NativeList<int> rerenderedThisFrame;
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<CameraPosition>();
            RequireForUpdate<RenderingComponent>();
            RequireForUpdate<GridLevelComponent>();
        }

        protected override void OnUpdate()
        {
            var levelIndex = SystemAPI.GetSingleton<CameraLevelComponent>().level;
            var render = SystemAPI.GetSingleton<RenderingComponent>();
            var level = SystemAPI.GetSingletonBuffer<GridLevelComponent>()[levelIndex];

            //rerenderedThisFrame.Clear();

            Mesh[] meshes = chunkMeshes[levelIndex];
            for (int i = 0; i < render.toRenderCount; i++)
            {
                int index = render.toRenderIndexArray[i];
                if (level.dirtyChunks.GetBits(index) == 1)
                {
                    level.dirtyChunks.Set(index, false);
                    // re-render
                    meshes[index] = combiner.GetNewMesh(
                        new(
                            index % level.levelDimensions,
                            index / level.levelDimensions),
                        level.levelDimensionsScaled, 
                        level.renderGrid);
                }
            }
        }
    }
}
