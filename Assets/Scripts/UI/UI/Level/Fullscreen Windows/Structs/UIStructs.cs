using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class TradeSave
{
    public ColonyLocation colonyLocation;
    public List<TradeLocation> tradeLocations;
    public List<TradeExpedition> expeditions;
    public List<Outpost> outposts;
    public int money;
    public TradeSave(Trade t)
    {
        colonyLocation = t.colonyLocation;
        tradeLocations = t.tradeLocations;
        expeditions = t.expeditions;
        outposts = t.outposts;
        money = MyRes.money;
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