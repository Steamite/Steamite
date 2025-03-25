using System.Collections.Generic;
using System;
using UnityEngine;
using TradeData.Locations;
using System.Linq;

public struct Save
{
    public WorldSave world;
    public GameStateSave gameState;
    public HumanSave[] humans;
    public ResearchSave research;
    public TradeSave trade;

    //public string worldName;
}


[Serializable]
public class TradeSave
{
    public string colonyLocation;
    public List<int> prodLevels;
    public List<int> statLevels;
    public List<TradeLocation> tradeLocations;
    public List<TradeConvoy> convoys;
    //public List<Outpost> outposts;
    public int money;
    public TradeSave(Trading trading)
    {
        colonyLocation = trading.colonyLocation.name;
        prodLevels = trading.colonyLocation.production.Select(q => q.CurrentState).ToList();
        statLevels = trading.colonyLocation.stats.Select(q => q.CurrentState).ToList();
        tradeLocations = trading.tradeLocations;
        convoys = trading.GetConvoys();
        //outposts = trading.outposts;
        money = MyRes.Money;
    }
    public TradeSave()
    {

    }
}

[Serializable]
public class ResearchSave
{
    public int currentResearch;
    public ResearchCategory[] categories;

    public ResearchSave(int categCount)
    {
        categories = new ResearchCategory[categCount];
    }
    public ResearchSave()
    {

    }
}