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

    [SerializeField] public LoadActions loadAction = LoadActions.NewGameGen;
    private async void Awake()
    {

#if !UNITY_EDITOR
    loadAction = LoadActions.MainMenu;
#endif
        Time.timeScale = 1;
        await SceneManager.LoadSceneAsync("Loading Screen");

        await ResFluidTypes.Init();
        LoadingScreen screen = GameObject.Find("Loading Screen").GetComponent<LoadingScreen>();
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

#if UNITY_INCLUDE_TESTS
        afterLoad?.Invoke();
#endif
    }
}
