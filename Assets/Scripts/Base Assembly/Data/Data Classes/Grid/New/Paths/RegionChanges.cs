using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Humans;
using Grid.New;
using System.Drawing;
using Unity.Burst;
using Unity.Collections;
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


        var conectedOffsets = CreateOffsetArray(worldComponent.offsets, size, levelIndex);


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
                    Join(worldIndex, path, ref pathRegions, conectedOffsets, GetNewRegionId(regions));
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
        if(conectedOffsets.IsCreated)
            conectedOffsets.Dispose();
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

    static NativeArray<int> CreateOffsetArray(in NativeArray<int> offsets, int gridSize, int levelIndex)
    {
        // Vertical offsets
        NativeArray<int> conectedOffsets = new(6, Allocator.Persistent);
        conectedOffsets[0] = -offsets[0];
        conectedOffsets[1] = -offsets[1];
        conectedOffsets[2] = -offsets[2];
        conectedOffsets[3] = -offsets[3];

        if (levelIndex > 0)
        {
            conectedOffsets[5] = -gridSize;
        }
        if (levelIndex < 4)
        {
            conectedOffsets[4] = gridSize;
        }
        return conectedOffsets;
    }

    public static void SetPath(int openPosition, in WorldComponent world, int levelIndex)
    {
        var offsets = CreateOffsetArray(world.offsets, world.largestGridSize, levelIndex);
        var path = world.path;

        var regions = world.regions;
        var availableRegions = world.availableRegions;
        byte regionId = JoinRegionArray(openPosition, ref offsets, ref path, ref regions, availableRegions);


        Join(openPosition, path, ref regions, offsets, regionId);
    }
    static byte JoinRegionArray(int openPosition, ref NativeArray<int> offsets, ref NativeArray<short> path, ref NativeArray<byte> regions, NativeBitArray availableRegions)
    {
        byte regionId = byte.MaxValue;

        for (int i = 0; i < offsets.Length; i++)
        {
            if (offsets[i] == 0)
                continue;

            int pos = openPosition + offsets[i];
            if (((path[pos] >> i) & 0x1) == 1)
            {
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

    static void Join(
        int startPosition, 
        in NativeArray<short> path, 
        ref NativeArray<byte> regions,
        in NativeArray<int> offsets,
        byte regionId)
    {
        NativeList<int> queue = new NativeList<int>(5, Allocator.TempJob)
        {
            startPosition
        };

        regions[startPosition] = regionId;
        while(queue.Length > 0)
        {
            int worldIndex = queue[0];
            
            queue.RemoveAt(0);
            for (int i = 0; i < offsets.Length; i++)
            {
                if (offsets[i] == 0)
                    continue;

                int index = worldIndex + offsets[i];

                if (((path[index] >> i) & 0x1) == 1)
                {                        
                    if(regions[index] != regionId)
                    {
                        regions[index] = regionId;
                        queue.Add(index);
                    }
                }
            }
        }

        queue.Dispose();
    }




}
