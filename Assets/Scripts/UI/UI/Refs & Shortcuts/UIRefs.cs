using System;
using UnityEngine;

public class UIRefs : MonoBehaviour
{
    static UIRefs instance;

    [SerializeField] CameraMovement _levelCamera;
    [SerializeField] Trading _trading;
    [SerializeField] ResearchUI _research;
    [SerializeField] Menu _pauseMenu;

    public static CameraMovement levelCamera => instance._levelCamera;
    public static Trading trading => instance._trading;
    public static ResearchUI research => instance._research;
    public static Menu pauseMenu => instance._pauseMenu;

    public void Init()
    {
        instance = this;
    }

    public static void SetTrade()
    {
        FindFirstObjectByType<UIRefs>().Init();
        instance._trading = FindFirstObjectByType<Trading>();
    }
}
