using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Paths;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New.Modes;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{

    public struct InitialRenderData
    {
        public BuildingGroupData[] buildingGroups;
        public BuildingRenderData[] instanceData;

        public BuildingNew[] buildings;
    }

    [Serializable]
    public class WorldBuildings
    {
        public BuildingNew[] buildings;

        int lastIndex = 0;
        int firstRemovedSpace = int.MaxValue;

        [NonSerialized] public BuildingRenderManager buildingRenderer;
        LevelRenderer levelRenderer;

        public void LoadState(
            BuildingNew[] newSave, 
            WorldComponent worldComponent, 
            NativeArray<LevelComponent> levelArray, 
            NativeArray<OverlayComponent> overlays,
            LevelRenderer levelRenderer)
        {
            this.worldComponent = worldComponent;
            this.levelArray = levelArray;
            this.overlays = overlays;
            this.levelRenderer = levelRenderer;

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
                    AddBuildingToArray(item);
                }
            }
        }

        WorldComponent worldComponent;
        NativeArray<LevelComponent> levelArray;
        NativeArray<OverlayComponent> overlays;
        

        public InitialRenderData GetInitialRenderData()
        {
            List<BuildingWrapper> wrappers = SceneRefs.ObjectFactory.buildPrefabs.GetAllObjects();
            BuildingGroupData[] buildingGroups = new BuildingGroupData[wrappers.Count];

            for (int i = 0; i < buildingGroups.Length; i++)
            {
                buildingGroups[i].mesh = wrappers[i].blueprint.mesh;
                buildingGroups[i].count = 0;


                // TODO
                //buildingGroups[i].material = wrappers[i].blueprint.material;
            }

            BuildingRenderData[] instanceData = new BuildingRenderData[lastIndex];
            for (int i = 0; i < lastIndex; i++)
            {
                if (buildings[i] == null)
                    continue;
                buildingGroups[buildings[i].renderData.meshIndex].count++;
                instanceData[i] = buildings[i].renderData;
            }

            return new InitialRenderData()
            {
                buildingGroups = buildingGroups,
                instanceData = instanceData,
                buildings = buildings
            };
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
