using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Outpost
{
    public struct UpgradeCost
    {
        public int money;
        public int timeInTicks;
        public Resource resource;

        public UpgradeCost(int _money, float _timeInHours, Resource _resource)
        {
            money = _money;
            timeInTicks = Mathf.RoundToInt(_timeInHours * 4);
            resource = _resource;
        }
    }

    public static Dictionary<ResourceType, int> resourceCosts = new()
    {
        { ResourceType.Coal,  1 },
        { ResourceType.Metal, 2 },
        { ResourceType.Stone, 1 },
        { ResourceType.Food,  1 },
    };

    public static Dictionary<ResourceType, int> resourceAmmount = new()
    {
        { ResourceType.Coal, 10 },
        { ResourceType.Metal, 7 },
        { ResourceType.Stone, 5 },
        { ResourceType.Food, 10 },
    };

    public static List<UpgradeCost> upgradeCosts = new()
    {
        new(1000, 7, new(
            new(){ResourceType.Metal},
            new(){10})),
        new(2000, 5, new(
            new(){ResourceType.Metal},
            new(){40})),
        new(4000, 4, new(
            new(){ResourceType.Metal, ResourceType.Stone},
            new(){60, 15})),
    };

    public string name;
    public int level;
    /// <summary>
    /// <para>If <see cref="constructed"/> is false marks start date of production, <br/>
    /// else shows how much time is left until finishing the upgrade.</para>
    /// </summary>
    public int timeToFinish;
    public bool constructed;
    public Resource production;

    public Outpost(string _name, ResourceType type)
    {
        name = _name;
        level = 0;
        production = new(
            new() { type }, 
            new() { 0 });
        StartUpgrade();
    }
    public Outpost()
    {
        
    }

    /// <summary>
    /// Starts the upgrade process.
    /// </summary>
    public void StartUpgrade()
    {
        constructed = false;
        timeToFinish = upgradeCosts[level].timeInTicks;
        MyRes.TakeFromGlobalStorage(upgradeCosts[level].resource);
        MyRes.ManageMoney(-upgradeCosts[level].money);
        if (level == 0)
        {
            Trade trade = CanvasManager.trade;
            Button button = trade.AddOutpostButton(trade.transform.GetChild(0).GetChild(2), trade.outposts.Count+1);
        }
    }

    /// <summary>
    /// Ends the upgrade process and marks when it finished.
    /// </summary>
    public void Upgrade()
    {
        constructed = true;
        if(level == 0)
        {
            Trade trade = CanvasManager.trade;
            trade.transform.GetChild(0).GetChild(2).GetChild(trade.outposts.Count-1).GetChild(0).GetComponent<Image>().color = trade.availableColor;
        }
        level++;
        timeToFinish = SceneRefs.tick.timeController.GetWeekTime();
        production.ammount[0] += resourceAmmount[production.type[0]];
    }
}