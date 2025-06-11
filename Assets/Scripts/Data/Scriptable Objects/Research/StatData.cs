using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
                    while(newMask > 0)
                    {
                        if ((newMask & 1) == 1 || newMask == -1)
                        {
                            HandleCases(_building, pair);
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
                    //_building.cost ;
                    break;
                case StatModifiers.AssignLimit:
                    ((IModifiable)((IAssign)building).AssignLimit).AddMod(pair);
                    building.UIUpdate(nameof(IAssign.AssignLimit));
                    break;
                case StatModifiers.ProdSpeed:
                    //((IModifiable)((IProduction)building).ProductionCost).AddMod(pair);
                    //((IProduction)building).Modifier += pair.modAmmount * 0.01f;
                    building.UIUpdate(nameof(IProduction.Modifier));
                    break;
                case StatModifiers.InputResource:
                    ((IModifiable)((IResourceProduction)building).ProductionCost).AddMod(pair);
                    building.UIUpdate(nameof(IResourceProduction.ProductionCost));
                    break;
                case StatModifiers.ProductionYield:
                    ((IModifiable)((IResourceProduction)building).ProductionYield).AddMod(pair);
                    building.UIUpdate(nameof(IResourceProduction.ProductionYield));
                    break;
                case StatModifiers.Capacity:
                    //building.LocalRes.capacity = 
                    break;
            }
            Debug.Log(pair.mask);
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