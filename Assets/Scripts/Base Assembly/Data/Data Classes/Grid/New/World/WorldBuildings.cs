using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Android.Gradle.Manifest;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World
{
    public class WorldBuildings : IDisposable
    {
        public BuildingDataNew[] buildings;

        int lastIndex = 0;
        int firstRemovedSpace = int.MaxValue;

        BuildingRender render;

        public void LoadState(BuildingDataNew[] newSave)
        {
            buildings = new BuildingDataNew[math.max(50, newSave.Length * 2)];

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

        public BuildingRender SetupRenderer(Mesh[] meshes, Material buildingMaterial, Material mat2)
        {
            uint[] counts = new uint[meshes.Length];
            uint2[] instanceData = new uint2[lastIndex];

            for (int i = 0; i < lastIndex; i++)
            {
                if (buildings[i] == null)
                    continue;
                counts[buildings[i].meshIndex]++;
                instanceData[i] = new(buildings[i].position, buildings[i].meshIndex);
            }

            render = new(buildingMaterial, mat2);
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

        int AddBuilding(BuildingDataNew data)
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
                    BuildingDataNew[] buildingDataNew = new BuildingDataNew[buildings.Length * 2];
                    buildings.CopyTo(buildingDataNew, 0);
                    buildings = buildingDataNew;
                }
                buildings[lastIndex] = data;
                lastIndex++;
            }

            render?.CreateBuilding(new(data.position, data.meshIndex));

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
    }
}
