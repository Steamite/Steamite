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
    public static UIDocument BottomBar => instance._bottomBar;

    public void Init()
    {
        instance = this;
        toolkitShotcuts.GetComponent<IToolkitController>().Init(_bottomBar.rootVisualElement);
    }
}
