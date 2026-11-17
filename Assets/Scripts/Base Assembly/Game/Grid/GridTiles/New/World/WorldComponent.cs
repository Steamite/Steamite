using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Grid.New;
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct WorldComponent : IComponentData, IDisposable
{
    // Region
    [ReadOnly] public NativeArray<byte> regions;
    [ReadOnly] public NativeBitArray availableRegions;


    /// <summary>
    /// 0 - Left <br/>
    /// 1 - Backwards <br/>
    /// 2 - Right <br/>
    /// 3 - Forward <br/>
    /// 4 - Down <br/>
    /// 5 - Up <br/>
    /// </summary>
    [ReadOnly] public NativeArray<short> path;

    /// <summary>
    /// Offsets is the opposite of the path order, to check if an adjacent tile connects to this one.
    /// For going in the real direction use the negative version of this.
    /// -1, -dim, 1, dim
    /// </summary>
    [ReadOnly] public NativeArray<int> offsets;


    [ReadOnly] public int LargestLevelDimensionScaled;
    [ReadOnly] public int largestGridSize;

    public void Dispose()
    {
        if (offsets.IsCreated)
            offsets.Dispose();
        if (path.IsCreated)
            path.Dispose();

        if (availableRegions.IsCreated)
            availableRegions.Dispose();
        if (regions.IsCreated)
            regions.Dispose();
    }
}

[InternalBufferCapacity(5)]
public struct LevelComponent : IBufferElementData, IDisposable
{
    public readonly static int CHUNK_DIMENSIONS = 16;
    [ReadOnly] public NativeBitArray dirtyChunks;
    [ReadOnly] public NativeArray<LevelGridTile> renderGrid;

    [ReadOnly] public int offsetsScaled;
    [ReadOnly] public int offsets;
    [ReadOnly] public int dimensionsScaled;
    [ReadOnly] public int dimensions;

    /// <summary>
    /// Numbers of chunks (used for render Cull)
    /// </summary>
    [ReadOnly] public int chunkCount;

    public bool Unlocked;

    public void Dispose()
    {
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

    public void Dispose()
    {
        if (markedTilesGrid.IsCreated)
            markedTilesGrid.Dispose();

        if (overlayGrid.IsCreated)
            overlayGrid.Dispose();
    }
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

