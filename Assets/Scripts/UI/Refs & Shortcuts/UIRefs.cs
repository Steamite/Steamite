using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class UIRefs : MonoBehaviour, IBeforeLoad
{
    static UIRefs instance;

    [SerializeField] CameraMovement _levelCamera;
    [SerializeField] Trading _trading;
    [SerializeField] TradingWindow _tradingWindow;
    [SerializeField] Research _research;
    [SerializeField] ResearchWindow _researchWindow;
    [SerializeField] FullscreenWindow _quests;
    [SerializeField] Menu _pauseMenu;
    [SerializeField] MonoBehaviour _toolkitShotcuts;
    [SerializeField] PanelRendererRoot _bottomBar;
    [SerializeField] PanelRendererRoot _topBar;

    [SerializeField] SaveDialog _saveDialog;
    [SerializeReference] MonoBehaviour _loadMenu;

    public static CameraMovement LevelCamera => instance._levelCamera;
    public static Trading Trading => instance._trading;
    public static TradingWindow TradingWindow => instance._tradingWindow;
    public static Research Research => instance._research;
    public static ResearchWindow ResearchWindow => instance._researchWindow;
    public static FullscreenWindow Quests => instance._quests;
    public static Menu PauseMenu => instance._pauseMenu;
    public static PanelRendererRoot BottomBar => instance._bottomBar;
    public static VisualElement BottomBarRoot => instance._bottomBar.Root;

    public static PanelRendererRoot TopBar => instance._topBar;
    public static VisualElement TopBarRoot => instance._topBar.Root;

    public static IToolkitController ToolkitShortcuts => instance._toolkitShotcuts.GetComponent<IToolkitController>();


    public static SaveDialog SaveDialog => instance._saveDialog;
    public static MonoBehaviour LoadMenu => instance._loadMenu;


    [RuntimeInitializeOnLoadMethod]
    static void ReloadDomain() => instance = null;


    public static bool WindowConstraint()
    {
        if (ConfirmWindow.window.opened)
            ConfirmWindow.window.Close(false);
        else if (instance._saveDialog.opened)
            instance._saveDialog.CloseWindow();
        else if (((IGridMenu)instance._loadMenu).IsOpen())
            ((IGridMenu)instance._loadMenu).CloseWindow();
        else if (instance._researchWindow.IsOpen)
            instance._researchWindow.CloseWindow();
        else if (instance._tradingWindow.IsOpen)
            instance._tradingWindow.CloseWindow();
        else if (instance._quests.IsOpen)
            instance._quests.CloseWindow();
        else if (SceneRefs.GridTiles.activeControl != (int)ControlMode.Nothing)
            SceneRefs.GridTiles.BreakAction();
        else
            return true;

        return false;
    }

    public static bool FullscreenConstraint()
    {
        if (instance._researchWindow.IsOpen)
            instance._researchWindow.CloseWindow();
        if (instance._tradingWindow.IsOpen)
            instance._tradingWindow.CloseWindow();
        if (instance._quests.IsOpen)
            instance._quests.CloseWindow();
        if (SceneRefs.GridTiles.activeControl != (int)ControlMode.Nothing)
            SceneRefs.GridTiles.BreakAction();

        return true;
    }

    public Task BeforeInit()
    {
        instance = this;
        return Task.CompletedTask;
    }
}
