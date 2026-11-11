using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;


/// <summary>
/// There are 3 types of indexes right now, I know it sucks and I will try eliminate one.
/// Local - the index for the <see cref="LevelData.renderGrid"/>> data, affected by offset
/// World - the index for level specific world positions, (without offset)
/// Global - the index for Paths, identifies any one tile (even nonexisting ones)
/// </summary>
public static class Recalculation
{
    public static int GlobalIndexFromLocalIndex(int localIndex, int offsetScaled, int dimensionsScaled, int levelIndex)
    {
        int x = localIndex % dimensionsScaled + offsetScaled;
        int y = localIndex / dimensionsScaled + offsetScaled;


        int largestDim = dimensionsScaled + (2 * offsetScaled);
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
}

