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
        TradeHolder tradeHolder = Resources.Load<TradeHolder>("Holders/Data/Trade Data");
        colonyLocation = tradeHolder.startingLocations[selectedColony];
        tradeLocations = tradeHolder.tradeLocations;

        convoys = new();
        Init();
    }

    public void LoadGame(TradeSave tradeSave)
    {
        convoys = tradeSave.convoys;
        colonyLocation = tradeSave.colonyLocation;
        tradeLocations = tradeSave.tradeLocations;
        Init();
    }

    public void Init()
    {
        GetWindow();
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        map = (IUIElement)root.Q<VisualElement>("Map");
        ((IInitiableUI)map).Init();
        ((IInitiableUI)root.Q<VisualElement>("ColonyView")).Init();


        //Moves all convoys each tick.
        SceneRefs.tick.tickAction += 
            () =>
            {
                for (int i = convoys.Count - 1; i >= 0; i--)
                    convoys[i].Move(CONVOY_SPEED);
            };
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        map.Open(convoys);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
    }

    public void Trade(TradeConvoy convoy, Resource sellResource, int buyMoney)
    {
        convoys.Add(convoy);
        MyRes.TakeFromGlobalStorage(sellResource);
        MyRes.UpdateMoney(buyMoney);
    }

}
