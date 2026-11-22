using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans;
using Grid.New;
using System.Drawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
public static class RegionChanges
{
    /// <summary>
    /// Uses the computed path nodes to flood mark regions
    /// </summary>
    /// <param name="path"></param>
    /// <param name="regions"></param>
    /// <param name="availableRegions"></param>
    [BurstCompile]
    public static void InitFill(
        int levelIndex,
        ref WorldComponent worldComponent,
        in NativeArray<LevelComponent> levels)
    {
        var level = levels[levelIndex];
        int scaledDim = level.dimensionsScaled;

        int size = worldComponent.largestGridSize;// .LargestLevelDimensionScaled * worldComponent.LargestLevelDimensionScaled;

        var pathRegions = worldComponent.regions;
        var regions = worldComponent.availableRegions;

        var path = worldComponent.path;

        int len = scaledDim * scaledDim;
        int x = 0;

        int off = level.offsetsScaled;
        int iJump = level.offsetsScaled * 2;
        int worldIndex = 
            (levelIndex * size) + 
            (off * level.dimensionsScaled) + (off * off) * 2 + off; 
        //(level.offsetsScaled*level.offsetsScaled) + (level.offsetsScaled * level.offsetsScaled) + level.offsetsScaled;

        for (int i = 0; i < len; i++)
        {
            if (pathRegions[worldIndex] == 0)
            {
                var t = levels[levelIndex].renderGrid[i].TType;
                bool accessible = t == TileType.Road || t == TileType.PassableBuilding || t == TileType.Elevator;

                if (accessible) 
                {
                    Join(worldIndex, path, ref pathRegions, worldComponent.offsets, GetNewRegionId(regions));
                }
            }


            worldIndex++;

            x++;
            if (x == scaledDim)
            {
                x = 0;
                worldIndex += iJump;
            }
        }
    }

    static byte GetNewRegionId(in NativeBitArray array)
    {
        for (byte i = 1; i < array.Length; i++)
        {
            if (array.GetBits(i) == 0)
            {
                array.Set(i, true);
                return i;
            }
        }
        return byte.MaxValue;
    }

    public static void SetPath(int openPosition, in WorldComponent world, int levelIndex)
    {
        var path = world.path;

        var regions = world.regions;
        var availableRegions = world.availableRegions;
        openPosition += levelIndex * world.largestGridSize;
        byte regionId = JoinRegionArray(openPosition, world.offsets, path, ref regions, availableRegions);


        Join(openPosition, path, ref regions, world.offsets, regionId);
    }
    static byte JoinRegionArray(int openPosition, in NativeArray<int> offsets, in NativeArray<short> path, ref NativeArray<byte> regions, NativeBitArray availableRegions)
    {
        byte regionId = byte.MaxValue;

        for (int i = 0; i < 4; i++)
        {
            if (((path[openPosition] >> i) & 0x1) == 1)
            {
                int pos = openPosition + offsets[i];
                if (regions[pos] < regionId && regions[pos] > 0)
                {
                    if (regionId != byte.MaxValue)
                    {
                        availableRegions.Set(regionId, false);
                    }
                    regionId = regions[pos];
                }
            }
        }

        return regionId;
    }

    public static uint GetAdjacentRegions(int position, in NativeArray<int> offsets, in NativeArray<byte> regions)
    {
        uint regionResult = 0;
        for (int i = 0; i < 4; i++)
        {
            int pos = position + offsets[i];
            regionResult = (regionResult << 8) | regions[pos];
        }
        return regionResult;
    }

    static void Join(
        int startPosition, 
        in NativeArray<short> path, 
        ref NativeArray<byte> regions,
        in NativeArray<int> offsets,
        byte regionId)
    {
        NativeList<int> queue = new NativeList<int>(5, Allocator.Persistent)
        {
            startPosition
        };

        regions[startPosition] = regionId;
        while(queue.Length > 0)
        {
            int worldIndex = queue[0];
            
            queue.RemoveAt(0);
            int p = path[worldIndex];
            for (int i = 0; i < offsets.Length; i++)
            {
                if((p & 0x1) == 1)
                {
                    int index = worldIndex + offsets[i];
                    if (regions[index] != regionId)
                    {
                        regions[index] = regionId;
                        queue.Add(index);
                    }
                }
                p >>= 0x1;
            }
        }

        queue.Dispose();
    }
}
