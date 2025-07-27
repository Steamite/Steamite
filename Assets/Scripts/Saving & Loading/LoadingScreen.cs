using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
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
    const int RES_WEIGHT = 2;

    /// <summary>Load progress for the load bar.</summary>
    int progressGlobal = 0;

    [SerializeField] List<GroundLevel> testLevels;
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


    public async void StartNewGame(string _folderName, string seed = "")
    {
        NewGameInit newGameInit = gameObject.GetComponent<NewGameInit>();
        WorldSave save;
        int size;
        if (seed != "")
        {
            size = gameObject.GetComponent<MapGen>().Generate(seed, out save);
        }
        else
        {
            size = testLevels[0].height;
            newGameInit.CreateGrid(testLevels, out save);
        }

        ResearchSave researchSave = await newGameInit.InitResearch();
        StartCoroutine(StartLoading(_folderName, _folderName,
            new Save()
            {
                gameState = newGameInit.SetNewGameState(),
                trade = newGameInit.CreateTrade(0),
                research = researchSave,
                humans = newGameInit.InitHumans(size),
                world = save
            }));
    }
    #endregion

    #region Loading Game State
    public void LoadGame(string _folderName, string _worldName)
    {
        StartCoroutine(StartLoading(_folderName, _worldName, LoadSavedData(_folderName)));
    }
    /// <summary>
    /// Assigns parameters and shows loading screen.<br/>
    /// If called while on "Main Menu" scene unloads it.<br/>
    /// After that loads "Level" scene.
    /// </summary>
    /// <param name="_folderName">Current folder name.</param>
    IEnumerator StartLoading(string _folderName, string _worldName, Save save)
    {
        worldName = _worldName;
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = _folderName;
        transform.GetChild(0).GetChild(2).gameObject.SetActive(true);

        yield return BeforeLevelLoad();
        yield return LoadWorldData(save);

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
        jsonReader = new(new StreamReader($"{_folderName}/Game State.json"));
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
    IEnumerator LoadWorldData(Save save)
    {
        MyGrid.worldName = worldName;
        WorldSave worldSave = save.world;
        GameStateSave gameState = save.gameState;
        HumanSave[] humanSaves = save.humans;
        ResearchSave researchSave = save.research;
        TradeSave tradeSave = save.trade;

        // create progress val for loading slider
        int maxprogress = 0;
        for (int i = 0; i < worldSave.gridSave.Length; i++)
            maxprogress += worldSave.gridSave[i].width * worldSave.gridSave[i].height * TILE_WEIGHT; // Tiles and pipes
        maxprogress += worldSave.objectsSave.buildings.Length * BUILD_WEIGHT; //scale number
        maxprogress += worldSave.objectsSave.chunks.Length * CHUNK_WEIGHT;
        maxprogress += humanSaves.Length * HUMAN_WEIGHT;
        maxprogress += researchSave.count * RES_WEIGHT;
        loadingSlider.maxValue = maxprogress;
        IProgress<int> progress = new Progress<int>(value =>
        {
            loadingSlider.value = value;
        });
        FillGrid(progress, worldSave);
        yield return new();
        FillGameState(progress, gameState);
        yield return new();
        FillHumans(progress, humanSaves);
        yield return new();
        FillResearches(progress, researchSave);
        yield return new();
        FillTrade(progress, tradeSave);
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
        LoadGrid(progress, worldSave.gridSave);
        actionText.text = "Filling Veins";
        LoadVeins(progress, worldSave.objectsSave.veins);
        actionText.text = "Spawning chunks";
        LoadChunks(progress, worldSave.objectsSave.chunks);
        actionText.text = "Constructing buildings";
        LoadBuildings(progress, worldSave.objectsSave.buildings);
    }

    /// <summary>
    /// Creates chunks.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="chunks"></param>
    void LoadChunks(IProgress<int> progress, ChunkSave[] chunks)
    {
        foreach (ChunkSave chunkSave in chunks)
        {
            SceneRefs.ObjectFactory
                .CreateChunk(chunkSave.gridPos, chunkSave.resSave, true)
                .Load(chunkSave);
            progress.Report(progressGlobal += CHUNK_WEIGHT);
        }
    }

    /// <summary>
    /// Creates Buildings.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="buildings"></param>
    void LoadBuildings(IProgress<int> progress, BuildingSave[] buildings)
    {
        foreach (BuildingSave save in buildings) // for each saved building
        {
            SceneRefs.ObjectFactory.CreateSavedBuilding(save);
            progress.Report(progressGlobal += BUILD_WEIGHT);
        }
    }

    void LoadVeins(IProgress<int> progress, VeinSave[] veins)
    {
        foreach (VeinSave save in veins) // for each saved building
        {
            SceneRefs.ObjectFactory.CreateSavedVein(save);
            progress.Report(progressGlobal += BUILD_WEIGHT);
        }
    }

    /// <summary>
    /// Recreates the tiles of the grid.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="gridSave"></param>
    void LoadGrid(IProgress<int> progress, GridSave[] gridSave)
    {
        // Empties grid
        MyGrid.PrepGridLists();
        // creates an empty ground level

        MapGen mapGen = gameObject.GetComponent<MapGen>();
        for (int i = 0; i < gridSave.Length; i++)
        {
            MyGrid.Load(gridSave[i], templateLevel, i, mapGen.minableResources, mapGen.dirt);
            progress.Report(progressGlobal += TILE_WEIGHT);
        }
    }

    /// <summary>
    /// Loads humans.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="humanSaves"></param>
    void FillHumans(IProgress<int> progress, HumanSave[] humanSaves)
    {
        actionText.text = "Kidnaping workers";
        SceneRefs.Humans.LoadHumans(progress, humanSaves, ref humanActivation, HUMAN_WEIGHT, ref progressGlobal);
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
        SceneRefs.JobQueue.priority = gameState.priorities;
        SceneRefs.Tick.Load(gameState);
    }

    /// <summary>
    /// Loads Research.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="researchSave"></param>
    void FillResearches(IProgress<int> progress, ResearchSave researchSave)
    {
        actionText.text = "Remembering research";
        UIRefs.ResearchWindow.LoadGame(researchSave);
    }

    /// <summary>
    /// Loads Trading.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="tradeSave"></param>
    void FillTrade(IProgress<int> progress, TradeSave tradeSave)
    {
        actionText.text = "Making Deals";
        UIRefs.TradingWindow.LoadGame(tradeSave);
    }
    #endregion UI loading
    #endregion Loading Game State

    IEnumerator BeforeLevelLoad()
    {
        if (GameObject.Find("Main Menu"))
            yield return SceneManager.UnloadSceneAsync("Main Menu");
        yield return SceneManager.LoadSceneAsync("Level", LoadSceneMode.Additive);
        yield return GameObject.Find("Scene").GetComponent<SceneRefs>().BeforeLoad();
        GameObject.Find("UI canvas").GetComponent<UIRefs>().Init();
        MyRes.PreLoad();
    }

    /// <summary>
    /// Things that need to be after both loading and creating a new game.
    /// </summary>
    /// <param name="newGame">Is it the new game(used for calling the same methods just with different paramaters).</param>
    void AfterLevelLoad(bool newGame)
    {
        MyGrid.worldName = worldName;

        MyRes.ActivateResources();

        actionText.text = "Press any button";
        InputSystem.onAnyButtonPress.CallOnce(
            (action) => StartCoroutine(CancelInput()));
    }

    /// <summary>
    /// Wait one frame to forget the pressed value.
    /// </summary>
    /// <param name="newGame"></param>
    /// <returns></returns>
    IEnumerator CancelInput()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return null;
        }
        EnableLevelContollers();
    }

    /// <summary>
    /// Unloads the loading screen. 
    /// And enables user input.
    /// </summary>
    /// <param name="newGame">Information for ticks</param>
    async void EnableLevelContollers()
    {
        StopCoroutine(CancelInput());
        for (int i = SceneManager.sceneCount - 1; i > -1; i--)
        {
            if (SceneManager.GetSceneAt(i).name == "LoadingScreen" && SceneManager.GetSceneAt(i).isLoaded)
                await SceneManager.UnloadSceneAsync("LoadingScreen");
        }

        SceneRefs.FinishLoad();

        humanActivation?.Invoke();
        humanActivation = null;
        SceneRefs.Tick.InitTicks();
        UIRefs.TimeDisplay.GetComponent<IToolkitController>().Init(UIRefs.TimeDisplay.rootVisualElement);
        UIRefs.ToolkitShortcuts.Init(UIRefs.TimeDisplay.rootVisualElement);

    }
}
