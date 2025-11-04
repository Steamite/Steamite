using Outposts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeData.Locations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class TradingWindow : FullscreenWindow, IGameDataController<TradeSave>
{
    #region Const
    public const int CONVOY_STORAGE_LIMIT = 50;
    public const int CONVOY_SPEED = 10;
    public int MAX_CONVOYS = 3;

    public static Dictionary<ResourceType, int> RESOURCE_COSTS = new();
    /**/
    #endregion

    #region Variables
    public IFullScreenWindowElem map;

    [SerializeField] List<TradeConvoy> convoys;

    public ColonyLocation colonyLocation;
    public List<TradeLocation> tradeLocations;
    public List<Outpost> outposts;

    [HideInInspector] public float distance;
    //[SerializeField] string baseLocation = "Highlands";

    public int outpostLimit = 3;

    #endregion

    #region Properties
    public int AvailableConvoy => MAX_CONVOYS - convoys.Count;
    public bool ConvoyOnRoute(int locationIndex) => convoys.Count(q => q.tradeLocation == locationIndex) == 1;
    public void RemoveConvoy(TradeConvoy convoy) => convoys.Remove(convoy);
    public List<TradeConvoy> GetConvoys() => convoys;
    #endregion

    public async Task LoadState(TradeSave tradeSave)
    {
        RESOURCE_COSTS = new()
        {
            { ResFluidTypes.None, 0},
            { ResFluidTypes.GetResByName("Coal"), 3},
            { ResFluidTypes.GetResByName("Metal"), 7},
            { ResFluidTypes.GetResByName("Stone"), 10},
            { ResFluidTypes.GetResByName("Meat"), 15},
            { ResFluidTypes.GetResByName("Wood"), 12},
        };
        TradeHolder tradeHolder = Instantiate(await Addressables.LoadAssetAsync<TradeHolder>($"Assets/Game Data/Colony Locations/{tradeSave.colonyLocation}.asset").Task);
        colonyLocation = tradeHolder.startingLocation;
        colonyLocation.LoadGame(tradeSave.prodLevels, tradeSave.statLevels);

        tradeLocations = tradeSave.tradeLocations.Select(q => new TradeLocation(q)).ToList();
        convoys = tradeSave.convoys.Select(q => new TradeConvoy(q)).ToList();
        outposts = tradeSave.outposts.Select(q => new Outpost(q)).ToList();
        SceneRefs.Stats.GetComponent<ResourceDisplay>().Money = tradeSave.money;
        GetWindow();
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        map = (IFullScreenWindowElem)root.Q<VisualElement>("Map");
        ((IInitiableUI)map).Init();
        ((IInitiableUI)root.Q<VisualElement>("Colony")[0]).Init();


        //Moves all convoys each tick.
        SceneRefs.Tick.SubscribeToEvent(
            () =>
            {
                for (int i = convoys.Count - 1; i >= 0; i--)
                    convoys[i].Move(CONVOY_SPEED);
            },
           Tick.TimeEventType.Ticks);

        foreach (var item in outposts)
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
            colonyLocation.DoProduction,
            Tick.TimeEventType.Week);
    }

    #region Window
    public override void OpenWindow()
    {
        base.OpenWindow();
        map.Open(convoys);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        map.Close();
    }
    #endregion

    public void Trade(TradeConvoy convoy, Resource sellResource, int buyMoney)
    {
        convoys.Add(convoy);
        MyRes.PayCostGlobal(sellResource, buyMoney);
    }
}
