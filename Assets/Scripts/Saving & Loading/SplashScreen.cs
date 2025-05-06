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

        LoadingScreen screen = GameObject.Find("Loading Screen").transform.GetChild(0).GetComponent<LoadingScreen>();
        if (loadNewGame)
            screen.StartNewGame("test - TopGun");
        else
            screen.OpenMainMenu();
    }
}
