using Newtonsoft.Json;
using System;
using UnityEngine;

namespace TradeData.Locations
{
    [Serializable]
    public abstract class Location : INameChangable
    {
        /// <summary>Location display name.</summary>
        [Header("Location")][SerializeField]
        [JsonProperty]
        string name;
        /// <summary>Location world position.</summary>
        public GridPos pos;

        [JsonIgnore] public string Name { get => name; set => name = value; }
    }

}
