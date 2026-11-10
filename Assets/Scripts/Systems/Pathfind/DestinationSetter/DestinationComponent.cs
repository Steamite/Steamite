using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pathing
{
    public struct DestinationComponent : IComponentData
    {
        public NativeArray<int> positions;
        public int level;
    }

    public struct WorldComponent : IComponentData
    {
        /*[ReadOnly] public NativeArray<byte> pathRegions;
        [ReadOnly] public NativeBitArray regions;*/


        [ReadOnly] public NativeArray<short> path;
        [ReadOnly] public NativeArray<int> offsets;
        [ReadOnly] public int LargestLevelDimensionScaled;
        [ReadOnly] public int largestGridSize;
        [ReadOnly] public int chunkSize;
    }

    [InternalBufferCapacity(5)]
    public struct GridLevelComponent : IBufferElementData
    {
        [ReadOnly] public NativeArray<LevelGridTile> renderGrid;
        [ReadOnly] public NativeArray<int> neighbourOffsets;
        [ReadOnly] public int levelOffsetScaled;
        [ReadOnly] public int levelOffset;
        [ReadOnly] public int levelDimensionsScaled;
        [ReadOnly] public int levelDimensions;
        [ReadOnly] public int chunkCount;
        [ReadOnly] public NativeBitArray dirtyChunks;
    }

    public struct OverlayChangeComponent : IComponentData
    {
        public bool hasChange;
    }

    public struct RenderingComponent : IComponentData
    {
        [ReadOnly] public NativeArray<int> toRenderIndexArray;
        [ReadOnly] public int chunkSize;
        public int toRenderCount;
    }
    public struct CameraPosition : IComponentData
    {
        public bool CameraMoved;
        public float2 cameraPosition;
        public float2 cameraSize;
    }
    public struct CameraLevelComponent : IComponentData
    {
        public int level;
    }

    [InternalBufferCapacity(5)]
    public struct OverlayDataComponent : IBufferElementData
    {
        public NativeBitArray markedTilesGrid;
        public NativeArray<OverlayData> overlayGrid;
    }
}
