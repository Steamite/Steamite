using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace BuildingStats
{
    public enum StatModifiers
    {
        Cost,
        AssignLimit,
        ProdSpeed,
        InputResource,
        ProductionYield
    }

    [Serializable]
    public class StatPair
    {
        /// <summary>Which buildings does this effect.</summary>
        public BuildingCategType type;
        /// <summary>Which properies does this effect.</summary>
        public StatModifiers mod;
        /// <summary>How much much it effects it.</summary>
        public float modAmmount;
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
            for (int i = 0; i < pairs.Count; i++)
            {
                mask = 1 << ((int)pairs[i].type) | mask;
            }

            List<Building> buildings = MyGrid.Buildings;
            int j = Enum.GetNames(typeof(BuildingCategType)).Length;
            foreach (var _building in buildings)
            {
                // filter buildings using the mask 
                int newMask = _building.BuildingCateg & mask;
                if (newMask > 0)
                {
                    // loop though the mask and do the effect
                    for (int i = 0; i < j; i++)
                    {
                        if ((newMask & 1) == 1 || newMask == -1)
                        {
                            StatPair pair = pairs.First(q => q.type == (BuildingCategType)i);
                            switch (pair.mod)
                            {
                                case StatModifiers.Cost:
                                    //_building.cost ;
                                    break;
                                case StatModifiers.AssignLimit:
                                    ((IAssign)_building).AssignLimit += Convert.ToInt32(pair.modAmmount);
                                    _building.UIUpdate(nameof(IAssign.AssignLimit));
                                    break;
                                case StatModifiers.ProdSpeed:
                                    ((IProduction)_building).Modifier += pair.modAmmount * 0.01f;
                                    _building.UIUpdate(nameof(IProduction.Modifier));
                                    break;
                                case StatModifiers.InputResource:
                                    ((IResourceProduction)_building).ProductionCost.Modifier += pair.modAmmount * 0.01f;
                                    _building.UIUpdate(nameof(IResourceProduction.ProductionCost));
                                    break;
                                case StatModifiers.ProductionYield:
                                    ((IResourceProduction)_building).ProductionYield.Modifier += pair.modAmmount * 0.01f;
                                    _building.UIUpdate(nameof(IProduction.Modifier));
                                    break;
                            }
                            Debug.Log(pair.type);
                        }
                        newMask = newMask >> 1;
                        if (newMask == 0)
                            break;
                    }
                }
            }
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