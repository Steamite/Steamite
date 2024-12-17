using UnityEngine;

public class UIRefs : MonoBehaviour
{
    static UIRefs instance;

    [SerializeField] CameraMovement _levelCamera;
    [SerializeField] Trade _trade;
    [SerializeField] ResearchUI _research;
    [SerializeField] Menu _pauseMenu;

    public static CameraMovement levelCamera => instance._levelCamera;
    public static Trade trade => instance._trade;
    public static ResearchUI research => instance._research;
    public static Menu pauseMenu => instance._pauseMenu;

    public void Init()
    {
        instance = this;
    }
}
