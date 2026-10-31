using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{
    [Serializable]
    public class WorldBuildings : IDisposable
    {
        public BuildingNew[] buildings;

        int lastIndex = 0;
        int firstRemovedSpace = int.MaxValue;

        BuildingRenderer buildingRenderer;
        LevelRenderer levelRenderer;

        public void LoadState(BuildingNew[] newSave)
        {

            List<BuildingWrapper> wrappers = SceneRefs.ObjectFactory.buildPrefabs.GetAllObjects();

            buildings = new BuildingNew[math.max(50, newSave.Length * 2)];

            foreach (var item in newSave)
            {
                if (item == null)
                {
                    if (firstRemovedSpace == int.MaxValue)
                        firstRemovedSpace = lastIndex;
                    lastIndex++;
                }
                else
                {
                    /*int2 pos = new((int)item.renderData.position & 0x9, (int)(item.renderData.position >> 10) & 0x9);
                    PlaceBuilding(pos, wrappers[item.wrapperIndex], item);*/
                    AddBuilding(item);
                }
            }
        }

        public BuildingRenderer SetupRenderer(Material buildingMaterial, LevelRenderer levelRenderer)
        {
            this.levelRenderer = levelRenderer;



            List<BuildingWrapper> wrappers = SceneRefs.ObjectFactory.buildPrefabs.GetAllObjects();

            BuildingGroupData[] buildingGroups = new BuildingGroupData[wrappers.Count];

            for (int i = 0; i < buildingGroups.Length; i++)
            {
                buildingGroups[i].mesh = wrappers[i].blueprint.mesh;
                //buildingGroups[i].meshSizeHalved = wrappers[i].blueprint.renderSize;
            }

            BuildingRenderData[] instanceData = new BuildingRenderData[lastIndex];
            for (int i = 0; i < lastIndex; i++)
            {
                if (buildings[i] == null)
                    continue;
                buildingGroups[buildings[i].renderData.meshIndex].count++;
                instanceData[i] = buildings[i].renderData; 
            }

            buildingRenderer = new(buildingMaterial);
            int[] indexes = buildingRenderer.UploadBuildingData(buildingGroups, instanceData);

            for (int i = 0; i < lastIndex; i++)
            {
                buildings[i].renderIndex = indexes[i];
            }

            return buildingRenderer;
        }

        int AddBuilding(BuildingNew data)
        {
            int index;
            if(firstRemovedSpace <= lastIndex)
            {
                index = firstRemovedSpace;
                buildings[firstRemovedSpace] = data;
                FindNextRemoved(firstRemovedSpace);
            }
            else
            {
                index = lastIndex;
                if(lastIndex == buildings.Length)
                {
                    BuildingNew[] buildingDataNew = new BuildingNew[buildings.Length * 2];
                    buildings.CopyTo(buildingDataNew, 0);
                    buildings = buildingDataNew;
                }
                buildings[lastIndex] = data;
                lastIndex++;
            }

            //render?.CreateBuilding(data.renderData);

            return index;
        }

        public void RemoveBuilding(int index)
        {
            if (index < firstRemovedSpace)
                firstRemovedSpace = index;
            int count = buildingRenderer.RemoveBuilding(buildings[index].renderIndex);

            int startIndex = buildings[index].renderIndex;
            int max = startIndex + count;

            for (int i = 0; i < buildings.Length; i++)
            {
                if (buildings[i]?.renderIndex >= startIndex && buildings[i].renderIndex < max)
                    buildings[i].renderIndex--;
            }

            BuildingWrapper wrapper = SceneRefs.ObjectFactory.buildPrefabs.GetBuilding(buildings[index].categoryIndex, buildings[index].wrapperIndex);
            
            int2 size = wrapper.blueprint.size;
            size.y -= 1;
            uint rot = (buildings[index].renderData.position >> 23) & 0x3;

            int2 worldPos = new int2(
                (int)buildings[index].renderData.position & 0x3FF, 
                (int)(buildings[index].renderData.position >> 10) & 0x3FF);
            worldPos += SceneRefs.World.CurrentLevel.levelOffsetScaled;

            int2 worldTemp;
            int tileIndex = 0;
            GridLevel level = SceneRefs.World.CurrentLevel;

            for (int i = -1; i < size.y; i++)
            {
                for (int j = 0; j < size.x; j++)
                {
                    int2 rotPos = new(
                        j * cosTable[rot] - i * sinTable[rot],
                        j * sinTable[rot] + i * cosTable[rot]);

                    worldTemp = worldPos + rotPos;

                    switch (wrapper.blueprint.itemArray[tileIndex])
                    {
                        case GridItemType.Road:
                        case GridItemType.PassableRoad:

                            level.SetTileByWorldPos(
                                worldTemp,
                                LevelGridTile.Road);
                            break;
                        case GridItemType.Entrance:
                            // mark in texture
                            level.SetEntrance(worldTemp, levelRenderer, false);
                            break;
                    }
                    tileIndex++;
                }
            }
            levelRenderer.UpdateOverlayTexture();

/*

            int x = size.x * cosTable[rot] - size.y * sinTable[rot];
            int y = size.x * sinTable[rot] + size.y * cosTable[rot];
            size.x = x;
            size.y = y;



            if (size.x < 0)
            {
                world.x += size.x + 1;
                size.x = -size.x;
            }
            if (size.y < 0)
            {
                world.y += size.y + 1;
                size.y = -size.y;
            }
            int originalX = world.x;

            for (int j = 0; j < size.y; j++)
            {
                for (int i = 0; i < size.x; i++)
                {
                    SceneRefs.World.CurrentLevel.SetTileByWorldPos(
                        world,
                        LevelGridTile.Road);
                    world.x++;
                }
                world.x = originalX;
                world.y++;
            }
            buildings[index] = null;*/
        }

        void FindNextRemoved(int startIndex)
        {
            for (int i = startIndex; i < lastIndex; i++)
            {
                if (buildings[i] == null)
                {
                    firstRemovedSpace = i;
                    return;
                }
            }
            firstRemovedSpace = int.MaxValue;
        }

        public void Dispose()
        {
            buildingRenderer?.Dispose();
        }

        readonly static int[] cosTable = { 1, 0, -1, 0 };
        readonly static int[] sinTable = { 0, 1, 0, -1 };
        public void PlaceBuilding(int2 worldPos, BuildingWrapper wrapper, BuildingNew buildingData)
        {
            int arrayIndex = AddBuilding(buildingData);
            int2 size = wrapper.blueprint.size;
            size.y -= 1;
            uint rot = (buildingData.renderData.position >> 23) & 0x3;


            int2 worldTemp;
            int index = 0;
            GridLevel level = SceneRefs.World.CurrentLevel;

            for (int i = -1; i < size.y; i++)
            {
                for (int j = 0; j < size.x; j++)
                {
                    int2 rotPos = new(
                        j * cosTable[rot] - i * sinTable[rot],
                        j * sinTable[rot] + i * cosTable[rot]);

                    worldTemp = worldPos + rotPos;

                    switch (wrapper.blueprint.itemArray[index])
                    {
                        case GridItemType.Road:

                            level.SetTileByWorldPos(
                                worldTemp,
                                LevelGridTile.Building(arrayIndex));
                            break;
                        /*case GridItemType.PassableRoad:
                            level.SetTileByWorldPos(
                                worldTemp,
                                LevelGridTile.PassableRoad);
                            break;*/
                        case GridItemType.Entrance:
                            // mark in texture
                            level.SetEntrance(worldTemp, levelRenderer, true);
                            break;
                    }
                    index++;
                }
            }
            levelRenderer.UpdateOverlayTexture();
        }

        public void UpdateRenderIndexes(int startIndex, int increase)
        {
            for (int i = 0; i < lastIndex; i++)
            {
                if (buildings[i]?.renderIndex >= startIndex)
                    buildings[i].renderIndex += increase;
            }
        }
    }
}
