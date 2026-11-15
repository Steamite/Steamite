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
    /// <param name="path"></param>
    /// <param name="regions"></param>
    /// <param name="availableRegions"></param>
    [BurstCompile]
    public static void InitFill(
        int levelIndex,
        ref WorldComponent worldComponent,
        NativeArray<LevelComponent> levels)
    {
        var level = levels[levelIndex];
        int scaledDim = level.dimensionsScaled;

        int gridSizeScaled = scaledDim * scaledDim;

        // Vertical offsets
        NativeArray<int> connectOffsets;
        if (levelIndex > 0)
        {
            connectOffsets = new NativeArray<int>(3, Allocator.Persistent);
            connectOffsets[2] = -gridSizeScaled;
        }
        else
            connectOffsets = new NativeArray<int>(2, Allocator.Persistent);
        connectOffsets[0] = -1;
        connectOffsets[1] = -scaledDim;



        var pathRegions = worldComponent.pathRegions;
        var regions = worldComponent.regions;

        var path = worldComponent.path;

        int len = scaledDim * scaledDim;
        int x = 0;

        int off = level.offsetsScaled;
        int iJump = level.offsetsScaled * 2;
        int worldIndex = (off * level.dimensionsScaled) + (off * off) * 2 + off; 
        //(level.offsetsScaled*level.offsetsScaled) + (level.offsetsScaled * level.offsetsScaled) + level.offsetsScaled;

        for (int i = 0; i < len; i++)
        {
            if (pathRegions[worldIndex] == 0)
            {
                var t = levels[levelIndex].renderGrid[i].TType;
                bool accessible = t == TileType.Road || t == TileType.PassableBuilding || t == TileType.Elevator;

                if (accessible) 
                {
                    Join(worldIndex, path, pathRegions, worldComponent.offsets, GetNewRegionId(regions));
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
        if(connectOffsets.IsCreated)
            connectOffsets.Dispose();
    }

    static byte GetNewRegionId(NativeBitArray array)
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

    static void Join(
        int startPosition, 
        NativeArray<short> path, 
        NativeArray<byte> regions, 
        NativeArray<int> offsets,
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
                int index = worldIndex - offsets[i];

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
