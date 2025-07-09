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
    public class StatPair
    {
        /// <summary>Which buildings are effected.</summary>
        public int mask;
        public int underProp = 0;
        /// <summary>Which properies are effected.</summary>
        public StatModifiers mod;
        /// <summary>How much much it effects it.</summary>
        public float modAmmount;
        /// <summary>If the modification is absolute or by a percentage;</summary>
        public bool percent;
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
        public void Mask(bool addStat)
        {
            // create a mask with the affected categories
            int mask = 0;
            List<Building> buildings = MyGrid.Buildings
                .Union(SceneRefs.objectFactory.buildPrefabs.Categories
                    .SelectMany(q => q.Objects)
                        .Select(w => w.building)).ToList();
            int j = Enum.GetNames(typeof(BuildingCategType)).Length;
            foreach (var pair in pairs)
            {
                mask = pair.mask;
                foreach (var _building in buildings)
                {
                    // filter buildings using the mask 
                    int newMask = _building.BuildingCateg & mask;
                    // loop though the mask and do the effect
                    while (newMask > 0)
                    {
                        if ((newMask & 1) == 1 || newMask == -1)
                        {
                            try
                            {
                                HandleCases(_building, pair);
                            }
                            catch (Exception e)
                            {
                                if (e is InvalidCastException)
                                {
                                    Debug.LogError(
                                        $"{_building} doesnt implement inteface containing: ${pair.mod}\n" +
                                        $"{e}");

                                }
                                else
                                    Debug.LogError(e);
                            }
                        }
                        newMask = newMask >> 1;
                        if (newMask == 0)
                            break;
                    }
                }
            }
        }

        void HandleCases(Building building, StatPair pair)
        {

            switch (pair.mod)
            {
                case StatModifiers.Cost:
                    if (building.id == -1)
                    {
                        DoMod(
                            building.Cost,
                            pair,
                            building);
                    }
                    break;
                case StatModifiers.AssignLimit:
                    DoMod(
                        ((IAssign)building).AssignLimit,
                        pair,
                        nameof(IAssign.AssignLimit),
                        building);
                    break;
                // TODO TEST THIS(no indicator exists right now)
                case StatModifiers.ProdSpeed:
                    DoMod(
                        ((IProduction)building).ProdSpeed,
                        pair,
                        nameof(IProduction.ProdSpeed),
                        building);
                    break;
                case StatModifiers.InputResource:
                    DoMod(
                        ((IResourceProduction)building).ResourceCost,
                        pair,
                        nameof(IResourceProduction.ResourceCost),
                        building);
                    break;
                case StatModifiers.ProductionYield:
                    DoMod(
                        ((IResourceProduction)building).ResourceYield,
                        pair,
                        nameof(IResourceProduction.ResourceYield),
                        building);
                    break;
                case StatModifiers.Capacity:
                    DoMod(
                        building.LocalRes.capacity,
                        pair,
                        nameof(building.LocalRes),
                        building);
                    break;
            }
            //Debug.Log(pair.mask);
        }

        void DoMod(IModifiable obj, StatPair pair, string propName, Building building)
        {
            obj.AddMod(pair);
            building.UIUpdate(propName);
        }

        void DoMod(IModifiable obj, StatPair pair, Building building)
        {
            obj.AddMod(pair);
            SceneRefs.infoWindow.buildingCostChange?.Invoke(building);
        }

        public void AddEffect()
        {
            Mask(true);
        }

        public void RemoveEffect()
        {
            Mask(false);
        }
    }
    [Serializable]
    public class BuildingStatCateg : DataCategory<Stat>
    {
    }

    [CreateAssetMenu(fileName = "Stats", menuName = "UI Data/Stats", order = 2)]
    public class StatData : DataHolder<BuildingStatCateg, Stat>
    {
    }
}