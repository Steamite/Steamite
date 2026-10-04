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

        int currentLevel;
        RenderParams renderParams;

        [SerializeField] int2 cameraPosition = new(0,0);
        [SerializeField] int2 cameraChunkSize;
        [SerializeField] Material worldMaterial;

        int2[] levelDimensions;
        int2 chunkDimension;
        int chunksCount;

        MeshCombinerPlus combiner;

        public void Awake()
        {
            enabled = false;
        }

        public void Init(int levelCount, GridLevel[] levels)
        {
            chunkDimension = GridLevel.chunkDimensions;

            combiner = new(chunkDimension);
            chunkMeshes = new Mesh[levelCount][];
            dirtyMeshes = new bool[levelCount][];
            levelDimensions = new int2[levelCount];


            for (int i = 0; i < levelCount; i++)
            {
                levelDimensions[i] = levels[i].levelDimensions;
/*
                chunkMeshes[i] = new Mesh[levelDimensions[i].x * levelDimensions[i].y];
                dirtyMeshes[i] = new bool[levelDimensions[i].x * levelDimensions[i].y];*/

                chunkMeshes[i] = combiner.GetLevelMeshes(levelDimensions[i], levels[i].renderGrid);
            }

            renderParams = new(worldMaterial) 
            {
                shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true,
                worldBounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000))
            };

            toRender = new NativeArray<int>(50, Allocator.Persistent);

            GridWorld.AttachChange((_, newI) => ChangeLevel(newI));
            ChangeLevel(0);
            enabled = true;
        }



        private void OnDestroy()
        {
            if (toRender.IsCreated)
                toRender.Dispose();
            
            combiner?.Dispose();
        }

        public void ChangeLevel(int _currentLevel)
        {
            currentLevel = _currentLevel;
            chunksCount = levelDimensions[currentLevel].x * levelDimensions[currentLevel].y;
        }

        public void SetCameraPosition(float2 newPos)
        {
            cameraPosition = new(Mathf.FloorToInt(newPos.x / chunkDimension.x), Mathf.FloorToInt(newPos.y / chunkDimension.y));
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
                    width = levelDimensions[currentLevel].x
                };
                job.Run(chunksCount);

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
                int y = j / levelDimensions[currentLevel].x;
                int x = j - y * levelDimensions[currentLevel].x;


                Graphics.RenderMesh(
                    renderParams,
                    chunkMeshes[currentLevel][j], 0, Matrix4x4.TRS(
                        new Vector3(
                            x * chunkDimension.x,
                            0,
                            y * chunkDimension.y
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
    }
}
