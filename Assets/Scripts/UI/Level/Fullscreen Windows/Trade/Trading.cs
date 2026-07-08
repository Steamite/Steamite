using Outposts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeData.Locations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class Trading : IGameDataController<TradeSave>
{
    #region Const
    public const int CONVOY_STORAGE_LIMIT = 50;
    public const int CONVOY_SPEED = 10;
    public const int MAX_CONVOYS = 3;
    public const int MAX_OUTPOSTS = 3;

    public static Dictionary<ResourceType, int> RESOURCE_COSTS = new();
    #endregion

    #region Variables

    [SerializeField] List<TradeConvoy> convoys;

    ColonyLocation colonyLocation;
    List<TradeLocation> tradeLocations;
    List<Outpost> outposts;

    //[SerializeField] string baseLocation = "Highlands";


    #endregion

    #region Properties
    public int AvailableConvoy => MAX_CONVOYS - convoys.Count;


    public bool ConvoyOnRoute(int locationIndex) => convoys.Count(q => q.tradeLocation == locationIndex) == 1;
    public void RemoveConvoy(TradeConvoy convoy) => convoys.Remove(convoy);
    public List<TradeConvoy> Convoys => convoys;

    public ColonyLocation ColonyLocation { get => colonyLocation; set => colonyLocation = value; }
    public List<TradeLocation> TradeLocations { get => tradeLocations; set => tradeLocations = value; }
    public List<Outpost> Outposts { get => outposts; set => outposts = value; }
    #endregion

    public override async Task LoadState(TradeSave tradeSave)
    {
        TradeHolder tradeHolder = Instantiate(await Addressables.LoadAssetAsync<TradeHolder>($"Colony Locations/{tradeSave.colonyLocation}.asset").Task);
        ColonyLocation = tradeHolder.startingLocation;
        ColonyLocation.LoadGame(tradeSave.prodLevels, tradeSave.statLevels);

        TradeLocations = tradeSave.tradeLocations.Select(q => new TradeLocation(q)).ToList();
        convoys = tradeSave.convoys.Select(q => new TradeConvoy(q)).ToList();
        Outposts = tradeSave.outposts.Select(q => new Outpost(q)).ToList();
        SceneRefs.Stats.GetComponent<ResourceDisplay>().Money = tradeSave.money;

        //Moves all convoys each tick.
        SceneRefs.Tick.SubscribeToEvent(
            () =>
            {
                for (int i = convoys.Count - 1; i >= 0; i--)
                    convoys[i].Move(CONVOY_SPEED);
            },
           Tick.TimeEventType.Ticks);

        foreach (var item in Outposts)
        {
            if (item.buildInProgress)
            {
                SceneRefs.Tick.SubscribeToEvent(
                    item.ProgressBuilding,
                    Tick.TimeEventType.Ticks);
            }
            else if (item.exists && item.production.Sum() != 0)
            {
                SceneRefs.Tick.SubscribeToEvent(item.MakeWeekProduction, Tick.TimeEventType.Week);
            }

        }


        SceneRefs.Tick.SubscribeToEvent(
            ColonyLocation.DoProduction,
            Tick.TimeEventType.Week);

        AfterLoad();
    }
    public void Trade(TradeConvoy convoy, Resource sellResource, int buyMoney)
    {
        convoys.Add(convoy);
        MyRes.PayCostGlobal(sellResource, buyMoney);
    }

    public override TradeSave SaveState()
    {
        TradeSave tradeSave = new()
        {
            colonyLocation = ColonyLocation.Name,
            prodLevels = ColonyLocation.production.Select(q => q.CurrentState).ToList(),
            statLevels = ColonyLocation.stats.Select(q => q.CurrentState).ToList(),
            tradeLocations = TradeLocations.Select(q => new TradeLocationSave(q)).ToList(),
            convoys = convoys.Select(q => new TradeConvoySave(q)).ToList(),
            outposts = Outposts.Select(q => new OutpostSave(q)).ToList(),
            money = MyRes.Money
        };
        return tradeSave;
    }
}
