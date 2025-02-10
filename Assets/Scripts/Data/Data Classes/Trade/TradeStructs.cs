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