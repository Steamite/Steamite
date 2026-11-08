using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Pathing;
using System;
using System.Collections.Generic;
using System.Text;
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

            Mesh[] meshes = chunkMeshes[levelIndex];
            for (int i = 0; i < render.toRenderCount; i++)
            {
                int index = render.toRenderIndexArray[i];
                if (level.dirtyChunks.GetBits(index) == 1)
                {
                    // re-render
                    meshes[index] = combiner.GetNewMesh(
                        new(
                            index % GridLevel.chunkDimensions,
                            index / GridLevel.chunkDimensions),
                        level.levelDimensionsScaled, 
                        level.renderGrid);
                }
            }
        }

/*
        public void UpdateMesh(int levelIndex, int tileIndex, GridLevelComponent gridLevel, int range = 1)
        {
            int y = tileIndex / gridLevel.levelDimensionsScaled;
            int x = tileIndex % gridLevel.levelDimensionsScaled;

            int2 chunkPos = new
                int2(x, y) / GridLevel.chunkDimensions;

            int chunkIndex = (chunkPos.y * gridLevel.levelDimensions) + chunkPos.x;

            // neighbours 
            int2 offset;
            offset.y = CheckNeighbours(y, chunkPos.y, chunkPos, new(0, 1), range, chunkIndex, gridLevel.levelDimensions, gridLevel);
            offset.x = CheckNeighbours(x, chunkPos.x, chunkPos, new(1, 0), range, chunkIndex, 1, gridLevel);
            if (range > 1 && offset.x != 0 && offset.y != 0)
            {
                int tempIndex = chunkIndex + offset.x + offset.y * gridLevel.levelDimensions;
                UpdateChunk(levelIndex, gridLevel, chunkPos + offset, tempIndex);
            }
            UpdateChunk(levelIndex, gridLevel, chunkPos, chunkIndex);
        }

        private void UpdateChunk(int levelIndex, GridLevelComponent gridLevel, int2 chunkPos, int chunkIndex)
        {
            Mesh m = combiner.GetNewMesh(chunkPos, gridLevel.levelDimensions, gridLevel.renderGrid);
            chunkMeshes[levelIndex][chunkIndex] = m;
        }

        int CheckNeighbours(int toModulo, int axisPosition, int2 chunkPos, int2 change, int range, int chunkIndex, int chunkIndexOffset, GridLevelComponent level)
        {
            toModulo %= GridLevel.chunkDimensions;

            if (toModulo < range)
            {
                if (axisPosition > 0)
                {
                    UpdateChunk(level, level, chunkPos - change, chunkIndex - chunkIndexOffset);
                    return -1;
                }
            }
            else if (toModulo + range >= GridLevel.chunkDimensions)
            {
                if (axisPosition < level.levelDimensions - 1)
                {
                    UpdateChunk(level, level, chunkPos + change, chunkIndex + chunkIndexOffset);
                    return 1;
                }
            }
            return 0;
        }*/
    }
}
