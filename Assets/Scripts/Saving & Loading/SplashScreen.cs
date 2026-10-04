using Assets.Scripts.Saving___Loading;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LoadActions
{
    NewGame,
    MainMenu,
    LoadGame,
    NewGameGen
}

/// <summary>
/// First splash screen scene.
/// </summary>
public class SplashScreen : MonoBehaviour
{
#if UNITY_INCLUDE_TESTS
    public Action afterLoad;
#endif

#if UNITY_EDITOR
    public LoadActions loadAction = LoadActions.NewGameGen;
#endif
    private async void Awake()
    {
        Time.timeScale = 1;
        await SceneManager.LoadSceneAsync("Loading Screen");

        await ResFluidTypes.Init();
        LoadingScreen screen = GameObject.Find("Loading Screen").GetComponent<LoadingScreen>();

#if UNITY_EDITOR
        switch (loadAction)
        {
            case LoadActions.NewGame:
                await screen.StartNewGame("test - TopGun");
                break;
            case LoadActions.NewGameGen:
                await GameObject.Find("Loading Screen New").GetComponent<LoadingScreenNew>().StartNewGame("test - TopGun", "DA4131C9");
                break;
            case LoadActions.MainMenu:
                await screen.OpenMainMenu(true);
                break;
            case LoadActions.LoadGame:
                await screen.OpenMainMenu(false);
                break;
        }
#else
        await screen.OpenMainMenu(true);
#endif

#if UNITY_INCLUDE_TESTS
        afterLoad?.Invoke();
#endif
    }
}
