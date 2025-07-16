using ResearchUI;
using System;
using System.Collections.Generic;
using System.Linq;
using TradeData.Locations;

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
    public TradeSave(TradingWindow trading)
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
    public int count;
    public List<List<float>> saveData;
    public List<(int cat, int id)> queue;

    public ResearchSave()
    {

    }
    public ResearchSave(ResearchWindow window)
    {
        saveData = new();
        queue = new();

        count = 0;
        for (int i = 0; i < window.researchData.Categories.Count; i++)
        {
            List<float> saves = new();
            for (int j = 0; j < window.researchData.Categories[i].Objects.Count; j++)
            {
                saves.Add(window.researchData.Categories[i].Objects[j].CurrentTime);
                if (window.researchData.Categories[i].Objects[j].Equals(window.currentResearch) &&
                    window.researchData.Categories[i].Objects[j].Name == window.currentResearch.Name)
                    queue.Add(new(i, window.researchData.Categories[i].Objects[j].id));
            }
            saveData.Add(saves);
        }
    }

    public ResearchSave(ResearchData data)
    {
        saveData = new();
        queue = new();

        count = 0;
        for (int i = 0; i < data.Categories.Count; i++)
        {
            List<float> saves = new();
            for (int j = 0; j < data.Categories[i].Objects.Count; j++)
            {
                saves.Add(data.Categories[i].Objects[j].CurrentTime);
            }
            saveData.Add(saves);
        }
    }
}