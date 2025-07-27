using System.Collections.Generic;
using System.Linq;
using TradeData.Locations;
using UnityEngine;
using UnityEngine.UIElements;

public class TradingWindow : FullscreenWindow
{
    #region Const
    public const int CONVOY_STORAGE_LIMIT = 50;
    public const int CONVOY_SPEED = 10;
    public int maxConvoy = 3;
    #endregion

    #region Variables
    public IUIElement map;

    [SerializeField] List<TradeConvoy> convoys;

    public ColonyLocation colonyLocation;
    public List<TradeLocation> tradeLocations;
    public List<Outpost> outposts;

    [HideInInspector] public float distance; 
    [SerializeField] string baseLocation = "Highlands";

    public int outpostLimit = 3;
    #endregion

    #region Properties
    public int AvailableConvoy => maxConvoy - convoys.Count;
    public bool ConvoyOnRoute(int locationIndex) => convoys.Count(q => q.tradeLocation == locationIndex) == 1;
    public void RemoveConvoy(TradeConvoy convoy) => convoys.Remove(convoy);
    public List<TradeConvoy> GetConvoys() => convoys;
    #endregion

    public void LoadGame(TradeSave tradeSave)
    {
        TradeHolder tradeHolder = Resources.Load<TradeHolder>($"Holders/Data/Colony Locations/{tradeSave.colonyLocation}");
        colonyLocation = tradeHolder.startingLocation;
        colonyLocation.LoadGame(tradeSave.prodLevels, tradeSave.statLevels);

        tradeLocations = tradeSave.tradeLocations;
        convoys = tradeSave.convoys;
        outposts = tradeSave.outposts;
        SceneRefs.BottomBar.GetComponent<ResourceDisplay>().Money = tradeSave.money;
        Init();
    }

    public void Init()
    {
        GetWindow();
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        map = (IUIElement)root.Q<VisualElement>("Map");
        ((IInitiableUI)map).Init();
        ((IInitiableUI)root.Q<VisualElement>("Colony")).Init();


        //Moves all convoys each tick.
        SceneRefs.Tick.SubscribeToEvent(
            () =>
            {
                for (int i = convoys.Count - 1; i >= 0; i--)
                    convoys[i].Move(CONVOY_SPEED);
            },
           Tick.TimeEventType.Ticks);

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
    }
    #endregion

    public void Trade(TradeConvoy convoy, Resource sellResource, int buyMoney)
    {
        convoys.Add(convoy);
        MyRes.PayCostGlobal(sellResource, buyMoney);
    }
}
