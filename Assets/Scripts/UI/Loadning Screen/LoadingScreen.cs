using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
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
    public event Action humanActivation;

    GameObject roadPref;
    public void LoadMainMenu()
    {
        Load(2); // the loading Process
    }
    async void Load(int id)
    {
        await SceneManager.LoadSceneAsync(id, LoadSceneMode.Additive);
        
        Debug.Log("start:" + Time.realtimeSinceStartup);
        if(id == 2) // if main menu is being activated
        {
            // sets Action Listeners
            MainMenu mainMenu = GameObject.Find("Main Menu").GetComponent<MainMenu>();
            mainMenu.newGame += NewGame;
            LoadMenu saveLoader = mainMenu.transform.GetChild(3).GetComponent<LoadMenu>();
            //saveLoader.loadGame += LoadSave;
        }
        else // if loading level
        {
            MyGrid.CreateGrid(startLevel, mainLevel);
            CanvasManager.research.NewGame();
            CanvasManager.trade.NewGame();
            AfterLevelLoad(true);
        }
        Debug.Log("end:" + Time.realtimeSinceStartup);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void NewGame(string _folderName, bool tutorial)
    {
        folderName = _folderName;
        if (folderName == "test - TopGun")
            CreateW(null);
        else
        {
            AsyncOperation sceneUnloading = SceneManager.UnloadSceneAsync("Main Menu");
            sceneUnloading.completed += CreateW;
        }
    }
    void CreateW(AsyncOperation obj)
    {
        transform.GetChild(0).gameObject.SetActive(true);
        Load(3);
    }

    //////////////////////////////////////////////////////////////////
    //-----------------------------Loading--------------------------//
    //////////////////////////////////////////////////////////////////

    /// <summary>
    /// Assigns parameters and shows loading screen.<br/>
    /// If called while on "Main Menu" scene unloads it.<br/>
    /// After that loads "Level" scene.
    /// </summary>
    /// <param name="_folderName">Current folder name.</param>
    public async void StartLoading(GridSave _gridSave, PlayerSettings _playerSettings, List<HumanSave> _humanSaves,
        ResearchSave _researchSave, TradeSave _tradeSave, string _folderName)
    {
        folderName = _folderName;

        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = folderName;
        transform.GetChild(0).GetChild(2).gameObject.SetActive(true);

        if (GameObject.Find("Main Menu"))
        {
           await SceneManager.UnloadSceneAsync("Main Menu");
        }
        await SceneManager.LoadSceneAsync("Level", LoadSceneMode.Additive);
        LoadWorldData(_gridSave, _playerSettings, _humanSaves, _researchSave, _tradeSave);
    }

    /// <summary>
    /// Uses the data from
    /// </summary>
    /// <param name="obj"></param>
    void LoadWorldData(GridSave gridSave, PlayerSettings playerSettings, List<HumanSave> humanSaves,
        ResearchSave researchSave, TradeSave tradeSave)
    {
        GameObject.Find("Scene").GetComponent<SceneRefs>().Init();
        GameObject.Find("UI canvas").GetComponent<CanvasManager>().Init();
        Slider slider = transform.GetChild(0).GetChild(1).GetComponent<Slider>();
        // create Progress val for loading slider
        int maxProgress = gridSave.width * gridSave.height*4; // Tiles and pipes
        maxProgress += gridSave.buildings.Count * buildWeigth; //scale number
        maxProgress += gridSave.chunks.Count;
        maxProgress += humanSaves.Count;
        maxProgress += researchSave.categories.SelectMany(q => q.nodes).Count();
        //maxProgress += trade
        slider.maxValue = maxProgress;
        Progress<int> progress = new Progress<int>(value =>
        {
            slider.value = value;
        });
        //FillPlayerSettings(progress, playerSettings);
        //FillGrid(progress, gridSave);
        //FillHumans(progress, humanSaves);
        //FillResearches(progress, researchSave);
        //FillTrade(progress, tradeSave);
        //..
        StartCoroutine(LoadWaiting());
    }

    IEnumerator LoadWaiting()
    {
        yield return new WaitForSeconds(2);
        GameObject.Find("Scene").GetComponent<SceneRefs>().Init();
        AfterLevelLoad(false);
    }
    void FillPlayerSettings(IProgress<int> progress, PlayerSettings playerSettings)
    {
        SceneRefs.humans.GetComponent<JobQueue>().priority = playerSettings.priorities;
    }
    //////////////////////////////////////////////////////////////////
    //-----------------------------Grid-----------------------------//
    //////////////////////////////////////////////////////////////////
    void FillGrid(IProgress<int> progress, GridSave gridSave)
    {
        throw new NotImplementedException();
        /*roadPref = MyGrid.tilePrefabs.GetPrefab("Road").gameObject;
        CreateGrid(progress, gridSave);
        CreateChunks(progress, gridSave);
        SceneRefs.objectFactory.CreatePipes(progress, gridSave);
        CreateBuilding(progress, gridSave);*/
    }
    /*
    /// <summary>
    /// 
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="gridSave"></param>
    void CreateGrid(IProgress<int> progress, GridSave gridSave)
    {
        // gets gridTiles reference for rocks
        SceneRefs.gridTiles = SceneRefs.eventSystem.GetChild(0).GetComponent<GridTiles>();
        
        // Empties grid
        MyGrid.ClearGrid();
        
        // creates an empty ground level
        GroundLevel groundLevel = Instantiate(templateLevel, SceneRefs.gridTiles.transform);
        SceneRefs.levels.Add(groundLevel);

        Camera.main.GetComponent<PhysicsRaycaster>().eventMask = SceneRefs.gridTiles.defaultMask;
        // process of creating items
        CreateTiles(progress, gridSave);
    }

    /// <summary>
    /// Instantiates and fills ClickableObject tiles (Rock, Water, Road).
    /// </summary>
    /// <param name="progress"></param>
    void CreateTiles(IProgress<int> progress, GridSave gridSave)
    {
        ClickableObject waterPref = MyGrid.tilePrefabs.GetPrefab("Water");//Assets/Resources/.prefab
        for (int x = 0; x < gridSave.height; x++)
        {
            for (int z = 0; z < gridSave.width; z++)
            {
                // Creates the tile object
                ClickableObjectSave objectSave = gridSave.gridItems[x, z];
                switch (objectSave)
                {
                    case RockSave:
                        Rock rock = Instantiate(MyGrid.tilePrefabs.GetPrefab((objectSave as RockSave).oreName), new(x, 1.5f, z), Quaternion.identity, SceneRefs.levels[0].rocks).GetComponent<Rock>();
                        rock.Load(objectSave);
                        MyGrid.SetGridItem(new(x,z), rock);
                        if (rock.toBeDug)
                        {
                            SceneRefs.gridTiles.toBeDigged.Add(rock);
                            SceneRefs.gridTiles.HighLight(SceneRefs.gridTiles.toBeDugColor, rock.gameObject);
                        }
                        break;
                    case BSave:
                        //MyGrid.grid[x, z] = null;
                        break;
                    case WaterSave:
                        Water water = Instantiate(waterPref, new(x, 0, z), Quaternion.identity, SceneRefs.levels[0].water).GetComponent<Water>();
                        water.Load(objectSave);
                        MyGrid.SetGridItem(new(x, z), water);
                        break;
                    default:
                        Road road = Instantiate(roadPref, new(x, 0.45f, z), Quaternion.identity, SceneRefs.levels[0].roads).GetComponent<Road>();
                        MyGrid.SetGridItem(new(x, z), road);
                        break;
                }
                progress.Report(progressGlobal += 2);
            }
        }
        SceneRefs.humans.GetComponent<JobQueue>().toBeDug = SceneRefs.gridTiles.toBeDigged.ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="gridSave"></param>
    void CreateChunks(IProgress<int> progress, GridSave gridSave)
    {
        MyGrid.chunks = new();
        GameObject chunkPref = MyGrid.specialPrefabs.GetPrefab("Chunk").gameObject;
        foreach (ChunkSave chunkSave in gridSave.chunks)
        {
            Vector3 vec = chunkSave.gridPos.ToVec();
            MyGrid.chunks.Add(Instantiate(chunkPref, new(vec.x, 1, vec.z), Quaternion.identity, SceneRefs.levels[0].chunks).GetComponent<Chunk>());
            MyGrid.chunks[^1].Load(chunkSave);
        }
    }
    

    /// <summary>
    /// Instantiates and places all the buildings.
    /// </summary>
    /// <param name="progress"></param>
    void CreateBuilding(IProgress<int> progress, GridSave gridSave)
    {
        foreach (BSave save in gridSave.buildings) // for each saved building
        {
            Building _pref = MyGrid.buildPrefabs.GetPrefab(save.prefabName) as Building; // find the prefab
            // create the prefab
            Building b = Instantiate(_pref,
                new(save.gridPos.x, 1, save.gridPos.z),
                Quaternion.Euler(0, save.rotationY, 0),
                SceneRefs.levels[0].buildings);

            // fill the prefab with saved Data
            b.Load(save);
            MyGrid.PlaceBuild(b, true);
            progress.Report(progressGlobal += buildWeigth);
        }
    }
    
    //////////////////////////////////////////////////////////////////
    //-----------------------------Humans---------------------------//
    //////////////////////////////////////////////////////////////////
    void FillHumans(IProgress<int> progress, List<HumanSave> humanSaves)
    {
        // Destroys all humans 
        for (int i = SceneRefs.humans.transform.GetChild(0).childCount-1; i >= 0; i--)
        {
            Destroy(SceneRefs.humans.transform.GetChild(0).GetChild(i).gameObject);
        }
        GameObject humanPrefab = MyGrid.specialPrefabs.GetPrefab("Human").gameObject;
        Humans humans = SceneRefs.humans.GetComponent<Humans>();
        JobQueue jobQueue = humans.GetComponent<JobQueue>();
        humans.humen = new();
        foreach (HumanSave h in humanSaves)
        {
            int parent = h.workplaceId > -1 ? 1 : 0;
            Human human = GameObject.Instantiate(humanPrefab,
                new Vector3(h.gridPos.x, 1, h.gridPos.z), 
                Quaternion.identity,
                SceneRefs.humans.transform.GetChild(parent)).GetComponent<Human>();
            human.transform.GetChild(1).GetComponent<MeshRenderer>().material.color = h.color.ConvertColor();
            human.id = h.id;
            human.jData = new(h.jobSave, human);
            human.name = h.name;
            human.inventory = h.inventory;
            human.specialization = h.specs;
            if (human.jData.path.Count > 0)
                human.ChangeAction(HumanActions.Move);
            else
                human.Decide();
            // house assigment
            if (h.houseID != -1)
            {
                human.home = MyGrid.buildings.Where(q => q.id == h.houseID).
                    SingleOrDefault().GetComponent<House>();
                human.home.assigned.Add(human);
            }
            // workplace assigment
            if (h.workplaceId != -1)
            {
                human.workplace = MyGrid.buildings.Where(q => q.id == h.workplaceId).
                    SingleOrDefault().GetComponent<ProductionBuilding>();
                human.workplace.assigned.Add(human);
            }
            humans.AddHuman(human, ref humanActivation);
            progress.Report(progressGlobal++);
        }
    }

    void FillResearches(Progress<int> progress, ResearchSave researchSave)
    {
        CanvasManager.research.LoadGame(researchSave);
    }

    void FillTrade(Progress<int> progress, TradeSave tradeSave)
    {
        CanvasManager.trade.LoadGame(tradeSave);
    }
    */
    async void AfterLevelLoad(bool newGame)
    {
        transform.parent.GetChild(1).GetComponent<AudioListener>().enabled = false;

        SceneRefs.humans.GetComponent<Humans>().GetHumans();
        CanvasManager.miscellaneous.GetChild(1).GetComponent<LocalInfoWindow>().SetUp();
        SceneRefs.saveController.activeFolder = folderName;
        MyGrid.Init();
        Camera.main.GetComponent<PhysicsRaycaster>().eventMask = SceneRefs.gridTiles.defaultMask;
        Camera.main.GetComponent<PhysicsRaycaster>().enabled = true;
        Camera.main.GetComponent<Physics2DRaycaster>().enabled = true;
        Camera.main.GetComponent<AudioListener>().enabled = true;

        MyRes.ActivateResources(newGame);
        await SceneManager.UnloadSceneAsync("LoadingScreen");
        CanvasManager.stats.GetChild(0).GetChild(0).GetComponent<TimeButtons>().SetStartSpeed(SceneRefs.tick);
        //CanvasManager.ToggleCanvases();
    }

}
