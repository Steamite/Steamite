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

        public void LoadState(BuildingNew[] newSave)
        {
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
                    AddBuilding(item);
            }
        }

        public BuildingRenderer SetupRenderer(Material buildingMaterial)
        {
            List<BuildingWrapper> wrappers = SceneRefs.ObjectFactory.buildPrefabs.GetAllObjects();

            BuildingGroupData[] buildingGroups = new BuildingGroupData[wrappers.Count];

            for (int i = 0; i < buildingGroups.Length; i++)
            {
                buildingGroups[i].mesh = wrappers[i].blueprint.mesh;
                buildingGroups[i].meshSizeHalved = wrappers[i].blueprint.renderSize;
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

            int x = size.x * cosTable[rot] - size.y * sinTable[rot];
            int y = size.x * sinTable[rot] + size.y * cosTable[rot];
            size.x = x;
            size.y = y;

            int2 world = new int2(
                (int)buildings[index].renderData.position & 0x3FF, 
                (int)(buildings[index].renderData.position >> 10) & 0x3FF);
            world += SceneRefs.World.CurrentLevel.levelOffsetScaled;


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
            buildings[index] = null;
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
        public void PlaceBuilding(int2 worldPos, BuildingWrapper wrapper, BuildingNew buildingData, uint buildingPos)
        {
            buildingData.renderData.position = buildingPos;

            int arrayIndex = AddBuilding(buildingData);
            int2 size = wrapper.blueprint.size;
            size.y -= 1;
            uint rot = (buildingPos >> 23) & 0x3;

            int x = size.x * cosTable[rot] - size.y * sinTable[rot];
            int y = size.x * sinTable[rot] + size.y * cosTable[rot];
            size.x = x;
            size.y = y;

            int2 world = worldPos;


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
                        LevelGridTile.Building(arrayIndex));
                    world.x++;
                }
                world.x = originalX;
                world.y++;
            }
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
