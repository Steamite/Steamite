using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Trade Data", menuName = "UI Data/Trade Holder", order = 1)]
public class TradeHolder : ScriptableObject
{
    public List<ColonyLocation> startingLocations = new();
    public List<TradeLocation> tradeLocations = new();
}
