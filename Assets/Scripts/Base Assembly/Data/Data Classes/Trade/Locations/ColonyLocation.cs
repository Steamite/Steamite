using System;
using System.Collections.Generic;
using System.Linq;
using TradeData.Stats;
using UnityEngine;

namespace TradeData.Locations
{
    /// <summary>Colony location show where the colony is, provides some passive production.</summary>
    [Serializable]
    public class ColonyLocation : Location
    {
        /// <summary>Upgradable passive production data.</summary>
        [SerializeField] public StatData config;

        [HideInInspector] public List<ColonyStat> stats;
        [HideInInspector] public List<ColonyStat> production;

        public void DoProduction()
        {
            foreach (ColonyStat stat in stats)
            {
                stat.DoStat();
            }
        }

        public void LoadGame(List<int> prodLevels, List<int> statLevels)
        {
            stats = Resources.LoadAll<ColonyStat>("Holders/Data/Stats").ToList();
            for (int i = 0; i < stats.Count; i++)
                stats[i].LoadState(prodLevels[i], config.stats[i].max);

            production = Resources.LoadAll<ColonyStat>("Holders/Data/Prods").ToList();
            for (int i = 0; i < production.Count; i++)
                production[i].LoadState(prodLevels[i], config.production[i].max);
        }
    }
}
