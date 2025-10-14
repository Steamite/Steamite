using Outposts;
using ResearchUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TradeData.Locations;


[Serializable]
public class TradeSave
{
    public string colonyLocation;
    public List<int> prodLevels;
    public List<int> statLevels;
    public List<TradeLocationSave> tradeLocations;
    public List<TradeConvoySave> convoys;
    public List<OutpostSave> outposts;
    public int money;

    public TradeSave(TradingWindow trading)
    {
        colonyLocation = trading.colonyLocation.Name;
        prodLevels = trading.colonyLocation.production.Select(q => q.CurrentState).ToList();
        statLevels = trading.colonyLocation.stats.Select(q => q.CurrentState).ToList();
        tradeLocations = trading.tradeLocations.Select(q => new TradeLocationSave(q)).ToList();
        convoys = trading.GetConvoys().Select(q => new TradeConvoySave(q)).ToList();
        outposts = trading.outposts.Select(q=> new OutpostSave(q)).ToList();
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

[Serializable]
public class TradeConvoySave
{
    public bool firstPhase = true;
    public int tradeLocation = -1;

    public float currentprogress = 0;
    public float maxprogress = -1;

    public ResourceSave buying;
    public int reward;

    public TradeConvoySave(TradeConvoy convoy)
    {
        firstPhase = convoy.firstPhase;
        tradeLocation = convoy.tradeLocation;
        
        currentprogress = convoy.currentprogress;
        maxprogress = convoy.maxprogress;

        buying = new(convoy.buying);
        reward = convoy.reward;
    }

    public TradeConvoySave()
    {

    }
}


[Serializable]
public class OutpostSave : LocationSave
{
    public int level;
    /// <summary>
    /// If <see cref="exists"/> is false marks start date of production, <br/>
    /// else shows how much time is left until finishing the upgrade.
    /// </summary>
    public int timeToFinish;
    public bool exists;
    public bool buildInProgress;
    public ResourceSave production;
    public int[] outpostLevels;

    public ResourceSave storedResources = new();

    public OutpostSave() : base() { }
    public OutpostSave(string _name)
    {
        outpostLevels = new int[Outpost.MAX_LEVEL];
        production = new();
        exists = false;
        buildInProgress = false;
        timeToFinish = -1;
        name = _name;
        level = 0;
    }

    public OutpostSave(Outpost outpost) : base(outpost)
    {
        level = outpost.level;
        timeToFinish = outpost.timeToFinish;
        exists = outpost.exists;
        buildInProgress = outpost.buildInProgress;
        production = new ResourceSave(outpost.production);
        outpostLevels = outpost.outpostLevels.Select(q => ResFluidTypes.GetResourceIndex(q)).ToArray();
        storedResources = new(outpost.storedResources);
    }
}