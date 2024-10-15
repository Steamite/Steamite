using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TradeLocation
{
    public string name;
    public List<TradeDeal> selling;
    public List<TradeDeal> buying;
}