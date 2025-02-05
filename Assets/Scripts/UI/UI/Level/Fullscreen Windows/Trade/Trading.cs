using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Trading : FullscreenWindow
{
    #region Variables
    IUIElement map;
    [SerializeField] List<TradeConvoy> convoys;
    [SerializeField] TradeHolder tradeHolder;
    int selectedColony = 0;

    public int maxConvoy = 3;
    public const int CONVOY_STORAGE_LIMIT = 50;
    const int CONVOY_SPEED = 10;
    #endregion

    #region Properties
    public int AvailableConvoy => maxConvoy - convoys.Count;
    public bool ConvoyOnRoute(int locationIndex) => convoys.Count(q => q.tradeLocation == locationIndex) == 1;
    public ColonyLocation colonyLocation => tradeHolder.startingLocations[selectedColony];
    public List<TradeLocation> tradeLocations => tradeHolder.tradeLocations;
    public void RemoveConvoy(TradeConvoy convoy) => convoys.Remove(convoy);
    #endregion

    public void NewGame()
    {
        convoys = new();
        Init();
    }

    public void LoadGame(TradeSave tradeSave)
    {
        throw new NotImplementedException();
    }

    public void Init()
    {
        GetWindow();
        map = (IUIElement)GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Map");
        ((IToolkitController)map).Init(GetComponent<UIDocument>().rootVisualElement);

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
        MyRes.ManageMoney(buyMoney);
    }

}
