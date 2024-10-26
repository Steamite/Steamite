using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TradeLocation
{
    [Header("Location")]
    public string name;
    public List<TradeDeal> wantToSell;
    public List<TradeDeal> wantToBuy;
}