using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    string folderName;
    int buildWeigth = 5;
    int progressGlobal = 1;

    [SerializeField] GroundLevel startLevel;
    [SerializeField] GroundLevel mainLevel;
    [SerializeField] GroundLevel templateLevel;

    [SerializeField] TMP_Text actionText;
    [SerializeField] Slider loadingSlider;
    public event Action humanActivation;

    GameObject roadPref;
    public void OpenMainMenu()
    {
        OpenScene(2); // the loading Process
    }

    async void OpenScene(int id, string seed = "")
    {
        await SceneManager.LoadSceneAsync(id, LoadSceneMode.Additive);
        
        Debug.Log("start:" + Time.realtimeSinceStartup);
        if(id == 3) // if loading level
        {
            GameObject.Find("UI canvas").GetComponent<UIRefs>().Init();
            if (seed != "")
            {
                gameObject.GetComponent<MapGen>().Generate(seed, templateLevel);
            }
            else
                MyGrid.CreateGrid(startLevel, mainLevel);
            UIRefs.research.NewGame();
            UIRefs.trade.NewGame();
            SceneRefs.humans.NewGameInit(ref humanActivation);
            AfterLevelLoad(true);
        }
        Debug.Log("end:" + Time.realtimeSinceStartup);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public async void NewGame(string _folderName, string seed = ""/*, bool tutorial*/)
    {
        folderName = _folderName;
        if (folderName != "test - TopGun")
            await SceneManager.UnloadSceneAsync("Main Menu");
        transform.GetChild(0).gameObject.SetActive(true);
        OpenScene(3, seed);
    }

    #region Loading Game State
    /// <summary>
    /// Assigns parameters and shows loading screen.<br/>
    /// If called while on "Main Menu" scene unloads it.<br/>
    /// After that loads "Level" scene.
    /// </summary>
    /// <param name="_folderName">Current folder name.</param>
    public async void StartLoading(Save save)
    {
        folderName = save.worldName;

        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = folderName;
        transform.GetChild(0).GetChild(2).gameObject.SetActive(true);

        if (GameObject.Find("Main Menu"))
        {
           await SceneManager.UnloadSceneAsync("Main Menu");
        }
        await SceneManager.LoadSceneAsync("Level", LoadSceneMode.Additive);
        await LoadWorldData(save);
        AfterLevelLoad(false);
    }

    /// <summary>
    /// Preps <see cref="SceneRefs"/>, <see cref="CanvasManager"/> and loads saved data.
    /// </summary>
    /// <param name="obj"></param>
    Task LoadWorldData(Save save)
    {
        MyGrid.worldName = save.worldName;
        WorldSave worldSave = save.world;
        GameStateSave gameState = save.gameState;
        HumanSave[] humanSaves = save.humans;
        ResearchSave researchSave = save.research;
        TradeSave tradeSave = save.trade;

        GameObject.Find("UI canvas").GetComponent<UIRefs>().Init();
        // create Progress val for loading slider
        int maxProgress = 0;
        for(int i = 0; i < worldSave.gridSave.Length; i++)
            maxProgress += worldSave.gridSave[i].width * worldSave.gridSave[i].height * 2; // Tiles and pipes
        maxProgress += worldSave.objectsSave.buildings.Length * buildWeigth; //scale number
        maxProgress += worldSave.objectsSave.chunks.Length;
        maxProgress += humanSaves.Length;
        maxProgress += researchSave.categories.SelectMany(q => q.nodes).Count();
        loadingSlider.maxValue = maxProgress;
        Progress<int> progress = new Progress<int>(value =>
        {
            loadingSlider.value = value;
        });
        FillGrid(progress, worldSave);
        FillGameState(progress, gameState);
        FillHumans(progress, humanSaves);
        FillResearches(progress, researchSave);
        FillTrade(progress, tradeSave);


        return Task.CompletedTask;
    }


    #region Model Loading
    void FillGrid(IProgress<int> progress, WorldSave worldSave)
    {
        actionText.text = "Loading grid";
        CreateGrid(progress, worldSave.gridSave);
        actionText.text = "Spawning chunks";
        CreateChunks(progress, worldSave.objectsSave.chunks);
        actionText.text = "Constructing buildings";
        CreateBuildings(progress, worldSave.objectsSave.buildings);
        
    }

    void CreateChunks(IProgress<int> progress, ChunkSave[] chunks)
    {
        foreach (ChunkSave chunkSave in chunks)
        {
            SceneRefs.objectFactory
                .CreateAChunk(chunkSave.gridPos, chunkSave.resSave.stored)
                .Load(chunkSave);
            progress.Report(progressGlobal += 1);
        }
    }

    void CreateBuildings(IProgress<int> progress, BSave[] buildings)
    {
        foreach (BSave save in buildings) // for each saved building
        {
            SceneRefs.objectFactory.CreateSavedBuilding(save);
            progress.Report(progressGlobal += buildWeigth);
        }
    }

    void CreateGrid(IProgress<int> progress, GridSave[] gridSave)
    {
        // Empties grid
        MyGrid.PrepGridLists();
        // creates an empty ground level
        for(int i = 0; i < gridSave.Length; i++)
        {
            MyGrid.Load(gridSave[i], templateLevel, i);
            //progress.Report(progressGlobal += );
        }
        Camera.main.GetComponent<PhysicsRaycaster>().eventMask = SceneRefs.gridTiles.defaultMask;
    }
    void FillHumans(IProgress<int> progress, HumanSave[] humanSaves)
    {
        actionText.text = "Kidnaping workers";
        SceneRefs.humans.LoadHumans(progress, humanSaves, ref humanActivation);
    }
    #endregion Model Loading

    #region UI loading

    void FillGameState(IProgress<int> progress, GameStateSave gameState)
    {
        SceneRefs.humans.GetComponent<JobQueue>().priority = gameState.priorities;
        SceneRefs.tick.timeController.Load(gameState);
    }

    void FillResearches(Progress<int> progress, ResearchSave researchSave)
    {
        actionText.text = "Remembering research";
        UIRefs.research.LoadGame(researchSave);
    }

    void FillTrade(Progress<int> progress, TradeSave tradeSave)
    {
        actionText.text = "Making Deals";
        UIRefs.trade.LoadGame(tradeSave);
    }
    #endregion UI loading
    #endregion Loading Game State
    async void AfterLevelLoad(bool newGame)
    {
        //transform.parent.GetChild(1).GetComponent<AudioListener>().enabled = false;
        CanvasManager.miscellaneous.GetChild(1).GetComponent<LocalInfoWindow>().SetUp();
        MyGrid.worldName = folderName;
        MyGrid.Init();
        Camera.main.GetComponent<PhysicsRaycaster>().eventMask = SceneRefs.gridTiles.defaultMask;
        Camera.main.GetComponent<PhysicsRaycaster>().enabled = true;
        Camera.main.GetComponent<Physics2DRaycaster>().enabled = true;
        Camera.main.GetComponent<AudioListener>().enabled = true;

        MyRes.ActivateResources(newGame);
        await SceneManager.UnloadSceneAsync("LoadingScreen");
        CanvasManager.stats.GetChild(0).GetChild(0).GetComponent<TimeButtons>().SetStartSpeed(SceneRefs.tick);
        humanActivation?.Invoke();
        SceneRefs.tick.timeController.Init(SceneRefs.tick, newGame);
        humanActivation = null;
    }
}
