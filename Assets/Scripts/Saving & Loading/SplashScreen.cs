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
    [NonSerialized] public LoadActions loadAction = LoadActions.MainMenu;
    private async void Awake()
    {
        Time.timeScale = 1;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
        await SceneManager.LoadSceneAsync(1);

        await ResFluidTypes.Init();
        LoadingScreen screen = GameObject.Find("Loading Screen").GetComponent<LoadingScreen>();
        switch (loadAction)
        {
            case LoadActions.NewGame:
                screen.StartNewGame("test - TopGun");
                break;
            case LoadActions.MainMenu:
                screen.OpenMainMenu(true);
                break;
            case LoadActions.LoadGame:
                screen.OpenMainMenu(false);
                break;
        }
    }
}
