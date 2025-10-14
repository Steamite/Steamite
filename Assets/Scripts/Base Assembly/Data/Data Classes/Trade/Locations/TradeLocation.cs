using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TradeData.Locations
{
    /// <summary>Trade locations.</summary>
    [Serializable]
    public class TradeLocation : Location
    {
        /// <summary>Buing deals.</summary>
        public List<TradeDeal> Buy;
        /// <summary>Selling deals.</summary>
        public List<TradeDeal> Sell;
        [HideInInspector] public float distance;

        public TradeLocation()
        {

        }

        public TradeLocation(TradeLocationSave save)
        {
            pos = save.position;
            Name = save.name;
            Buy = save.tradeDealsBuy.Select(q => new TradeDeal(q)).ToList();
            Sell = save.tradeDealsSell.Select(q => new TradeDeal(q)).ToList();
            distance = save.distance;
        }
    }
}
