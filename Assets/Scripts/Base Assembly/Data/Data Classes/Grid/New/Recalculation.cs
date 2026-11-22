using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.UIElements;


/// <summary>
/// There are 3 types of indexes right now, I know it sucks and I will try eliminate one.
/// Local - the index for the <see cref="LevelData.renderGrid"/>> data, affected by offset
/// World - the index for level specific world positions, (without offset)
/// Global - the index for Paths, identifies any one tile (even nonexisting ones)
/// </summary>
public static class Recalculation
{
    public static int Distance(int2 currentPosition, int2 destPosition)
    {
        int2 dist = math.abs(currentPosition - destPosition);
        return dist.x + dist.y;
    }

    public static int Distance(int3 currentPosition, int3 destPosition)
    {
        int3 dist = math.abs(currentPosition - destPosition);

        return math.select(dist.x + dist.z, 0, dist.y > 0);//math.select(dist.x + dist.z, 0, dist.y > 0) dist.x + /*(dist.y) +*/ dist.z;
    }

    public static int GlobalIndexFromLocalIndex(int localIndex, int offsetScaled, int dimensionsScaled, int largestDim, int levelIndex)
    {
        int x = localIndex % dimensionsScaled + offsetScaled;
        int y = localIndex / dimensionsScaled + offsetScaled;

        int original = x + (y * largestDim) + (levelIndex * (largestDim * largestDim));
        return original;
    }

    public static int WorldIndexFromLocalIndex(int localIndex, int offsetScaled, int dimensionsScaled)
    {
        int x = localIndex % dimensionsScaled + offsetScaled;
        int y = localIndex / dimensionsScaled + offsetScaled;


        int original = x + y * (dimensionsScaled + (2 * offsetScaled));
        return original;
    }

    public static int LocalIndexFromWorldIndex(int world, int offsetScaled, int dimensionsScaled)
    {
        int x = (world % (dimensionsScaled + (2 * offsetScaled))) - offsetScaled;
        int y = (world / (dimensionsScaled + (2 * offsetScaled))) - offsetScaled;

        return y * dimensionsScaled + x;
    }

    public static int LocalIndexFromWorldPos(int2 world, int offsetScaled, int dimensionsScaled)
    {
        world -= offsetScaled;

        return world.y * dimensionsScaled + world.x;
    }

    public static int2 WorldPosFromLocalIndex(int localIndex, int offsetScaled, int dimensionsScaled)
    {
        int x = localIndex % dimensionsScaled + offsetScaled;
        int y = localIndex / dimensionsScaled + offsetScaled;

        return new(x, y);
    }

    public static int GlobalIndexFromWorldPos(int2 world, int largestLevel)
    {
        return world.y * largestLevel + world.x;
    }

    public static int LocalIndexFromGlobalIndex(int global, in NativeArray<LevelComponent> levelArray, int levelGrid, out int levelIndex)
    {
        levelIndex = global / levelGrid;

        return LocalIndexFromWorldIndex(
            global % levelGrid, 
            levelArray[levelIndex].offsetsScaled,
            levelArray[levelIndex].dimensionsScaled);
    }

    public static uint BuildingPosToFixedBuildingPos(uint pos, int offsetsScaled)
    {

        uint offset = (uint)offsetsScaled;
        uint x = (pos & 0x3FF) + offset;
        uint z = ((pos >> 10) & 0x3FF) + offset;

        return (uint)((pos & ~0xFFFFF) | (x & 0x3FF) | ((z & 0x3FF) << 10));
    }

    /// <summary>
    /// See: <see cref="BuildingRenderData.encodedLocalPosition"/>
    /// </summary>
    /// <param name="posToPlace"></param>
    /// <param name="level"></param>
    /// <param name="rot"></param>
    /// <returns></returns>
    public static uint BuildingPosFromPos(int posToPlace, int level, int rot) => (uint)(
        (posToPlace & 0x3FF) | 
        ((posToPlace & 0x3FF) << 10) | 
        ((level & 0x7) << 20) |
        ((rot & 0x3) << 22));
}

