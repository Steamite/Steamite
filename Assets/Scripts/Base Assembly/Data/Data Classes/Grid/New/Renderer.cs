using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New
{
    public class Renderer : MonoBehaviour
    {
        NativeArray<int> toRender;
        int toRenderCount;
        NativeReference<int> ptr;
        bool cameraMoved;

        Mesh[][] chunkMeshes;
        bool[][] dirtyMeshes;

        int currentLevel;
        RenderParams renderParams;

        [SerializeField] float2 cameraPosition;
        [SerializeField] float2 cameraChunkSize;
        [SerializeField] Material worldMaterial;
        GraphicsBuffer buffer;

        int2 levelDimensions;
        int2 chunkDimension;
        int chunksCount;

        MeshCombinerPlus combiner;

        public void Awake()
        {
            enabled = false;
            combiner = new(GameLevel.chunkDimensions);
        }

        public void Init(int levelCount, int2[] worldChunkDimensions)
        {
            chunkMeshes = new Mesh[levelCount][];
            dirtyMeshes = new bool[levelCount][];
            for (int i = 0; i < levelCount; i++)
            {
                chunkMeshes[i] = new Mesh[worldChunkDimensions[i].x * worldChunkDimensions[i].y];
                dirtyMeshes[i] = new bool[worldChunkDimensions[i].x * worldChunkDimensions[i].y];
            }
            renderParams.material = worldMaterial;

            toRender = new NativeArray<int>(50, Allocator.Persistent);

            // create ALL MESHES HERE!!!!



            enabled = true;
        }

        private void OnDestroy()
        {
            if (toRender.IsCreated)
                toRender.Dispose();
            
            combiner?.Dispose();
        }

        public void ChangeLevel(int _currentLevel, int2 _worldChunkDimensions, int2 _chunkDimension)
        {
            currentLevel = _currentLevel;
            levelDimensions = _worldChunkDimensions;
            chunkDimension = _chunkDimension;
        }

        public void SetCameraPosition(float2 newPos)
        {
            cameraPosition = new(newPos.x / chunkDimension.x, newPos.y / chunkDimension.y);
            cameraMoved = true;
        }

        public void MarkDirty(int level, int index)
        {
            dirtyMeshes[level][index] = true;
        }

        private void Update()
        {
            if (cameraMoved)
            {
                NativeReference<int> nativeRenderCount = new(toRenderCount, Allocator.TempJob);
                CullJob job = new()
                {
                    cameraPosition = cameraPosition,
                    cameraSize = cameraChunkSize,
                    lastPosition = new(0, 0), //TODO maybe try to calculte a better place to start from?
                    maxSize = 50,
                    toRender = toRender,
                    toRenderCount = nativeRenderCount,
                    width = levelDimensions.x
                };
                job.Run(chunksCount);

                nativeRenderCount.Dispose();
                cameraMoved = false;
            }

            for (int i = 0; i < toRenderCount; i++)
            {
                // TODO regenerate dirty chunks
/*
                if (dirtyMeshes[currentLevel][toRender[i]])
                    chunkMeshes[currentLevel][toRender[i]] */ 

                Graphics.RenderMeshIndirect(
                    renderParams, 
                    chunkMeshes[currentLevel][toRender[i]], 
                    buffer);
            }
        }
    }
}
