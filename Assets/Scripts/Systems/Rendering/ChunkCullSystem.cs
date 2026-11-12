using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Pathing;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct ChunkCullSystem : ISystem
    {
        public readonly void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CameraPosition>();
            state.RequireForUpdate<RenderingComponent>();
            state.RequireForUpdate<LevelComponent>();
        }

        public readonly void OnUpdate(ref SystemState state)
        {
            RefRW<CameraPosition> position = SystemAPI.GetSingletonRW<CameraPosition>();
            if (!position.ValueRO.CameraMoved)
            {
                return;
            }

            int levelIndex = SystemAPI.GetSingleton<CameraLevelComponent>().level;

            RefRW<RenderingComponent> chunks = SystemAPI.GetSingletonRW<RenderingComponent>();
            LevelComponent level = SystemAPI.GetSingletonBuffer<LevelComponent>()[levelIndex];

            int levelDimensions = level.dimensions;

            NativeReference<int> nativeRenderCount = new(0, Allocator.TempJob);

            float2 camPos = math.floor(position.ValueRO.cameraPosition / GridLevel.chunkDimensions) - level.offsets;

            CullJob job = new()
            {
                cameraPosition = camPos,
                cameraSize = position.ValueRO.cameraSize,
                //lastPosition = new(0, 0), //TODO maybe try to calculte a better place to start from?
                maxSize = 50,
                toRender = chunks.ValueRO.toRenderIndexArray,
                toRenderCount = nativeRenderCount,
                width = levelDimensions
            };
            job.Run(level.chunkCount);

            chunks.ValueRW.toRenderCount = nativeRenderCount.Value;
            nativeRenderCount.Dispose();

            position.ValueRW.CameraMoved = false;

        }
    }
}