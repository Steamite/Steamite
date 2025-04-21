using System.Collections.Generic;
using TradeData.Locations;
using UnityEngine;

/// <summary>Each holder represents one starting location.</summary>
[CreateAssetMenu(fileName = "Trade Data", menuName = "UI Data/Trade/Trade Data", order = 1)]
public class TradeHolder : ScriptableObject
{
    /// <summary>Starting location.</summary>
    public ColonyLocation startingLocation;
    /// <summary>All trade locations.</summary>
    [Header("Locations")] public List<TradeLocation> tradeLocations = new();

}