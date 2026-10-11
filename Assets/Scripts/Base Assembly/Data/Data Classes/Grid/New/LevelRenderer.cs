using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public class LevelRenderer : MonoBehaviour
    {
        NativeArray<int> toRender;
        int toRenderCount;
        bool cameraMoved;

        Mesh[][] chunkMeshes;
        bool[][] dirtyMeshes;

        GridLevel currentLevel;
        int chunkCount;
        RenderParams renderParams;

        [SerializeField] int2 cameraPosition = new(0,0);
        [SerializeField] int2 cameraChunkSize;
        [SerializeField] Material worldMaterial;

        MeshCombinerPlus combiner;

        public void Awake()
        {
            enabled = false;
        }

        public void Init(int levelCount, GridLevel[] levels)
        {
            combiner = new(GridLevel.chunkDimensions);
            chunkMeshes = new Mesh[levelCount][];
            dirtyMeshes = new bool[levelCount][];


            for (int i = 0; i < levelCount; i++)
            {
                /*
                chunkMeshes[i] = new Mesh[levelDimensions[i].x * levelDimensions[i].y];
                dirtyMeshes[i] = new bool[levelDimensions[i].x * levelDimensions[i].y];*/

                chunkMeshes[i] = combiner.GetLevelMeshes(levels[i].levelDimensions, levels[i].renderGrid);
            }

            renderParams = new(worldMaterial) 
            {
                shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true,
                worldBounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000))
            };

            toRender = new NativeArray<int>(50, Allocator.Persistent);

            SceneRefs.World.AttachChange(OnChange);
            ChangeLevel(levels[0]);
            enabled = true;
        }

        void OnChange(int _, int newI)
        {
            ChangeLevel(SceneRefs.World.GetLevel(newI));
        }



        private void OnDestroy()
        {
            if (toRender.IsCreated)
                toRender.Dispose();
            
            combiner?.Dispose();
        }

        public void ChangeLevel(GridLevel level)
        {
            currentLevel = level;
            chunkCount = currentLevel.levelDimensions * currentLevel.levelDimensions;

            foreach (var item in customRenderers)
            {
                item.OnLevelChange(currentLevel.levelOffsetScaled);
            }
        }

        public void SetCameraPosition(float2 newPos)
        {
            cameraPosition = new(
                Mathf.FloorToInt((newPos.x) / GridLevel.chunkDimensions) - currentLevel.levelOffset, 
                Mathf.FloorToInt((newPos.y) / GridLevel.chunkDimensions) - currentLevel.levelOffset
                );
          
            cameraMoved = true;
        }

        public void MarkDirty(int level, int index)
        {
            dirtyMeshes[level][index] = true;
        }

        private void Update()
        {
            if (true || cameraMoved)
            {
                NativeReference<int> nativeRenderCount = new(0, Allocator.TempJob);
                CullJob job = new()
                {
                    cameraPosition = cameraPosition,
                    cameraSize = cameraChunkSize,
                    lastPosition = new(0, 0), //TODO maybe try to calculte a better place to start from?
                    maxSize = 50,
                    toRender = toRender,
                    toRenderCount = nativeRenderCount,
                    width = currentLevel.levelDimensions
                };
                job.Run(chunkCount);

                toRenderCount = nativeRenderCount.Value;
                nativeRenderCount.Dispose();
                cameraMoved = false;
            }

            /* for (int i = 0; i < chunksCount; i++)
             {
                 int y = i / levelDimensions[currentLevel].x;
                 int x = i - y * levelDimensions[currentLevel].x;


                 Graphics.RenderMesh(
                     renderParams,
                     chunkMeshes[currentLevel][i], 0, Matrix4x4.TRS(
                         new Vector3(
                             x * chunkDimension.x,
                             0,
                             y * chunkDimension.y
                         ),
                         Quaternion.identity,
                         Vector3.one));
             }*/

            for (int i = 0; i < toRenderCount; i++)
            {
                // TODO regenerate dirty chunks
/*
                if (dirtyMeshes[currentLevel][toRender[i]])
                    chunkMeshes[currentLevel][toRender[i]]*/
                int j = toRender[i];
                int y = j / currentLevel.levelDimensions /*+ levelOffset*/;
                int x = j - y * currentLevel.levelDimensions;

                Graphics.RenderMesh(
                    renderParams,
                    chunkMeshes[currentLevel.levelNumber][j], 0, Matrix4x4.TRS(
                        new Vector3(
                            (x + currentLevel.levelOffset) * GridLevel.chunkDimensions,
                            0,
                            (y + currentLevel.levelOffset) * GridLevel.chunkDimensions
                        ),
                        Quaternion.identity,
                        Vector3.one));
            }


            foreach (var item in customRenderers)
            {
                item.Render(cameraPosition, cameraChunkSize);
            }
        }

        readonly List<ICustomRenderer> customRenderers = new();
        public void AddRenderer(ICustomRenderer renderer)
        {
            customRenderers.Add(renderer);
        }

        public void UpdateMesh(int levelIndex, int tileIndex, GridLevel gridLevel)
        {
            int y = tileIndex / gridLevel.levelDimensionsScaled;
            int x = tileIndex % gridLevel.levelDimensionsScaled;

            int2 chunkPos = new 
                int2(x, y) / GridLevel.chunkDimensions;

            int chunkIndex = (chunkPos.y * gridLevel.levelDimensions) + chunkPos.x;

            // neighbours 
            CheckNeighbours(y, chunkPos.y, chunkPos, new(0, 1), chunkIndex, gridLevel.levelDimensions, gridLevel);
            CheckNeighbours(x, chunkPos.x, chunkPos, new(1, 0), chunkIndex, 1, gridLevel);

            UpdateChunk(levelIndex, gridLevel, chunkPos, chunkIndex);
        }

        private void UpdateChunk(int levelIndex, GridLevel gridLevel, int2 chunkPos, int chunkIndex)
        {
            Mesh m = combiner.GetNewMesh(chunkPos, gridLevel);
            chunkMeshes[levelIndex][chunkIndex] = m;
        }

        void CheckNeighbours(int toModulo, int axisPosition, int2 chunkPos, int2 change, int chunkIndex, int chunkIndexOffset, GridLevel level)
        {
            toModulo %= GridLevel.chunkDimensions;

            if (toModulo == 0)
            {
                if (axisPosition > 0)
                {
                    UpdateChunk(level.levelNumber, level, chunkPos - change, chunkIndex - chunkIndexOffset);
                }

            }
            else if (toModulo == GridLevel.chunkDimensions - 1)
            {
                if (axisPosition < level.levelDimensions - 1)
                {
                    UpdateChunk(level.levelNumber, level, chunkPos + change, chunkIndex + chunkIndexOffset);
                }
            }
        }
    }
}
