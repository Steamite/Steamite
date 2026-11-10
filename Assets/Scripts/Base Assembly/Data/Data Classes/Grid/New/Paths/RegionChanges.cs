using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

public static class RegionChanges
{
    /// <summary>
    /// Uses the computed path nodes to flood mark regions
    /// </summary>
    /// <param name="position"></param>
    /// <param name="path"></param>
    /// <param name="regions"></param>
    /// <param name="availableRegions"></param>
    [BurstCompile]
    public static void InitFill(
        int levelIndex,
        int scaledDim,
        NativeArray<short> path, 
        NativeArray<byte> regions, 
        NativeBitArray availableRegions)
    {
        int gridSizeScaled = scaledDim * scaledDim;
        int maxLen = (gridSizeScaled * levelIndex) - (scaledDim);


        NativeArray<int> connectOffsets;
        if (levelIndex > 0)
        {
            connectOffsets = new NativeArray<int>(3, Allocator.TempJob);
            connectOffsets[2] = -gridSizeScaled;
        }
        else
            connectOffsets = new NativeArray<int>(2, Allocator.TempJob);
        connectOffsets[0] = -1;
        connectOffsets[1] = -scaledDim;

        NativeArray<int> neighbourOffsets = new NativeArray<int>(4, Allocator.TempJob);
        neighbourOffsets[0] = -1;
        neighbourOffsets[1] = -scaledDim;
        neighbourOffsets[2] = 1;
        neighbourOffsets[3] = scaledDim;


        for (int i = (gridSizeScaled*levelIndex) + scaledDim; i < maxLen; i++)
        {
            if (regions[i] > 0)
                continue;

            bool accessible = false;
            for (int j = 0; j < neighbourOffsets.Length; j++)
            {
                int neighbour = path[i + neighbourOffsets[j]];
                if (((neighbour >> j) & 0x1) == 1)
                {
                    accessible = true;
                    break;
                }
            }

            if (!accessible)
                continue;

            //Join(i, path, regions, GetNewRegionId(availableRegions));
            // JOIN
        }

        connectOffsets.Dispose();
        neighbourOffsets.Dispose();
    }

    static byte GetNewRegionId(NativeBitArray array)
    {
        for (byte i = 0; i < array.Length; i++)
        {
            if (array.GetBits(i) == 0)
                return i;
        }
        return 0;
    }

    static void Join(
        int startPosition, 
        NativeArray<short> path, 
        NativeArray<byte> regions, 
        NativeBitArray takenArrays, 
        NativeArray<int> offsets,
        int regionId)
    {
        NativeList<int> queue = new NativeList<int>(5, Allocator.TempJob)
        {
            startPosition
        };

        while(queue.Length > 0)
        {
            int worldIndex = queue[0];
            
            queue.RemoveAt(0);
            for (int i = 0; i < offsets.Length; i++)
            {
                int index = worldIndex + offsets[i];

                if (((path[index] >> i) & 0x1) == 1)
                {
                   // regions[index] = regionId;
                }
            }
        }

        queue.Dispose();
    }
}
