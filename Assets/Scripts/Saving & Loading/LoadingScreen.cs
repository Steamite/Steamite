using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Handles scene transitions.</summary>
public class LoadingScreen : MonoBehaviour
{
    #region Variables
    /// <summary>Save name.</summary>
    string folderName;

    const int BUILD_WEIGHT = 5;
    const int CHUNK_WEIGHT = 1;
    const int TILE_WEIGHT = 2;
    const int HUMAN_WEIGHT = 2;

    /// <summary>Load progress for the load bar.</summary>
    int progressGlobal = 0;

    /// <summary>Start normal Level.</summary>
    [SerializeField] GroundLevel startLevel;
    /// <summary>Start main Level.</summary>
    [SerializeField] GroundLevel mainLevel;
    /// <summary>Empty template level.</summary>
    [SerializeField] GroundLevel templateLevel;

    /// <summary>Text showing the task at hand.</summary>
    [SerializeField] TMP_Text actionText;
    /// <summary>Bar showing the loading progress.</summary>
    [SerializeField] Slider loadingSlider;
    /// <summary>Action that is triggered after loading everything(Let's humans listen to ticks).</summary>
    public event Action humanActivation;
    #endregion

    #region Scene Managment
    /// <summary>Loads the Main Menu scene.</summary>
    public void OpenMainMenu()
    {
        OpenScene(2); // the loading Process
    }

    /// <summary>Loads a new scene.</summary>
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
            //UIRefs.trade.NewGame();
            SceneRefs.humans.NewGameInit(ref humanActivation);
            AfterLevelLoad(true);
        }
        Debug.Log("end:" + Time.realtimeSinceStartup);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    /// <summary>
    /// Creates a new game save and Load it's scene.
    /// </summary>
    /// <param name="_folderName">Save name</param>
    /// <param name="seed">Generation seed</param>
    public async void NewGame(string _folderName, string seed = ""/*, bool tutorial*/)
    {
        folderName = _folderName;
        if (folderName != "test - TopGun")
            await SceneManager.UnloadSceneAsync("Main Menu");
        transform.GetChild(0).gameObject.SetActive(true);
        OpenScene(3, seed);
    }
    #endregion

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
    /// Preps <see cref="SceneRefs"/>, <see cref="SceneRefs"/> and loads saved data.
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
        // create progress val for loading slider
        int maxprogress = 0;
        for(int i = 0; i < worldSave.gridSave.Length; i++)
            maxprogress += worldSave.gridSave[i].width * worldSave.gridSave[i].height * TILE_WEIGHT; // Tiles and pipes
        maxprogress += worldSave.objectsSave.buildings.Length * BUILD_WEIGHT; //scale number
        maxprogress += worldSave.objectsSave.chunks.Length * CHUNK_WEIGHT;
        maxprogress += humanSaves.Length * HUMAN_WEIGHT;
        maxprogress += researchSave.categories.SelectMany(q => q.nodes).Count();
        loadingSlider.maxValue = maxprogress;
        IProgress<int> progress = new Progress<int>(value =>
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
                .CreateAChunk(chunkSave.gridPos, chunkSave.resSave.stored, true)
                .Load(chunkSave);
            progress.Report(progressGlobal += CHUNK_WEIGHT);
        }
    }

    void CreateBuildings(IProgress<int> progress, BSave[] buildings)
    {
        foreach (BSave save in buildings) // for each saved building
        {
            SceneRefs.objectFactory.CreateSavedBuilding(save);
            progress.Report(progressGlobal += BUILD_WEIGHT);
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
            progress.Report(progressGlobal += TILE_WEIGHT);
        }
        Camera.main.GetComponent<PhysicsRaycaster>().eventMask = SceneRefs.gridTiles.defaultMask;
    }
    void FillHumans(IProgress<int> progress, HumanSave[] humanSaves)
    {
        actionText.text = "Kidnaping workers";
        SceneRefs.humans.LoadHumans(progress, humanSaves, ref humanActivation, HUMAN_WEIGHT, ref progressGlobal);
    }
    #endregion Model Loading

    #region UI loading

    void FillGameState(IProgress<int> progress, GameStateSave gameState)
    {
        SceneRefs.jobQueue.priority = gameState.priorities;
        SceneRefs.tick.timeController.Load(gameState);
    }

    void FillResearches(IProgress<int> progress, ResearchSave researchSave)
    {
        actionText.text = "Remembering research";
        UIRefs.research.LoadGame(researchSave);
    }

    void FillTrade(IProgress<int> progress, TradeSave tradeSave)
    {
        actionText.text = "Making Deals";
        UIRefs.trade.LoadGame(tradeSave);
    }
    #endregion UI loading
    #endregion Loading Game State

    /// <summary>
    /// Things that need to be after both loading and creating a new game.
    /// </summary>
    /// <param name="newGame">Is it the new game(used for calling the same methods just with different paramaters).</param>
    async void AfterLevelLoad(bool newGame)
    {
        //transform.parent.GetChild(1).GetComponent<AudioListener>().enabled = false;
        SceneRefs.miscellaneous.GetChild(1).GetComponent<LocalInfoWindow>().SetUp();
        MyGrid.worldName = folderName;
        MyGrid.Init();
        Camera.main.GetComponent<PhysicsRaycaster>().eventMask = SceneRefs.gridTiles.defaultMask;
        Camera.main.GetComponent<PhysicsRaycaster>().enabled = true;
        Camera.main.GetComponent<Physics2DRaycaster>().enabled = true;
        Camera.main.GetComponent<AudioListener>().enabled = true;

        MyRes.ActivateResources(newGame);
        await SceneManager.UnloadSceneAsync("LoadingScreen");
        SceneRefs.stats.GetChild(1).GetChild(0).GetComponent<TimeButtons>().SetStartSpeed(SceneRefs.tick);
        SceneRefs.stats.GetChild(0).GetComponent<LevelButtons>().Init();
        humanActivation?.Invoke();
        humanActivation = null;
        SceneRefs.tick.timeController.Init(SceneRefs.tick, newGame);
    }
}
