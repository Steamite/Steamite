using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Paths;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;

public static class BuildingChanges
{
    readonly static int[] cosTable = { 1, 0, -1, 0 };
    readonly static int[] sinTable = { 0, 1, 0, -1 };

    public static void MarkTiles(
        BuildingGrid blueprint, 
        BuildingRenderData renderData,
        int arrayIndex,
        ref WorldComponent worldComponent,
        in NativeArray<LevelComponent> levelArray,
        ref NativeArray<OverlayComponent> overlays)
    {
        uint encodedPosition = renderData.encodedLocalPosition;
        int levelIndex = (int)(encodedPosition >> 20) & 0x3;

        int2 worldPos = 
            new int2(
                (int)encodedPosition & 0x3FF, 
                (int)(encodedPosition >> 10) & 0x3FF) 
            + levelArray[levelIndex].offsetsScaled;


        int2 size = blueprint.size;
        size.y -= 1;
        uint rot = (renderData.encodedLocalPosition >> 23) & 0x3;


        int2 worldTemp;
        int index = 0;

        LevelComponent level = levelArray[levelIndex];
        OverlayComponent overlay = overlays[levelIndex];

        for (int i = -1; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                int2 rotPos = new(
                    j * cosTable[rot] - i * sinTable[rot],
                    j * sinTable[rot] + i * cosTable[rot]);

                worldTemp = worldPos + rotPos;
                int locaIndex = Recalculation.LocalIndexFromWorldPos(worldTemp, level.offsetsScaled, level.dimensionsScaled);

                LevelGridTile tile = default;
                switch (blueprint.itemArray[index])
                {
                    case GridItemType.Road:
                        tile = LevelGridTile.Building(arrayIndex);
                        break;
                    case GridItemType.PassableRoad:
                        tile = LevelGridTile.PassableRoad(arrayIndex);
                        break;
                    case GridItemType.Elevator:
                        tile = LevelGridTile.Elevator(arrayIndex);
                        break;
                    case GridItemType.Entrance:
                        index++;
                        OverlayChanges.SetEntrance(worldTemp, true, ref level, ref overlay);
                        continue;
                }

                WorldChanges.SetTile(
                    locaIndex,
                    levelIndex,
                    in tile,
                    ref worldComponent,
                    in levelArray);

                index++;
            }
        }
    }

    public static void UnMarkTiles(
        BuildingGrid blueprint, 
        in BuildingRenderData buildingRenderData, 
        ref WorldComponent worldComponent, 
        in NativeArray<LevelComponent> levelArray,
        ref NativeArray<OverlayComponent> overlays)
    {
        int2 size = blueprint.size;
        size.y -= 1;
        uint rot = (buildingRenderData.encodedLocalPosition >> 23) & 0x3;

        int2 worldPos = new int2(
            (int)buildingRenderData.encodedLocalPosition & 0x3FF,
            (int)(buildingRenderData.encodedLocalPosition >> 10) & 0x3FF);
        int levelIndex = (int)(buildingRenderData.encodedLocalPosition >> 20) & 0x3;
        

        int2 worldTemp;
        int tileIndex = 0;
        LevelComponent level = levelArray[levelIndex];
        OverlayComponent overlay = overlays[levelIndex];

        worldPos += level.offsetsScaled;

        for (int i = -1; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                int2 rotPos = new(
                    j * cosTable[rot] - i * sinTable[rot],
                    j * sinTable[rot] + i * cosTable[rot]);

                worldTemp = worldPos + rotPos;

                int local = Recalculation.LocalIndexFromWorldPos(worldTemp, level.offsetsScaled, level.dimensionsScaled);

                switch (blueprint.itemArray[tileIndex])
                {
                    case GridItemType.Road:
                    case GridItemType.PassableRoad:
                    case GridItemType.Elevator:
                        WorldChanges.SetTile(local, levelIndex, LevelGridTile.Road, ref worldComponent, in levelArray);
                        break;
                    case GridItemType.Entrance:
                        OverlayChanges.SetEntrance(worldTemp, false, ref level, ref overlay);
                        break;
                }
                tileIndex++;
            }
        }
    }
}
