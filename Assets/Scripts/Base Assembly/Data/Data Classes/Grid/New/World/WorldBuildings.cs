using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{
    [Serializable]
    public class WorldBuildings : IDisposable
    {
        public BuildingNew[] buildings;

        int lastIndex = 0;
        int firstRemovedSpace = int.MaxValue;

        BuildingRenderer render;

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

        public BuildingRenderer SetupRenderer(Mesh[] meshes, Material buildingMaterial)
        {
            uint[] counts = new uint[meshes.Length];
            BuildingRenderData[] instanceData = new BuildingRenderData[lastIndex];

            for (int i = 0; i < lastIndex; i++)
            {
                if (buildings[i] == null)
                    continue;
                counts[buildings[i].renderData.meshIndex]++;
                instanceData[i] = buildings[i].renderData; //new(, buildings[i].meshIndex);
            }
            // DEBUG

            render = new(buildingMaterial);
            int[] indexes = render.UploadBuildingData(meshes, counts, instanceData);

            int x = 0;
            for (int i = 0; i < lastIndex; i++)
            {
                if (buildings[i] == null)
                {
                    x--;
                    continue;
                }
                buildings[i].renderIndex = indexes[i-x];
            }

            return render;
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

        void RemoveBuilding(int index)
        {
            buildings[index] = null;
            if (index < firstRemovedSpace)
                firstRemovedSpace = index;
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
            render?.Dispose();
        }

        public void PlaceBuilding(int2 worldPos, BuildingNew newBuilding)
        {
            AddBuilding(newBuilding);
            SceneRefs.World.CurrentLevel.SetTileByWorldPos(worldPos, LevelGridTile.Elevator(0));
        }
    }
}
