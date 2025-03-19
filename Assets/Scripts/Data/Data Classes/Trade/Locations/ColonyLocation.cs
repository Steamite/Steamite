using System.Collections.Generic;
using System;
using UnityEngine;
using TradeData.Stats;

namespace TradeData.Locations
{
    /// <summary>Colony location show where the colony is, provides some passive production.</summary>
    [Serializable]
    public class ColonyLocation : Location
    {
        /// <summary>Upgradable passive production data.</summary>
        [SerializeField] public List<ColonyStat> passiveProductions;
        /// <summary>Colony stats data.</summary>
        [SerializeField] public List<ColonyStat> stats;

		public void DoProduction()
		{
			foreach(ColonyStat stat in stats)
            {
                stat.DoStat();
            }
		}
	}
}
