using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Pathing;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(ChunkDirtySystem))]
    public partial class ChunkRenderSystem : SystemBase
    {
        public RenderParams renderParams;
        public Mesh[][] chunkMeshes;
        protected override void OnCreate()
        {
            RequireForUpdate<CameraLevelComponent>();
            RequireForUpdate<RenderingComponent>();
            RequireForUpdate<GridLevelComponent>();
        }

        protected override void OnUpdate()
        {
            int levelNumber = SystemAPI.GetSingleton<CameraLevelComponent>().level;
            RenderingComponent rendering = SystemAPI.GetSingleton<RenderingComponent>();
            GridLevelComponent level = SystemAPI.GetSingletonBuffer<GridLevelComponent>()[levelNumber];

            int toRenderCount = rendering.toRenderCount;
            for (int i = 0; i < toRenderCount; i++)
            {
                int j = rendering.toRenderIndexArray[i];
                int y = j / level.levelDimensions;
                int x = j - y * level.levelDimensions;

                Graphics.RenderMesh(
                    renderParams,
                    chunkMeshes[levelNumber][j], 0, Matrix4x4.TRS(
                        new Vector3(
                            (x + level.levelOffset) * GridLevel.chunkDimensions,
                            0,
                            (y + level.levelOffset) * GridLevel.chunkDimensions
                        ),
                        Quaternion.identity,
                        Vector3.one));
            }
        }
    }
}
