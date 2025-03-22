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
        [SerializeField] public List<StatData> passiveProductions;
        /// <summary>Colony stats data.</summary>
        [SerializeField] public List<StatData> stats;


		public void DoProduction()
		{
			/*foreach(ColonyStat stat in stats)
            {
                stat.DoStat();
            }*/
		}

		public void NewGame(List<ColonyStat> stats)
		{
            /*foreach (ColonyStat production in passiveProductions)
                production.LoadState();
			foreach (ColonyStat production in stats)
				production.LoadState();*/
		}
	}
}
