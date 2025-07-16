using UnityEngine;
using UnityEngine.UIElements;

public class UIRefs : MonoBehaviour
{
    static UIRefs instance;

    [SerializeField] CameraMovement _levelCamera;
    [SerializeField] TradingWindow _trading;
    [SerializeField] ResearchWindow _research;
    [SerializeField] Menu _pauseMenu;
    [SerializeField] MonoBehaviour _toolkitShotcuts;
    [SerializeField] UIDocument _bottomBar;


    public static CameraMovement LevelCamera => instance._levelCamera;
    public static TradingWindow TradingWindow => instance._trading;
    public static ResearchWindow ResearchWindow => instance._research;
    public static Menu PauseMenu => instance._pauseMenu;
    public static VisualElement BottomBar => instance._bottomBar.rootVisualElement[0];
    public static UIDocument TimeDisplay => instance._bottomBar;
    public static IToolkitController ToolkitShortcuts => instance._toolkitShotcuts.GetComponent<IToolkitController>();


    [RuntimeInitializeOnLoadMethod]
    static void ReloadDomain() => instance = null;
    public void Init()
    {
        instance = this;

        ((IInitiableUI)BottomBar).Init();
    }
}
