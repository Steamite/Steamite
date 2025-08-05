using UnityEngine;

namespace TradeData.Locations
{
    public abstract class Location
    {
        /// <summary>Location display name.</summary>
        [Header("Location")] public string name;
        /// <summary>Location world position.</summary>
        public GridPos pos;
    }

}
