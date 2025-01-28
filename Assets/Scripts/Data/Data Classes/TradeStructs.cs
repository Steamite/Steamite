using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Customizes Trade deals</summary>
[Serializable]
public struct TradeDeal
{
    /// <summary>Name just for inspector.</summary>
    public string name;
    /// <summary>Resource type to trade.</summary>
    public ResourceType type;
    /// <summary>Cost for one unit.</summary>
    public int cost;
}

/// <summary>Trade locations.</summary>
[Serializable]
public class TradeLocation
{
    /// <summary>Location display name.</summary>
    [Header("Location")] public string name;
    /// <summary>Location world position.</summary>
    public GridPos pos;
    /// <summary>Selling deals.</summary>
    public List<TradeDeal> wantToSell;
    /// <summary>Buing deals.</summary>
    public List<TradeDeal> wantToBuy;
}