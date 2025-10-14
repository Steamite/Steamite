using UnityEngine;
using UnityEngine.UIElements;

public class UIRefs : MonoBehaviour
{
    static UIRefs instance;

    [SerializeField] CameraMovement _levelCamera;
    [SerializeField] TradingWindow _trading;
    [SerializeField] ResearchWindow _research;
    [SerializeField] FullscreenWindow _quests;
    [SerializeField] Menu _pauseMenu;
    [SerializeField] MonoBehaviour _toolkitShotcuts;
    [SerializeField] UIDocument _bottomBar;

    [SerializeField] SaveDialog _saveDialog;
    [SerializeReference] MonoBehaviour _loadMenu;

    public static CameraMovement LevelCamera => instance._levelCamera;
    public static TradingWindow TradingWindow => instance._trading;
    public static ResearchWindow ResearchWindow => instance._research;
    public static FullscreenWindow Quests => instance._quests;
    public static Menu PauseMenu => instance._pauseMenu;
    public static VisualElement BottomBar => instance._bottomBar.rootVisualElement[0];
    public static UIDocument TimeDisplay => instance._bottomBar;
    public static IToolkitController ToolkitShortcuts => instance._toolkitShotcuts.GetComponent<IToolkitController>();

    public static SaveDialog SaveDialog => instance._saveDialog;
    public static MonoBehaviour LoadMenu => instance._loadMenu;


    [RuntimeInitializeOnLoadMethod]
    static void ReloadDomain() => instance = null;
    public void Init()
    {
        instance = this;

        ((IInitiableUI)BottomBar).Init();
    }

    public static bool WindowConstraint()
    {
        if (ConfirmWindow.window.opened)
            ConfirmWindow.window.Close(false);
        else if (instance._saveDialog.opened)
            instance._saveDialog.CloseWindow();
        else if (((IGridMenu)instance._loadMenu).IsOpen())
            ((IGridMenu)instance._loadMenu).CloseWindow();
        else if (instance._research.isOpen)
            instance._research.CloseWindow();
        else if (instance._trading.isOpen)
            instance._trading.CloseWindow();
        else if (instance._quests.isOpen)
            instance._quests.CloseWindow();
        else if (SceneRefs.GridTiles.activeControl != ControlMode.Nothing)
            SceneRefs.GridTiles.BreakAction();
        else
            return true;

        return false;
    }

    public static bool FullscreenConstraint()
    {
        if (instance._research.isOpen)
            instance._research.CloseWindow();
        if (instance._trading.isOpen)
            instance._trading.CloseWindow();
        if (instance._quests.isOpen)
            instance._quests.CloseWindow();
        if (SceneRefs.GridTiles.activeControl != ControlMode.Nothing)
            SceneRefs.GridTiles.BreakAction();

        return true;
    }
}
