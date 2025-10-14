using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// First splash screen scene.
/// </summary>
public class SplashScreen : MonoBehaviour
{
    [NonSerialized] public bool loadNewGame = false;
    private async void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
        await SceneManager.LoadSceneAsync(1);

        await ResFluidTypes.Init();
        LoadingScreen screen = GameObject.Find("Loading Screen").GetComponent<LoadingScreen>();
        if (loadNewGame)
            screen.StartNewGame("test - TopGun");
        else
            screen.OpenMainMenu();
    }
}
