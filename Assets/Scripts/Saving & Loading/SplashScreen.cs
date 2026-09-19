using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LoadActions
{
    NewGame,
    MainMenu,
    LoadGame
}

/// <summary>
/// First splash screen scene.
/// </summary>
public class SplashScreen : MonoBehaviour
{
#if UNITY_INCLUDE_TESTS
    public Action afterLoad;
#endif

    [NonSerialized] public LoadActions loadAction = LoadActions.MainMenu;
    private async void Awake()
    {
        Time.timeScale = 1;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
        await SceneManager.LoadSceneAsync("Loading Screen");

        await ResFluidTypes.Init();
        LoadingScreen screen = GameObject.Find("Loading Screen").GetComponent<LoadingScreen>();
        switch (loadAction)
        {
            case LoadActions.NewGame:
                await screen.StartNewGame("test - TopGun");
                break;
            case LoadActions.MainMenu:
                await screen.OpenMainMenu(true);
                break;
            case LoadActions.LoadGame:
                await screen.OpenMainMenu(false);
                break;
        }

#if UNITY_INCLUDE_TESTS
        afterLoad?.Invoke();
#endif
    }
}
