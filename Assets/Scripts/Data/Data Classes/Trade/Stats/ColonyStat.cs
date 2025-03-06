using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TradeData.Stats
{
    /// <summary>Base class that each type of state must inherit from.</summary>
    public abstract class ColonyStat : ScriptableObject
    {
        public const int MAX_STAT_LEVEL = 5;

        /// <summary>Name of the stat in display.</summary>
        public string displayName;
        /// <summary>Current state, starting value set whe initializing a new game.</summary>
        [HideInInspector] public int currentState;
        /// <summary>Max possible state, set when initialining a new game.</summary>
        public int maxState;

        /// <summary>Resources needed for upgrading to that level.</summary>
        [SerializeField]public List<Resource> resourceUpgradeCost;

        /// <summary>
        /// Inits empty upgradeCosts.
        /// </summary>
        public ColonyStat()
        {
            resourceUpgradeCost = new();
            for (int i = 0; i < MAX_STAT_LEVEL; i++)
            {
                resourceUpgradeCost.Add(new());
            }
        }

        /// <summary>Provides production based on the <see cref="currentState"/>.</summary>
        public abstract void DoStat();

        /// <summary>Summary for one stat level.</summary>
        public abstract string GetText(int state);

        /// <summary>
        /// Returns text for the small label on top, or if <paramref name="complete"/> is true the complete summary.
        /// </summary>
        /// <param name="complete">If true, returns complete summary else only number and icon</param>
        public abstract string GetText(bool complete);

        /// <summary>
        /// Calculates which level of the stat that can be currenly unlocked.
        /// </summary>
        /// <returns>Maximal currently affordable state</returns>
        public int CanAfford()
        {
            if (currentState == maxState)
                return -1;
            Resource resourceCost = new();
            int moneyCost = 0, maxAffordable = 0;
            for (maxAffordable = currentState; maxAffordable < maxState; maxAffordable++)
            {
                MyRes.ManageRes(resourceCost, resourceUpgradeCost[currentState], 1);
                moneyCost += resourceUpgradeCost[currentState].capacity;
                if (moneyCost > MyRes.Money || !MyRes.CanAfford(resourceCost))
                    break;
            }
            return maxAffordable;
        }
    }
}
