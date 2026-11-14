using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Paths;
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
                    AddBuildingToArray(item);
                }
            }
        }

        WorldComponent worldComponent;
        NativeArray<LevelComponent> levelArray;
        NativeArray<OverlayComponent> overlays;

        public BuildingRenderer SetupRenderer(
            Material buildingMaterial, 
            LevelRenderer levelRenderer, 
            ref WorldComponent worldComponent, 
            in NativeArray<LevelComponent> levelArray,
            ref NativeArray<OverlayComponent> overlays)
        {
            this.levelRenderer = levelRenderer;
            this.worldComponent = worldComponent;
            this.levelArray = levelArray;
            this.overlays = overlays;


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
                BuildingNew build = buildings[i];

                BuildingChanges.MarkTiles(
                    wrappers[(int)buildings[i].renderData.meshIndex].blueprint,
                    build.renderData,
                    i,
                    ref worldComponent,
                    in levelArray,
                    ref overlays);
            }
            levelRenderer.UpdateOverlayTexture();

            return buildingRenderer;
        }

        #region Array management
        /// <summary>
        /// Doesn't do any registers, only adds the data to the right index in the <see cref="buildings"/>.
        /// Nothing else is afffected.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        int AddBuildingToArray(BuildingNew data)
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
            return index;
        }
        void RemoveBuildingFromArray(int index)
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
            buildings[index] = null;
        }
        #endregion

        public void RemoveBuilding(int index)
        {
            BuildingNew building = buildings[index];
            RemoveBuildingFromArray(index);
            BuildingWrapper wrapper = SceneRefs.ObjectFactory.buildPrefabs.GetBuilding(building.categoryIndex, building.wrapperIndex);
            BuildingChanges.UnMarkTiles(
                wrapper.blueprint, 
                building.renderData, 
                ref worldComponent, 
                in levelArray,
                ref overlays);
            
            levelRenderer.UpdateOverlayTexture();
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

        public void PlaceBuilding(BuildingWrapper blueprint, BuildingNew buildingData)
        {
            int arrayIndex = AddBuildingToArray(buildingData);

            BuildingChanges.MarkTiles(
                blueprint.blueprint, 
                buildingData.renderData, 
                arrayIndex, 
                ref worldComponent, 
                in levelArray,
                ref overlays);

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
