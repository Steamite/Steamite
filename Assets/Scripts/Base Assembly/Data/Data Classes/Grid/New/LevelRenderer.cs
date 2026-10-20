using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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

        [SerializeField] Texture2D toDigTexture;

        MeshCombinerPlus combiner;

        public void Awake()
        {
            enabled = false;
        }
        

        /// <summary>
        /// Values for the texture.
        /// </summary>
        [NonSerialized] NativeArray<byte>[] toDigRenderGrids;
        [NonSerialized] public NativeArray<byte> toDigRenderGrid;
        /// <summary>
        /// Values of marked rocks.
        /// </summary>
        [NonSerialized] NativeBitArray[] markedTilesGrids;
        [NonSerialized] public NativeBitArray markedTilesGrid;

        public ICustomRenderer GetCustomRenderer(int index) => customRenderers[index];

        public void Init(int levelCount, GridLevel[] levels, int maxSize)
        {
            combiner = new(GridLevel.chunkDimensions);
            chunkMeshes = new Mesh[levelCount][];
            dirtyMeshes = new bool[levelCount][];


            for (int i = 0; i < levelCount; i++)
            {
                chunkMeshes[i] = combiner.GetLevelMeshes(levels[i].levelDimensions, levels[i].renderGrid);
            }

            toDigTexture = new(maxSize, maxSize, TextureFormat.R8, false)
            {
                filterMode = FilterMode.Point
            };
            worldMaterial.SetFloat(Shader.PropertyToID("_Width"), maxSize);
            worldMaterial.SetFloat(Shader.PropertyToID("_Height"), maxSize);

            worldMaterial.SetTexture(Shader.PropertyToID("_DigTexture"), toDigTexture);
            toDigRenderGrids = new NativeArray<byte>[levelCount];
            markedTilesGrids = new NativeBitArray[levelCount];
            
            for (int i = 0; i < levelCount; i++)
            {
                toDigRenderGrids[i] = new NativeArray<byte>(maxSize * maxSize, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                markedTilesGrids[i] = new NativeBitArray(maxSize * maxSize, Allocator.Persistent, NativeArrayOptions.ClearMemory);
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


        public void UpdateToDigTexture()
        {
            toDigTexture.SetPixelData(toDigRenderGrids[currentLevel.levelNumber], 0);
            toDigTexture.Apply(false, false);
        }

        void OnChange(int _, int newI)
        {
            ChangeLevel(SceneRefs.World.GetLevel(newI));
        }



        private void OnDestroy()
        {
            if (toRender.IsCreated)
                toRender.Dispose();
            
            for (int i = 0; i < toDigRenderGrids?.Length; i++)
            {
                toDigRenderGrids[i].Dispose();
                markedTilesGrids[i].Dispose();
            }

            combiner?.Dispose();
        }

        public void ChangeLevel(GridLevel level)
        {
            currentLevel = level;
            toDigRenderGrid = toDigRenderGrids[level.levelNumber];
            markedTilesGrid = markedTilesGrids[level.levelNumber];
            chunkCount = currentLevel.levelDimensions * currentLevel.levelDimensions;
            
            UpdateToDigTexture();

            foreach (var item in customRenderers)
            {
                item.OnLevelChange(currentLevel);
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

        public void UpdateMesh(int levelIndex, int tileIndex, GridLevel gridLevel, int range = 1)
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
            if(range > 1 && offset.x != 0 && offset.y != 0)
            {
                int tempIndex = chunkIndex + offset.x + offset.y * gridLevel.levelDimensions;
                UpdateChunk(gridLevel.levelNumber, gridLevel, chunkPos + offset, tempIndex);
            }
            UpdateChunk(levelIndex, gridLevel, chunkPos, chunkIndex);
        }

        private void UpdateChunk(int levelIndex, GridLevel gridLevel, int2 chunkPos, int chunkIndex)
        {
            Mesh m = combiner.GetNewMesh(chunkPos, gridLevel);
            chunkMeshes[levelIndex][chunkIndex] = m;
        }

        int CheckNeighbours(int toModulo, int axisPosition, int2 chunkPos, int2 change, int range, int chunkIndex, int chunkIndexOffset, GridLevel level)
        {
            toModulo %= GridLevel.chunkDimensions;

            if (toModulo < range)
            {
                if (axisPosition > 0)
                {
                    UpdateChunk(level.levelNumber, level, chunkPos - change, chunkIndex - chunkIndexOffset);
                    return -1;
                }
            }
            else if (toModulo + range >= GridLevel.chunkDimensions)
            {
                if (axisPosition < level.levelDimensions - 1)
                {
                    UpdateChunk(level.levelNumber, level, chunkPos + change, chunkIndex + chunkIndexOffset);
                    return 1;
                }
            }
            return 0;
        }
    }
}
