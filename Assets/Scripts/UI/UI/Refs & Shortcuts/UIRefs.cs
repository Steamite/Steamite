using UnityEngine;
using UnityEngine.UIElements;

public class UIRefs : MonoBehaviour
{
    static UIRefs instance;

    [SerializeField] CameraMovement _levelCamera;
    [SerializeField] Trading _trading;
    [SerializeField] ResearchWindow _research;
    [SerializeField] Menu _pauseMenu;
    [SerializeField] MonoBehaviour toolkitShotcuts;
    [SerializeField] UIDocument _bottomBar;

    public static CameraMovement levelCamera => instance._levelCamera;
    public static Trading trading => instance._trading;
    public static ResearchWindow research => instance._research;
    public static Menu pauseMenu => instance._pauseMenu;
    public static VisualElement buildBar => instance._bottomBar.rootVisualElement[0];
    public static UIDocument timeBar => instance._bottomBar;

    [RuntimeInitializeOnLoadMethod]
    static void ReloadDomain() => instance = null;
    public void Init()
    {
        instance = this;
        toolkitShotcuts.GetComponent<IToolkitController>().Init(_bottomBar.rootVisualElement);
    }
}
