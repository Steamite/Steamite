using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Scripts.Saving___Loading
{
    public class LoadingScreenNew : MonoBehaviour, IUpdatable
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
        public async Awaitable OpenMainMenu(bool v)
        {
            await SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Additive);
            if (!v)
                (GameObject.FindGameObjectWithTag("GameController").GetComponentAtIndex(2) as IInitiableUI).Init();
        }


        public async Awaitable StartNewGame(string _folderName, string seed = "")
        {
            NewGameInit newGameInit = gameObject.GetComponent<NewGameInit>();
            WorldSaveNew worldSave;
            bool randomMap = seed != "";
            if (randomMap)
            {
                worldSave = await gameObject.GetComponent<MapGenNew>().Generate(seed);
            }
            else
            {
                return;
            }
            /*else
            {
                size = testLevels[0].height;
                newGameInit.CreateGrid(testLevels, out worldSave);
            }*/

            SaveNew saveData = new SaveNew()
            {
                gameState = newGameInit.SetNewGameState(),
                trade = await newGameInit.InitTrade(0),
                research = await newGameInit.InitResearch(),
                humans = newGameInit.InitHumans((worldSave.chunkDimensions * worldSave.gridSave[0].levelDimensions)/2),
                quests = await newGameInit.InitQuests(randomMap),
                world = worldSave
            };
            await StartLoad(_folderName, saveData);
        }
        #endregion

        #region Loading Game State
        public async Awaitable LoadGame(string _savePath)
        {
            await Awaitable.MainThreadAsync();

            SaveNew save = LoadSavedData(_savePath);
            await StartLoad(Directory.GetParent(_savePath).Name, save);
        }

        async Awaitable StartLoad(string _worldName, SaveNew save)
        {
            worldName = _worldName;
            var completionSource = new AwaitableCompletionSource();
            PanelRendererRoot panel = GetComponent<PanelRendererRoot>();

            PanelRenderer.UIReloadCallback onReady = null;

            VisualElement element = null;

            onReady = (pan, el) =>
            {
                element = el;
                pan.UnregisterUIReloadCallback(onReady);

                completionSource.TrySetResult();
            };

            panel.RegisterReload(onReady);//.RegisterUIReloadCallback(onReady);


            await completionSource.Awaitable;
            await Load(save, element);
        }

        async Awaitable Load(SaveNew save, VisualElement root)
        {

            root[0].style.display = DisplayStyle.Flex;
            Label actionText = root.Q<Label>("Title");
            ActionText = "Loading";
            actionText.SetBinding(nameof(ActionText), "text", this);

            ProgressBar progressBar = root.Q<ProgressBar>();
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
        SaveNew LoadSavedData(string _folderName)
        {
            SaveNew save = new();

            JsonSerializer jsonSerializer = SaveController.PrepSerializer();
            // for gridSave
            save.world = new();
            JsonTextReader jsonReader = new(new StreamReader($"{_folderName}/Grid.json"));
            save.world.objectsSave = jsonSerializer.Deserialize<BuildsAndChunksSave>(jsonReader);
            jsonReader.Close();
            // for all levels
            save.world.gridSave = new LevelGridSave[MyGrid.NUMBER_OF_LEVELS];
            for (int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
            {
                jsonReader = new(new StreamReader($"{_folderName}/Level{i}.json"));
                save.world.gridSave[i] = jsonSerializer.Deserialize<LevelGridSave>(jsonReader);
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
        async Awaitable LoadWorldData(SaveNew save, ProgressBar bar)
        {
            MyGrid.worldName = worldName;
            WorldSaveNew worldSave = save.world;
            GameStateSave gameState = save.gameState;
            HumanSave[] humanSaves = save.humans;
            ResearchSave researchSave = save.research;
            TradeSave tradeSave = save.trade;
            QuestControllerSave questSave = save.quests;

            // create progress val for loading slider
            int maxprogress = 0;
            /*for (int i = 0; i < MyGrid.NUMBER_OF_LEVELS; i++)
                maxprogress += worldSave.gridSave[i].width * worldSave.gridSave[i].height * TILE_WEIGHT; // Tiles and pipes*/
            maxprogress += worldSave.objectsSave.buildings.Length * BUILD_WEIGHT; //scale number
            maxprogress += worldSave.objectsSave.chunks.Length * CHUNK_WEIGHT;
            maxprogress += humanSaves.Length * HUMAN_WEIGHT;
            maxprogress += researchSave.count * RES_WEIGHT;
            bar.highValue = maxprogress;
            await Awaitable.MainThreadAsync();

            try
            {
                LoadMap(worldSave);
                LoadGameState(gameState);
                LoadHumans(humanSaves);
                await LoadResearches(researchSave);
                await LoadTrade(tradeSave);
                await LoadQuests(questSave);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load:" + e);
                // TODO add a button to return to main menu
            }
        }


        #region Model Loading
        /// <summary>
        /// Loads grid objects.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="worldSave"></param>
        void LoadMap(WorldSaveNew worldSave)
        {
            ActionText = "Loading grid";
            GameObject.FindAnyObjectByType<GridWorld>().LoadGrid(worldSave);/*
            ActionText = "Filling Veins";
            LoadVeins(worldSave.objectsSave.veins);*/
            ActionText = "Spawning chunks";
            LoadChunks(worldSave.objectsSave.chunks);
            ActionText = "Constructing buildings";
            LoadBuildings(worldSave.objectsSave.buildings);
        }

        /// <summary>
        /// Creates chunks.
        /// </summary>
        /// <param name="chunks"></param>
        void LoadChunks(ChunkSave[] chunks)
        {
            foreach (ChunkSave chunkSave in chunks)
            {
                SceneRefs.ObjectFactory
                    .CreateChunk(chunkSave.gridPos, new(chunkSave.resSave), false)
                    .Load(chunkSave);
                ProgressGlobal += CHUNK_WEIGHT;
            }
        }

        /// <summary>
        /// Creates Buildings.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="buildings"></param>
        void LoadBuildings(BuildingSave[] buildings)
        {
            foreach (BuildingSave save in buildings) // for each saved building
            {
                SceneRefs.ObjectFactory.CreateSavedBuilding(save);
                ProgressGlobal += BUILD_WEIGHT;
            }
        }

        void LoadVeins(VeinSave[] veins)
        {
            foreach (VeinSave save in veins) // for each saved building
            {
                SceneRefs.ObjectFactory.CreateSavedVein(save);
                ProgressGlobal += BUILD_WEIGHT;
            }
        }

        /// <summary>
        /// Loads humans.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="humanSaves"></param>
        void LoadHumans(HumanSave[] humanSaves)
        {
            ActionText = "Kidnaping workers";
            //SceneRefs.Humans.
            foreach (HumanSave save in humanSaves)
            {
                SceneRefs.Humans.LoadHuman(save, ref humanActivation);
                ProgressGlobal += HUMAN_WEIGHT;//SceneRefs.Humans.LoadHumans(humanSaves, ref humanActivation, );
            }
        }
        #endregion Model Loading

        #region UI loading

        /// <summary>
        /// Loads player preferences.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="gameState"></param>
        void LoadGameState(GameStateSave gameState)
        {
            SceneRefs.JobQueue.priority = gameState.priorities;
            SceneRefs.Tick.Load(gameState);
        }

        /// <summary>
        /// Loads Research.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="researchSave"></param>
        async Awaitable LoadResearches(ResearchSave researchSave)
        {
            ActionText = "Remembering research";
            await UIRefs.Research.LoadState(researchSave);
        }

        /// <summary>
        /// Loads Trading.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="tradeSave"></param>
        async Awaitable LoadTrade(TradeSave tradeSave)
        {
            ActionText = "Making Deals";
            await UIRefs.Trading.LoadState(tradeSave);
        }

        async Awaitable LoadQuests(QuestControllerSave questSave)
        {
            QuestController controller = SceneRefs.QuestController as QuestController;
            try
            {
                ActionText = "Writing quests";
                await controller.LoadState(questSave);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        #endregion UI loading
        #endregion Loading Game State


        async Awaitable BeforeLevelLoad()
        {
            if (GameObject.Find("Main Menu"))
                await SceneManager.UnloadSceneAsync("Main Menu");
            await SceneManager.LoadSceneAsync("Level", LoadSceneMode.Additive);
            await GameObject.Find("Scene").GetComponent<SceneRefs>().BeforeLoad();
        }

        IDisposable buttonListener;
        /// <summary>
        /// Things that need to be after both loading and creating a new game.
        /// </summary>
        /// <param name="newGame">Is it the new game(used for calling the same methods just with different paramaters).</param>
        void AfterLevelLoad(bool newGame)
        {
            MyGrid.worldName = worldName;

            MyRes.ActivateResources();

            ActionText = "Press any button";

            buttonListener = InputSystem.onAnyButtonPress.CallOnce(
                (action) => StartCoroutine(CancelInput()));
        }

        void RemoveButtonListener()
        {

            if (buttonListener == null)
                return;

            buttonListener.Dispose();
            buttonListener = null;
        }
        private void OnDisable()
        {
            RemoveButtonListener();
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


#if UNITY_INCLUDE_TESTS
        public event Action onFinishLoad;
#endif

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
                if (SceneManager.GetSceneAt(i).name == "Loading Screen" && SceneManager.GetSceneAt(i).isLoaded)
                    await SceneManager.UnloadSceneAsync("Loading Screen");
            }

            SceneRefs.FinishLoad();

            humanActivation?.Invoke();
            humanActivation = null;
            SceneRefs.Tick.InitTicks();
            UIRefs.TopBar.RegisterReload(
                (a, b) => UIRefs.TopBar.GetComponent<IToolkitController>().Init(b));

#if UNITY_INCLUDE_TESTS
            onFinishLoad?.Invoke();
#endif
        }

        public void UIUpdate(string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }
    }
}
