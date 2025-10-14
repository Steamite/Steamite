using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static Dictionary<ResourceType, int> ResourceCosts = new()
        {
            { ResFluidTypes.GetResByName("Coal"),  1 },
            { ResFluidTypes.GetResByName("Metal"), 2 },
            { ResFluidTypes.GetResByName("Stone"), 1 },
            { ResFluidTypes.GetResByName("Food"),  1 },
        };

        public static Dictionary<ResourceType, int> ResourceAmmount = new()
        {
            { ResFluidTypes.GetResByName("Coal"), 10 },
            { ResFluidTypes.GetResByName("Stone"), 7 },
            { ResFluidTypes.GetResByName("Metal"), 5 },
            { ResFluidTypes.GetResByName("Wood"), 10 },
            { ResFluidTypes.GetResByName("Food"), 10 },
        };

        public static List<UpgradeCost> UpgradeCosts = new()
        {
            new
            (
                2,
                new
                (
                    new(){ResFluidTypes.GetResByName("Metal")},
                    new(){10},
                    1000
                )
            ),
            new
            (
                3,
                new
                (
                    new(){ResFluidTypes.GetResByName("Metal")},
                    new(){25},
                    500
                )
            ),
            new
            (
                4,
                new
                (
                    new(){ResFluidTypes.GetResByName("Metal")},
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
        public Outpost(OutpostSave outpost)
        {
            level = outpost.level;
            timeToFinish = outpost.timeToFinish;
            exists = outpost.exists;
            buildInProgress = outpost.buildInProgress;
            production = new(outpost.production);
            outpostLevels = outpost.outpostLevels.Select(q => ResFluidTypes.GetResByIndex(q)).ToArray();
            storedResources = new(outpost.storedResources, level * 10);// (level * 10)
        }

        /// <summary>
        /// Starts the upgrade process and pays the cost.
        /// </summary>
        public void StartUpgrade(ResourceType selectedType)
        {
            buildInProgress = true;
            timeToFinish = UpgradeCosts[level].timeInTicks;
            MyRes.PayCostGlobal(UpgradeCosts[level].resource);
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

            production.ManageSimple(outpostLevels[level], ResourceAmmount[outpostLevels[level]], true); // upgrades the production
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
            return MyRes.CanAfford(UpgradeCosts[level].resource);
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