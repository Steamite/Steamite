using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TradeData.Locations;
using UnityEngine;

namespace Outposts
{
    public struct UpgradeCost
    {
        public int timeInTicks;
        public MoneyResource resource;

        public UpgradeCost(float _timeInHours, MoneyResource _resource)
        {
            timeInTicks = Mathf.RoundToInt(_timeInHours * 4);
            resource = _resource;
        }
    }

    [Serializable]
    public class Outpost : Location
    {
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
            { ResourceType.Wood, 10 },
        };

        public static List<UpgradeCost> upgradeCosts = new()
        {
            new
            (
                2,
                new
                (
                    new(){ResourceType.Metal},
                    new(){10},
                    1000
                )
            ),
            new
            (
                3,
                new
                (
                    new(){ResourceType.Metal},
                    new(){25},
                    500
                )
            ),
            new
            (
                4,
                new
                (
                    new(){ResourceType.Metal},
                    new(){40},
                    750
                )
            ),
        };

        public const int MAX_LEVEL = 3;

        public int level;
        /// <summary>
        /// If <see cref="exists"/> is false marks start date of production, <br/>
        /// else shows how much time is left until finishing the upgrade.
        /// </summary>
        public int timeToFinish;
        public bool exists;
        public bool buildInProgress;
        public Resource production;
        public ResourceType[] outpostLevels;

        public CapacityResource storedResources = new();

        [JsonIgnore]
        public Action OnUpgrade { get => onUpgrade; set => onUpgrade = value; }
        [JsonIgnore]
        Action onUpgrade;

        public Outpost() { }
        public Outpost(string _name)
        {
            Name = _name;
            level = 0;
            outpostLevels = new ResourceType[MAX_LEVEL];
            production = new();
            exists = false;
            buildInProgress = false;
            timeToFinish = -1;
        }

        /// <summary>
        /// Starts the upgrade process and pays the cost.
        /// </summary>
        public void StartUpgrade(ResourceType selectedType)
        {
            buildInProgress = true;
            timeToFinish = upgradeCosts[level].timeInTicks;
            MyRes.PayCostGlobal(upgradeCosts[level].resource);
            outpostLevels[level] = selectedType;

            if (level == 0)
            {
                /*Trade trade = UIRefs.trade;
                Button button = trade.AddOutpostButton(trade.transform.GetChild(0).GetChild(2), trade.outposts.Count+1);*/
            }
            SceneRefs.Tick.SubscribeToEvent(ProgressBuilding, Tick.TimeEventType.Ticks);
        }

        /// <summary>
        /// Ends the upgrade process and marks when it finished.
        /// </summary>
        public void FinishUpgrade()
        {
            SceneRefs.Tick.UnsubscribeToEvent(ProgressBuilding, Tick.TimeEventType.Ticks);
            exists = true;
            buildInProgress = false;
            if (level == 0)
            {
                onUpgrade();
                storedResources = new(10);
                /*Trade trade = UIRefs.trade;
                trade.transform.GetChild(0).GetChild(2).GetChild(trade.outposts.Count-1).GetChild(0).GetComponent<Image>().color = trade.availableColor;*/
            }
            else
                storedResources.capacity.ChangeBaseVal((level + 1) * 10);

            production.ManageSimple(outpostLevels[level], resourceAmmount[outpostLevels[level]], true); // upgrades the production
            level++;
            for (int i = 0; i < level; i++)
            {
                if (!storedResources.types.Contains(outpostLevels[i]))
                    storedResources.ManageSimple(outpostLevels[i], 0, true);
            }
            timeToFinish = SceneRefs.Tick.GetWeekTime(); // marks the finished time
            SceneRefs.Tick.SubscribeToEvent(MakeWeekProduction, Tick.TimeEventType.Week);
        }

        public bool CanAffordUpgrade()
        {
            return MyRes.CanAfford(upgradeCosts[level].resource);
        }

        public void ProgressBuilding()
        {
            timeToFinish--;
            if(timeToFinish == 0)
            {
                FinishUpgrade();
            }
        }

        public void MakeWeekProduction()
        {
            int mod = SceneRefs.Tick.GetWeekTimeFull();
            storedResources.Manage(production, true, (mod - timeToFinish) / (float)mod);
        }
    }
}