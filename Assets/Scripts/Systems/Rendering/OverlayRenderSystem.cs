using Pathing;
using Rendering;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Systems.Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(ChunkRenderSystem))]
    public partial class OverlayRenderSystem : SystemBase
    {
        public Texture2D gridTexture;
        protected override void OnCreate()
        {
            RequireForUpdate<CameraLevelComponent>();
            RequireForUpdate<OverlayChangeComponent>();
            RequireForUpdate<OverlayDataComponent>();
        }

        protected override void OnUpdate()
        {
            int levelNumber = SystemAPI.GetSingleton<CameraLevelComponent>().level;
            var change = SystemAPI.GetSingletonRW<OverlayChangeComponent>();
            if (!change.ValueRO.hasChange)
                return;
            
            change.ValueRW.hasChange = false;
            var level = SystemAPI.GetSingletonBuffer<OverlayDataComponent>()[levelNumber];

            gridTexture.SetPixelData(level.overlayGrid, 0);
            gridTexture.Apply(false, false);
        }
    }
}
