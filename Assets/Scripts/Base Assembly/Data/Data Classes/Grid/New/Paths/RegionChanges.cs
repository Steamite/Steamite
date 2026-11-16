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

        int size = worldComponent.LargestLevelDimensionScaled * worldComponent.LargestLevelDimensionScaled;

        // Vertical offsets
        NativeArray<int> conectedOffsets = new (6, Allocator.Persistent);
        conectedOffsets[0] = -worldComponent.offsets[0];
        conectedOffsets[1] = -worldComponent.offsets[1];
        conectedOffsets[2] = -worldComponent.offsets[2];
        conectedOffsets[3] = -worldComponent.offsets[3];
        
        if(levelIndex > 0)
        {
            conectedOffsets[5] = -size;
        }
        if (levelIndex < levels.Length -1)
        {
            conectedOffsets[4] = size;
        }



        var pathRegions = worldComponent.pathRegions;
        var regions = worldComponent.regions;

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
                    Join(worldIndex, path, pathRegions, conectedOffsets, GetNewRegionId(regions));
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
                if (offsets[i] == 0)
                    continue;

                int index = worldIndex + offsets[i];

                if (((path[index] >> i) & 0x1) == 1)
                {
                    if(i == 4)
                    {

                    }
                        
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
