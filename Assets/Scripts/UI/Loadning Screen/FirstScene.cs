using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstScene : MonoBehaviour
{
    [NonSerialized] public bool loadNewGame = false;
    private void Awake()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(1);
        load.completed += onLoad;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
    }
    void onLoad(AsyncOperation aO)
    {
        if (loadNewGame)
            GameObject.Find("Loading Screen").transform.GetChild(0).GetComponent<LoadingScreen>().NewGame("test - TopGun", false);
        else
            GameObject.Find("Loading Screen").transform.GetChild(0).GetComponent<LoadingScreen>().LoadMainMenu();
    }
}
