using System;
using System.Collections.Generic;
using System.Linq;
using TradeData.Locations;
using UnityEngine;
using UnityEngine.UIElements;

public class Trading : FullscreenWindow
{
    #region Const
    public const int CONVOY_STORAGE_LIMIT = 50;
    const int CONVOY_SPEED = 10;
    public int maxConvoy = 3;
    #endregion

    #region Variables
    public IUIElement map;

    [SerializeField] List<TradeConvoy> convoys;

    public ColonyLocation colonyLocation;
    public List<TradeLocation> tradeLocations;
    #endregion

    #region Properties
    public int AvailableConvoy => maxConvoy - convoys.Count;
    public bool ConvoyOnRoute(int locationIndex) => convoys.Count(q => q.tradeLocation == locationIndex) == 1;
    public void RemoveConvoy(TradeConvoy convoy) => convoys.Remove(convoy);
    public List<TradeConvoy> GetConvoys() => convoys;
    #endregion

    public void NewGame(int selectedColony)
    {
        TradeHolder tradeHolder = Resources.Load<TradeHolder>("Holders/Data/Trade Locations/Snowlandia");
        colonyLocation = tradeHolder.startingLocation;
        tradeLocations = tradeHolder.tradeLocations;

        convoys = new();
        Init();
    }

    public void LoadGame(TradeSave tradeSave)
    {
		TradeHolder tradeHolder = Resources.Load<TradeHolder>($"Holders/Data/Trade Locations/{tradeSave.colonyLocation}");
		colonyLocation = tradeHolder.startingLocation;

        for (int i = 0; i < colonyLocation.passiveProductions.Count; i++)
            colonyLocation.passiveProductions[i].LoadState(tradeSave.prodLevels[i]);
        for (int i = 0; i < colonyLocation.stats.Count; i++)
			colonyLocation.stats[i].LoadState(tradeSave.statLevels[i]);

        tradeLocations = tradeSave.tradeLocations;
        convoys = tradeSave.convoys;
        SceneRefs.stats.GetComponent<ResourceDisplay>().Money = tradeSave.money;
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
        SceneRefs.tick.SubscribeToTicks( 
            () =>
            {
                for (int i = convoys.Count - 1; i >= 0; i--)
                    convoys[i].Move(CONVOY_SPEED);
            });
        SceneRefs.tick.timeController.SubscribeToEvent(colonyLocation.DoProduction, DayTime.TimeEventType.Week);
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
        MyRes.TakeFromGlobalStorage(sellResource);
        MyRes.UpdateMoney(buyMoney);
    }

}
