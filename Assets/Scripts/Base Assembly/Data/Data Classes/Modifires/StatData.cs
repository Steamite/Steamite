using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingStats
{
    public enum StatModifiers
    {
        Nothing,
        Cost,
        AssignLimit,
        ProdSpeed,
        InputResource,
        ProductionYield,
        Capacity
    }

    [Serializable]
    public struct StatValue
    {
        /// <summary>Which properies are effected.</summary>
        public StatModifiers mod;
        /// <summary>How much much it effects it.</summary>
        public float modAmmount;
        /// <summary>If the modification is absolute or by a percentage;</summary>
        public bool percent;
    }
    [Serializable]
    public class StatPair
    {
        /// <summary>Which buildings are effected.</summary>
        public int mask;
        public StatValue statValue;
    }

    [Serializable]
    public class Stat : DataObject
    {
        public List<StatPair> pairs;

        public Stat(int _id) : base(_id)
        {
            pairs = new();
        }

        /// <summary>
        /// Masks the buildings by categories.
        /// </summary>
        /// <param name="addStat">If true the stat is added, else it's removed</param>
        public void ApplyStat(bool addStat)
        {
            // create a mask with the affected categories
            int mask = 0;
            List<Building> buildings = MyGrid.Buildings
                .Union(SceneRefs.ObjectFactory.buildPrefabs.Categories
                    .SelectMany(q => q.Objects)
                        .Select(w => w.Building)).ToList();
            int j = Enum.GetNames(typeof(BuildingCategType)).Length;
            foreach (var pair in pairs)
            {
                mask = pair.mask;
                foreach (var _building in buildings)
                {
                    // filter buildings using the mask 
                    int newMask = _building.BuildingCateg & mask;
                    // loop though the mask and do the effect
                    while (newMask != 0)
                    {
                        if ((newMask & 1) == 1)
                        {
                            try
                            {
                                HandleCases(_building, pair.statValue);
                            }
                            catch (Exception e)
                            {
                                if (e is InvalidCastException)
                                {
                                    Debug.LogError(
                                        $"{_building} doesnt implement inteface containing: ${pair.statValue.mod}\n" +
                                        $"{e}");

                                }
                                else
                                    Debug.LogError(e);
                            }
                        }
                        newMask = newMask >> 1;
                    }
                }
            }
        }

        void HandleCases(Building building, StatValue statValue)
        {

            switch (statValue.mod)
            {
                case StatModifiers.Cost:
                    if (building.id == -1)
                    {
                        DoMod(
                            building.Cost,
                            statValue,
                            building);
                    }
                    break;
                case StatModifiers.AssignLimit:
                    DoModWithUpdate(
                        ((IAssign)building).AssignData.AssignLimit,
                        statValue,
                        nameof(IAssign.AssignData),
                        building);
                    break;
                // TODO TEST THIS(no indicator exists right now)
                case StatModifiers.ProdSpeed:
                    DoModWithUpdate(
                        ((IProduction)building).ProdSpeed,
                        statValue,
                        nameof(IProduction.ProdSpeed),
                        building);
                    break;
                case StatModifiers.InputResource:
                    DoModWithUpdate(
                        ((IResourceProduction)building).ResourceCost,
                        statValue,
                        nameof(IResourceProduction.ResourceCost),
                        building);
                    break;
                case StatModifiers.ProductionYield:
                    DoModWithUpdate(
                        ((IResourceProduction)building).ResourceYield,
                        statValue,
                        nameof(IResourceProduction.ResourceYield),
                        building);
                    break;
                case StatModifiers.Capacity:
                    DoModWithUpdate(
                        building.LocalRes.capacity,
                        statValue,
                        nameof(building.LocalRes),
                        building);
                    break;
            }
            //Debug.Log(pair.mask);
        }

        void DoModWithUpdate(IModifiable obj, StatValue statValue, string propName, Building building)
        {
            obj.AddMod(statValue);
            building.UIUpdate(propName);
        }

        void DoMod(IModifiable obj, StatValue statValue, Building building)
        {
            obj.AddMod(statValue);
            InfoWindow.Window.buildingCostChange?.Invoke(building);
        }

        public void AddEffect()
        {
            ApplyStat(true);
        }

        public void RemoveEffect()
        {
            ApplyStat(false);
        }
    }
    [Serializable]
    public class BuildingStatCateg : DataCategory<Stat>
    {
    }

    [CreateAssetMenu(fileName = "Stats", menuName = "UI Data/Stats", order = 2)]
    public class StatData : DataHolder<BuildingStatCateg, Stat>
    {
        public new static string PATH = "Stats";
#if UNITY_EDITOR
        public new const string EDITOR_PATH = "Assets/Game Data/Research and Building/Stats.asset";
#endif
    }
}