using System;
using System.Collections.Generic;
using System.Text;

public static class Recalculation
{
    public static int WorldIndexFromLocal(int localIndex, int offsetScaled, int dimensionsScaled)
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
}

