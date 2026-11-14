using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct WorldComponent : IComponentData
{
    /*[ReadOnly] public NativeArray<byte> pathRegions;
    [ReadOnly] public NativeBitArray regions;*/
    [ReadOnly] public NativeArray<short> path;
    [ReadOnly] public NativeArray<int> offsets;
    [ReadOnly] public int LargestLevelDimensionScaled;
    [ReadOnly] public int largestGridSize;
}

[InternalBufferCapacity(5)]
public struct LevelComponent : IBufferElementData, IDisposable
{
    [ReadOnly] public NativeBitArray dirtyChunks;
    [ReadOnly] public NativeArray<LevelGridTile> renderGrid;
    /// <summary>
    /// Reverse of World offsets
    /// 1, dim, -1, -dim
    /// </summary>
    [ReadOnly] public NativeArray<int> neighbours;

    [ReadOnly] public int offsetsScaled;
    [ReadOnly] public int offsets;
    [ReadOnly] public int dimensionsScaled;
    [ReadOnly] public int dimensions;

    /// <summary>
    /// Numbers of chunks (used for render Cull)
    /// </summary>
    [ReadOnly] public int chunkCount;

    public void Dispose()
    {
        if (neighbours.IsCreated)
            neighbours.Dispose();
        if (renderGrid.IsCreated)
            renderGrid.Dispose();
        if (dirtyChunks.IsCreated)
            dirtyChunks.Dispose();
    }
}

[InternalBufferCapacity(5)]
public struct OverlayComponent : IBufferElementData
{
    public NativeBitArray markedTilesGrid;
    public NativeArray<OverlayData> overlayGrid;
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

