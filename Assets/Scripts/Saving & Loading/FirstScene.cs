using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstScene : MonoBehaviour
{
    [NonSerialized] public bool loadNewGame = false;
    private async void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
        await SceneManager.LoadSceneAsync(1);
        
        if (loadNewGame)
            GameObject.Find("Loading Screen").transform.GetChild(0).GetComponent<LoadingScreen>().NewGame("test - TopGun");
        else
            GameObject.Find("Loading Screen").transform.GetChild(0).GetComponent<LoadingScreen>().OpenMainMenu();
    }
}
