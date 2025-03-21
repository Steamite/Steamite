using Newtonsoft.Json;
using System;
using System.IO;
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
    string worldName;

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
    public async void OpenMainMenu()
    {
        await SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    /// <summary>
    /// Creates a new game save and Load it's scene.
    /// </summary>
    /// <param name="_folderName">Save name</param>
    /// <param name="seed">Generation seed</param>
    public async void NewGame(string _folderName, string seed = ""/*, bool tutorial*/)
    {
        worldName = _folderName;
        if (worldName != "test - TopGun")
            await SceneManager.UnloadSceneAsync("Main Menu");
        transform.GetChild(0).gameObject.SetActive(true);

        await SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive);
        GameObject.Find("UI canvas").GetComponent<UIRefs>().Init();
        if (seed != "")
        {
            gameObject.GetComponent<MapGen>().Generate(seed, templateLevel);
        }
        else
            MyGrid.CreateGrid(startLevel, mainLevel);
        UIRefs.research.NewGame();
        UIRefs.trading.NewGame(0);
        SceneRefs.humans.NewGameInit(ref humanActivation);
        AfterLevelLoad(true);
    }
    #endregion

    #region Loading Game State
    /// <summary>
    /// Assigns parameters and shows loading screen.<br/>
    /// If called while on "Main Menu" scene unloads it.<br/>
    /// After that loads "Level" scene.
    /// </summary>
    /// <param name="_folderName">Current folder name.</param>
    public async void StartLoading(string _folderName, string _worldName)
    {
        Save save = LoadSavedData(_folderName);
        worldName = _worldName;

        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = _folderName;
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
    /// Fills the save struct with the data from json.
    /// </summary>
    /// <param name="_folderName">Path to the save folder.</param>
    /// <returns></returns>
    Save LoadSavedData(string _folderName)
    {
        Save save = new();

        JsonSerializer jsonSerializer = SaveController.PrepSerializer();
        // for gridSave
        save.world = new();
        JsonTextReader jsonReader = new(new StreamReader($"{_folderName}/Grid.json"));
        save.world.objectsSave = jsonSerializer.Deserialize<BuildsAndChunksSave>(jsonReader);
        jsonReader.Close();
        // for all levels
        save.world.gridSave = new GridSave[MyGrid.NUMBER_OF_LEVELS];
        for (int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
        {
            jsonReader = new(new StreamReader($"{_folderName}/Level{i}.json"));
            save.world.gridSave[i] = jsonSerializer.Deserialize<GridSave>(jsonReader);
            jsonReader.Close();
        }
        // for playerSettings
        jsonReader = new(new StreamReader($"{_folderName}/PlayerSettings.json"));
        save.gameState = jsonSerializer.Deserialize<GameStateSave>(jsonReader);
        jsonReader.Close();
        // for humanSaves
        jsonReader = new(new StreamReader($"{_folderName}/Humans.json"));
        save.humans = jsonSerializer.Deserialize<HumanSave[]>(jsonReader);
        jsonReader.Close();
        // for researchCategories
        jsonReader = new(new StreamReader($"{_folderName}/Research.json"));
        save.research = jsonSerializer.Deserialize<ResearchSave>(jsonReader);
        jsonReader.Close();
        // for trading
        jsonReader = new(new StreamReader($"{_folderName}/Trade.json"));
        save.trade = jsonSerializer.Deserialize<TradeSave>(jsonReader);
        jsonReader.Close();
        return save;
    }



    /// <summary>
    /// Preps <see cref="SceneRefs"/>, <see cref="SceneRefs"/> and loads saved data.
    /// </summary>
    /// <param name="obj"></param>
    Task LoadWorldData(Save save)
    {
        MyGrid.worldName = worldName;
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
    /// <summary>
    /// Loads grid objects.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="worldSave"></param>
    void FillGrid(IProgress<int> progress, WorldSave worldSave)
    {
        actionText.text = "Loading grid";
        CreateGrid(progress, worldSave.gridSave);
        actionText.text = "Spawning chunks";
        CreateChunks(progress, worldSave.objectsSave.chunks);
        actionText.text = "Constructing buildings";
        CreateBuildings(progress, worldSave.objectsSave.buildings);
        
    }
    
    /// <summary>
    /// Creates chunks.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="chunks"></param>
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

    /// <summary>
    /// Creates Buildings.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="buildings"></param>
    void CreateBuildings(IProgress<int> progress, BSave[] buildings)
    {
        foreach (BSave save in buildings) // for each saved building
        {
            SceneRefs.objectFactory.CreateSavedBuilding(save);
            progress.Report(progressGlobal += BUILD_WEIGHT);
        }
    }

    /// <summary>
    /// Recreates the tiles of the grid.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="gridSave"></param>
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

    /// <summary>
    /// Loads humans.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="humanSaves"></param>
    void FillHumans(IProgress<int> progress, HumanSave[] humanSaves)
    {
        actionText.text = "Kidnaping workers";
        SceneRefs.humans.LoadHumans(progress, humanSaves, ref humanActivation, HUMAN_WEIGHT, ref progressGlobal);
    }
    #endregion Model Loading

    #region UI loading

    /// <summary>
    /// Loads player preferences.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="gameState"></param>
    void FillGameState(IProgress<int> progress, GameStateSave gameState)
    {
        SceneRefs.jobQueue.priority = gameState.priorities;
        SceneRefs.tick.timeController.Load(gameState);
    }

    /// <summary>
    /// Loads Research.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="researchSave"></param>
    void FillResearches(IProgress<int> progress, ResearchSave researchSave)
    {
        actionText.text = "Remembering research";
        UIRefs.research.LoadGame(researchSave);
    }

    /// <summary>
    /// Loads Trading.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="tradeSave"></param>
    void FillTrade(IProgress<int> progress, TradeSave tradeSave)
    {
        actionText.text = "Making Deals";
        UIRefs.trading.LoadGame(tradeSave);
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
        MyGrid.worldName = worldName;
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
