using System.Collections.Generic;
using System;

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
    }
}
