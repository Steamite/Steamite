using System.Collections.Generic;
using System;
using UnityEngine;
public struct Save
{
    public WorldSave world;
    public GameStateSave gameState;
    public HumanSave[] humans;
    public ResearchSave research;
    public TradeSave trade;

    public string worldName;
}


[Serializable]
public class TradeSave
{
    public ColonyLocation colonyLocation;
    public List<TradeLocation> tradeLocations;
    public List<TradeConvoy> expeditions;
    public List<Outpost> outposts;
    public int money;
    /*public TradeSave( t)
    {
        colonyLocation = t.colonyLocation;
        tradeLocations = t.tradeLocations;
        expeditions = t.expeditions;
        outposts = t.outposts;
        money = MyRes.Money;
    }*/
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