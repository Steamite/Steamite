using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstScene : MonoBehaviour
{
    private void Awake()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(1);
        load.completed += onLoad;
    }
    void onLoad(AsyncOperation aO)
    {
        // Fills all resource holders
        MyGrid.buildPrefabs = Resources.Load("Holders/Building Holder") as ResourceHolder;
        MyGrid.tilePrefabs = Resources.Load("Holders/Tile Holder") as ResourceHolder;
        MyGrid.specialPrefabs = Resources.Load("Holders/Special Holder") as ResourceHolder;
        GameObject.Find("Loading Screen").transform.GetChild(0).GetComponent<LoadingScreen>().LoadMainMenu();
    }
}
