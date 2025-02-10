using System.Collections;
using System.Collections.Generic;
using TradeData.Locations;
using UnityEngine;

/// <summary>Holds data to modify about trading.</summary>
[CreateAssetMenu(fileName = "Trade Data", menuName = "UI Data/Trade Holder", order = 1)]
public class TradeHolder : ScriptableObject
{
    /// <summary>All posible starting locations(WIP).</summary>
    public List<ColonyLocation> startingLocations;
    /// <summary>All posible trade locations(WIP).</summary>
    public List<TradeLocation> tradeLocations = new();
}
