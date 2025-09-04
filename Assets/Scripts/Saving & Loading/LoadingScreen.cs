using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>Handles scene transitions.</summary>
public class LoadingScreen : MonoBehaviour, IUpdatable
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
    int _progress = 0;
    [CreateProperty]
    int ProgressGlobal
    {
        get => _progress;
        set
        {
            _progress = value; 
            UIUpdate(nameof(ProgressGlobal));
        }
    }

    string _actionText = "Loading";
    [CreateProperty]
    string ActionText
    {
        get => _actionText;
        set
        {
            _actionText = value;
            UIUpdate(nameof(ActionText));
        }
    }


    [SerializeField] List<GroundLevel> testLevels;
    /// <summary>Empty template level.</summary>
    [SerializeField] GroundLevel templateLevel;

    /// <summary>Action that is triggered after loading everything(Let's humans listen to ticks).</summary>
    public event Action humanActivation;
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
    #endregion

    #region Scene Managment
    /// <summary>Loads the Main Menu scene.</summary>
    public async void OpenMainMenu()
    {
        await SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
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

        await StartLoading(_folderName, _folderName,
            new Save()
            {
                gameState = newGameInit.SetNewGameState(),
                trade = await newGameInit.InitTrade(0),
                research = await newGameInit.InitResearch(),
                humans = newGameInit.InitHumans(size),
                quests = await newGameInit.InitQuests(),
                world = save
            });
    }
    #endregion

    #region Loading Game State
    public async Task LoadGame(string _folderName, string _worldName)
    {
        await StartLoading(_folderName, _worldName, LoadSavedData(_folderName));
    }
    /// <summary>
    /// Assigns parameters and shows loading screen.<br/>
    /// If called while on "Main Menu" scene unloads it.<br/>
    /// After that loads "Level" scene.
    /// </summary>
    /// <param name="_folderName">Current folder name.</param>
    async Task StartLoading(string _folderName, string _worldName, Save save)
    {
        worldName = _worldName;
        VisualElement loadingScreenWindow = transform.GetComponent<UIDocument>().rootVisualElement;
        loadingScreenWindow[0].style.display = DisplayStyle.Flex;

        Label actionText = loadingScreenWindow.Q<Label>("Title");
        ActionText = "Loading";
        actionText.SetBinding(nameof(ActionText), "text", this);

        ProgressBar progressBar = loadingScreenWindow.Q<ProgressBar>();
        progressBar.value = 0;
        progressBar.SetBinding(
            nameof(ProgressGlobal), 
            "value", 
            (ref int i) => 
            {
                return (float)i;
            }, 
            this);

        await BeforeLevelLoad();
        await LoadWorldData(save, progressBar);

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
        // for quests
        jsonReader = new(new StreamReader($"{_folderName}/Quests.json"));
        save.quests = jsonSerializer.Deserialize<QuestControllerSave>(jsonReader);
        jsonReader.Close();
        return save;
    }



    /// <summary>
    /// Preps <see cref="SceneRefs"/>, <see cref="SceneRefs"/> and loads saved data.
    /// </summary>
    /// <param name="obj"></param>
    async Task LoadWorldData(Save save, ProgressBar bar)
    {
        MyGrid.worldName = worldName;
        WorldSave worldSave = save.world;
        GameStateSave gameState = save.gameState;
        HumanSave[] humanSaves = save.humans;
        ResearchSave researchSave = save.research;
        TradeSave tradeSave = save.trade;
        QuestControllerSave questSave = save.quests;

        // create progress val for loading slider
        int maxprogress = 0;
        for (int i = 0; i < worldSave.gridSave.Length; i++)
            maxprogress += worldSave.gridSave[i].width * worldSave.gridSave[i].height * TILE_WEIGHT; // Tiles and pipes
        maxprogress += worldSave.objectsSave.buildings.Length * BUILD_WEIGHT; //scale number
        maxprogress += worldSave.objectsSave.chunks.Length * CHUNK_WEIGHT;
        maxprogress += humanSaves.Length * HUMAN_WEIGHT;
        maxprogress += researchSave.count * RES_WEIGHT;
        bar.highValue = maxprogress;

        await LoadMap(worldSave);
        await LoadGameState(gameState);
        await LoadHumans(humanSaves);
        await LoadResearches(researchSave);
        await LoadTrade(tradeSave);
        await LoadQuests(questSave);
    }


    #region Model Loading
    /// <summary>
    /// Loads grid objects.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="worldSave"></param>
    Task LoadMap(WorldSave worldSave)
    {
        ActionText = "Loading grid";
        LoadGrid(worldSave.gridSave);
        ActionText = "Filling Veins";
        LoadVeins(worldSave.objectsSave.veins);
        ActionText = "Spawning chunks";
        LoadChunks( worldSave.objectsSave.chunks);
        ActionText = "Constructing buildings";
        LoadBuildings(worldSave.objectsSave.buildings);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates chunks.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="chunks"></param>
    void LoadChunks( ChunkSave[] chunks)
    {
        foreach (ChunkSave chunkSave in chunks)
        {
            SceneRefs.ObjectFactory
                .CreateChunk(chunkSave.gridPos, chunkSave.resSave, false)
                .Load(chunkSave);
            ProgressGlobal += CHUNK_WEIGHT;
        }
    }

    /// <summary>
    /// Creates Buildings.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="buildings"></param>
    void LoadBuildings( BuildingSave[] buildings)
    {
        foreach (BuildingSave save in buildings) // for each saved building
        {
            SceneRefs.ObjectFactory.CreateSavedBuilding(save);
            ProgressGlobal += BUILD_WEIGHT;
        }
    }

    void LoadVeins( VeinSave[] veins)
    {
        foreach (VeinSave save in veins) // for each saved building
        {
            SceneRefs.ObjectFactory.CreateSavedVein(save);
            ProgressGlobal += BUILD_WEIGHT;
        }
    }

    /// <summary>
    /// Recreates the tiles of the grid.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="gridSave"></param>
    void LoadGrid(GridSave[] gridSave)
    {
        // Empties grid
        MyGrid.PrepGridLists();
        // creates an empty ground level

        MapGen mapGen = gameObject.GetComponent<MapGen>();
        for (int i = 0; i < gridSave.Length; i++)
        {
            ProgressGlobal += TILE_WEIGHT * MyGrid.Load(
                gridSave[i], 
                templateLevel, 
                i, 
                mapGen.minableResources,
                mapGen.dirt);
        }
    }

    /// <summary>
    /// Loads humans.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="humanSaves"></param>
    Task LoadHumans( HumanSave[] humanSaves)
    {
        ActionText = "Kidnaping workers";
        //SceneRefs.Humans.
        foreach (HumanSave save in humanSaves)
        {
            SceneRefs.Humans.LoadHuman(save, ref humanActivation);
            ProgressGlobal += HUMAN_WEIGHT;//SceneRefs.Humans.LoadHumans(humanSaves, ref humanActivation, );
        }
        return Task.CompletedTask;
    }
    #endregion Model Loading

    #region UI loading

    /// <summary>
    /// Loads player preferences.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="gameState"></param>
    Task LoadGameState( GameStateSave gameState)
    {
        SceneRefs.JobQueue.priority = gameState.priorities;
        SceneRefs.Tick.Load(gameState);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Loads Research.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="researchSave"></param>
    async Task LoadResearches( ResearchSave researchSave)
    {
        ActionText = "Remembering research";
        await UIRefs.ResearchWindow.LoadState(researchSave);
    }

    /// <summary>
    /// Loads Trading.
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="tradeSave"></param>
    async Task LoadTrade( TradeSave tradeSave)
    {
        ActionText = "Making Deals";
        await UIRefs.TradingWindow.LoadState(tradeSave);
    }

    async Task LoadQuests( QuestControllerSave questSave)
    {
        QuestController controller = SceneRefs.QuestController as QuestController;
        try
        {
            Task task = controller.LoadState(questSave);
            await task;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    #endregion UI loading
    #endregion Loading Game State


    async Task BeforeLevelLoad()
    {
        if (GameObject.Find("Main Menu"))
            await SceneManager.UnloadSceneAsync("Main Menu");
        await SceneManager.LoadSceneAsync("Level", LoadSceneMode.Additive);
        await GameObject.Find("Scene").GetComponent<SceneRefs>().BeforeLoad();
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

        ActionText = "Press any button";
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

    public void UIUpdate(string property = "")
    {
        propertyChanged?.Invoke(this, new(property));
    }
}
