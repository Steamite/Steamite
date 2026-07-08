using Outposts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeData.Locations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

[RequireComponent(typeof(Trading))]
public class TradingWindow : FullscreenWindow
{
    public VisualElement Map { get; private set; }

    VisualElement colonyView;

    Trading trading;

    private void Awake()
    {
        trading = GetComponent<Trading>();
        AddOnLoad(ref trading.OnLoad);
    }

    protected override void OnUIReload()
    {
        Map = Root.Q<VisualElement>("Map");
        colonyView = Root.Q<VisualElement>("Colony")[0];
        base.OnUIReload();
    }

    protected override void OnDataLoadLogic()
    {
        ((IInitiableUI)Map).Init();
        ((IInitiableUI<ColonyLocation>)colonyView).Init(trading.ColonyLocation);
    }


    #region Window
    public override void OpenWindow()
    {
        base.OpenWindow();
        ((IClosableElement)Map).Open(trading.Convoys);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        ((IClosableElement)Map).Close();
    }
    #endregion

}
