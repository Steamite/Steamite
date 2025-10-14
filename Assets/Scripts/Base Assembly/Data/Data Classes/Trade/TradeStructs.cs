using System;
using UnityEngine;

/// <summary>Customizes Trade deals</summary>
[Serializable]
public class TradeDeal
{
    /// <summary>Resource type to trade.</summary>
    [SerializeReference]public ResourceType type;
    /// <summary>Cost for one unit.</summary>
    public int cost;

    public TradeDeal()
    {
        type = null;
        cost = 0;
    }

    public TradeDeal(TradeDealSave save)
    {
        type = ResFluidTypes.GetResByIndex(save.type);
        cost = 0;
    }
}