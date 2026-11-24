using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.Paths;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New.Modes;
using Assets.Scripts.Systems.Buildings;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Buildings
{

    public struct InitialRenderData
    {
        public BuildingGroupData[] buildingGroups;
        public NativeArray<BuildingRenderData> buildings;
    }

    [Serializable]
    public class WorldBuildings : MonoBehaviour
    {
        /// <summary>
        /// Used for bridging selection (tiles point to this array), rendering, and storing position.
        /// </summary>
        [NonSerialized] public NativeArray<BuildingRenderData> buildings;
        [NonSerialized] public NativeArray<Entity> buildingEntities;
        
        [SerializeField] BuildingData buildingPrefabs;
        int lastIndex = 0;
        int firstRemovedSpace = int.MaxValue;

        [NonSerialized] public BuildingRenderManager buildingRenderer;
        LevelRenderer levelRenderer;


        public void LoadState(
            List<BuildingSaveData> buildingsToLoad, 
            WorldComponent worldComponent, 
            NativeArray<LevelComponent> levelArray, 
            NativeArray<OverlayComponent> overlays,
            LevelRenderer levelRenderer)
        {
            this.worldComponent = worldComponent;
            this.levelArray = levelArray;
            this.overlays = overlays;
            this.levelRenderer = levelRenderer;

            int size = math.max(50, buildingsToLoad.Count * 2);
            buildings = new NativeArray<BuildingRenderData>(size, Allocator.Persistent);
            buildingEntities = new NativeArray<Entity>(size, Allocator.Persistent);

            for (int i = 0; i < buildings.Length; i++)
            {
                if(i >= buildingsToLoad.Count)
                {
                    if (firstRemovedSpace == int.MaxValue)
                        firstRemovedSpace = lastIndex;
                    buildings[lastIndex] = BuildingRenderData.Null;
                    lastIndex++;
                }
                else
                {
                    AddBuildingToArray(buildingsToLoad[i].data, buildingsToLoad[i].inventory);
                    
                }

            }
        }

        WorldComponent worldComponent;
        NativeArray<LevelComponent> levelArray;
        NativeArray<OverlayComponent> overlays;
        

        public InitialRenderData GetInitialRenderData()
        {
            List<BuildingWrapper> wrappers = buildingPrefabs.FlatWrappers;
            BuildingGroupData[] buildingGroups = new BuildingGroupData[wrappers.Count];

            for (int i = 0; i < buildingGroups.Length; i++)
            {
                buildingGroups[i].mesh = wrappers[i].blueprint.mesh;
                buildingGroups[i].count = 0;
                buildingGroups[i].material = wrappers[i].blueprint.material;
            }

            for (int i = 0; i < lastIndex; i++)
            {
                if (!buildings[i].IsValid)
                    continue;

                buildingGroups[buildings[i].meshIndex].count++;
            }

            return new InitialRenderData()
            {
                buildingGroups = buildingGroups,
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
        int AddBuildingToArray(BuildingRenderData data, ICollection<BuildingInventory> resource)
        {
            int index;
            if(firstRemovedSpace <= lastIndex)
            {
                index = firstRemovedSpace;

                buildings[index] = data;
                FindNextRemoved(index);
            }
            else
            {
                index = lastIndex;
                if(lastIndex == buildings.Length)
                {
                    NativeArray<BuildingRenderData> posTemp = new NativeArray<BuildingRenderData>(buildings.Length * 2, Allocator.Persistent);
                    posTemp.CopyFrom(buildings);
                    buildings.Dispose();
                    buildings = posTemp;

                    NativeArray<Entity> entitiesTemp = new NativeArray<Entity>(buildingEntities.Length * 2, Allocator.Persistent);

                    entitiesTemp.CopyFrom(buildingEntities);
                    buildingEntities.Dispose();
                    buildingEntities = entitiesTemp;
                }

                buildings[index] = data;
                lastIndex++;
            }

            CreateEntity(index, resource);


            return index;
        }

        void CreateEntity(int index, ICollection<BuildingInventory> resource)
        {
            // !!! Create new buildings, need to get resource data here !!!
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var ent = manager.CreateEntity();
            buildingEntities[index] = ent;

            manager.AddComponentData<BuildingComponent>(ent, new()
            {
                listIndex = index,
                state = BuildingState.Constructing
            });
            
            var inventory = manager.AddBuffer<BuildingInventory>(ent);
            foreach (var item in resource)
            {
                inventory.Add(item);
            }
        }

        /// <summary>
        /// Removes the data from render array, and updates renderIndexes.
        /// </summary>
        /// <param name="index"></param>
        void RemoveBuildingFromArray(int index)
        {
            if (index < firstRemovedSpace)
                firstRemovedSpace = index;
            int count = buildingRenderer.RemoveBuilding(buildings[index].renderIndex);

            int startIndex = buildings[index].renderIndex;
            int max = startIndex + count;

            var span = buildings.AsSpan();
            for (int i = 0; i < buildings.Length; i++)
            {
                if (span[i].renderIndex >= startIndex && span[i].renderIndex < max)
                    span[i].renderIndex--;
            }
            buildings[index] = BuildingRenderData.Null;

            // destroy Entity
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            manager.DestroyEntity(buildingEntities[index]);
            buildingEntities[index] = Entity.Null;
        }
        #endregion

        public void RemoveBuilding(int index)
        {
            // GET FROM COMPONENT
            BuildingRenderData building = buildings[index];
            RemoveBuildingFromArray(index);
            BuildingWrapper wrapper = buildingPrefabs.GetBuilding(building.meshIndex);
            BuildingChanges.UnMarkTiles(
                wrapper.blueprint, 
                building, 
                ref worldComponent, 
                in levelArray,
                ref overlays);
            
            levelRenderer.UpdateOverlayTexture();
        }

        void FindNextRemoved(int startIndex)
        {
            for (int i = startIndex; i < lastIndex; i++)
            {
                if (!buildings[i].IsValid)
                {
                    firstRemovedSpace = i;
                    return;
                }
            }
            firstRemovedSpace = int.MaxValue;
        }

        public void PlaceBuilding(BuildingWrapper blueprint, BuildingRenderData buildingData)
        {
            int arrayIndex = AddBuildingToArray(buildingData, blueprint.FillCost());

            BuildingChanges.MarkTiles(
                blueprint.blueprint, 
                buildingData, 
                arrayIndex, 
                ref worldComponent, 
                in levelArray,
                ref overlays);

            levelRenderer.UpdateOverlayTexture();
        }

        public void UpdateRenderIndexes(int startIndex, short increase)
        {
            for (int i = 0; i < lastIndex; i++)
            {
                if (buildings[i].renderIndex >= startIndex)
                {
                    var b = buildings[i];
                    b.renderIndex += increase;
                    buildings[i] = b;
                }
            }
        }

        private void OnDestroy()
        {
            buildingRenderer.Dispose();
            
            if(buildings.IsCreated) 
                buildings.Dispose();

            if (buildingEntities.IsCreated)
                buildingEntities.Dispose();
        }
    }
}
