using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public GridSave gridSave;
    public PlayerSettings playerSettings;
    public List<HumanSave> humanSaves;
    public ResearchSave researchSaves;

    [SerializeField] GroundLevel startLevel;
    [SerializeField] GroundLevel templateLevel;
    public event Action humanActivation;

    GameObject roadPref;
    public void LoadMainMenu()
    {
        StartCoroutine(Load(2)); // the loading Process
    }
    IEnumerator Load(int id)
    {
        var scene = SceneManager.LoadSceneAsync(id, LoadSceneMode.Additive);
        // starts the loading
        do
        {
            yield return new WaitForSeconds(0.2f);
        }
        while (!scene.isDone);
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
            MyGrid.CreateGrid(startLevel);
            MyGrid.canvasManager.research.NewGame();
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
        StartCoroutine(Load(3));
    }

    //////////////////////////////////////////////////////////////////
    //-----------------------------Loading--------------------------//
    //////////////////////////////////////////////////////////////////
    
    public void LoadSave(GridSave _gridSave, PlayerSettings _playerSettings, List<HumanSave> _humanSaves, ResearchSave _researchSaves, string _folderName)
    {
        gridSave = _gridSave;
        playerSettings = _playerSettings;
        humanSaves = _humanSaves;
        researchSaves = _researchSaves;
        folderName = _folderName;

        if (GameObject.Find("Main Menu"))
        {
            var unloading = SceneManager.UnloadSceneAsync("Main Menu");
            unloading.completed += LoadLevel;
        }
        else 
        {
            LoadLevel(null);
        }
    }
    void LoadLevel(AsyncOperation obj)
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = folderName;
        transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
        var loading = SceneManager.LoadSceneAsync("Level", LoadSceneMode.Additive);
        loading.completed += LoadWorld;
    }

    void LoadWorld(AsyncOperation obj)
    {
        MyGrid.sceneReferences = GameObject.Find("Scene").GetComponent<SceneReferences>();
        MyGrid.canvasManager = GameObject.Find("UI canvas").GetComponent<CanvasManager>();
        Slider slider = transform.GetChild(0).GetChild(1).GetComponent<Slider>();
        // create Progress val for loading slider
        int maxProgress = gridSave.width * gridSave.height*4; // Tiles and pipes
        maxProgress += gridSave.buildings.Count * buildWeigth; //scale number
        maxProgress += gridSave.chunks.Count;
        /*maxProgress += jobSaves.Count;
        maxProgress += humanSaves.Count;*/
        slider.maxValue = maxProgress;
        Progress<int> progress = new Progress<int>(value =>
        {
            slider.value = value;
        });
        FillPlayerSettings(progress);
        FillGrid(progress);
        FillHumans(progress);
        FillResearches(progress);
        //..
        StartCoroutine(LoadWaiting());
    }

    IEnumerator LoadWaiting()
    {
        yield return new WaitForSeconds(2);
        MyGrid.gridTiles = MyGrid.sceneReferences.eventSystem.GetChild(0).GetComponent<GridTiles>();
        AfterLevelLoad(false);
    }
    void FillPlayerSettings(IProgress<int> progress)
    {
        MyGrid.sceneReferences.humans.GetComponent<JobQueue>().priority = playerSettings.priorities;
    }
    //////////////////////////////////////////////////////////////////
    //-----------------------------Grid-----------------------------//
    //////////////////////////////////////////////////////////////////
    void FillGrid(IProgress<int> progress)
    {
        roadPref = MyGrid.tilePrefabs.GetPrefab("Road").gameObject;
        CreateGrid(progress);
        CreateChunks(progress);
        CreatePipes(progress);
        CreateBuilding(progress);
    }

    void CreateGrid(IProgress<int> progress)
    {
        // creates new grid
        MyGrid.ClearGrid();
        
        // gets gridTiles reference for rocks
        MyGrid.gridTiles = MyGrid.sceneReferences.eventSystem.GetChild(0).GetComponent<GridTiles>();
        // creates an empty ground level
        GroundLevel groundLevel = Instantiate(templateLevel, MyGrid.gridTiles.transform);
        MyGrid.sceneReferences.levels.Add(groundLevel);

        Camera.main.GetComponent<PhysicsRaycaster>().eventMask = MyGrid.gridTiles.defaultMask;
        // process of creating items
        CreateTiles(progress);
    }

    /// <summary>
    /// Instantiates and fills ClickableObject tiles (Rock, Water, Road).
    /// </summary>
    /// <param name="progress"></param>
    void CreateTiles(IProgress<int> progress)
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
                        Rock rock = Instantiate(MyGrid.tilePrefabs.GetPrefab((objectSave as RockSave).oreName), new(x, 1.5f, z), Quaternion.identity, MyGrid.sceneReferences.levels[0].rocks).GetComponent<Rock>();
                        rock.Load(objectSave);
                        MyGrid.SetGridItem(new(x,z), rock);
                        if (rock.toBeDug)
                        {
                            MyGrid.gridTiles.toBeDigged.Add(rock);
                            MyGrid.gridTiles.HighLight(MyGrid.gridTiles.toBeDugColor, rock.gameObject);
                        }
                        break;
                    case BSave:
                        //MyGrid.grid[x, z] = null;
                        break;
                    case WaterSave:
                        Water water = Instantiate(waterPref, new(x, 0, z), Quaternion.identity, MyGrid.sceneReferences.levels[0].water).GetComponent<Water>();
                        water.Load(objectSave);
                        MyGrid.SetGridItem(new(x, z), water);
                        break;
                    default:
                        Road road = Instantiate(roadPref, new(x, 0.45f, z), Quaternion.identity, MyGrid.sceneReferences.levels[0].roads).GetComponent<Road>();
                        MyGrid.SetGridItem(new(x, z), road);
                        break;
                }
                progress.Report(progressGlobal += 2);
            }
        }
        MyGrid.sceneReferences.humans.GetComponent<JobQueue>().toBeDug = MyGrid.gridTiles.toBeDigged.ToList();
    }

    void CreateChunks(IProgress<int> progress)
    {
        MyGrid.chunks = new();
        GameObject chunkPref = MyGrid.specialPrefabs.GetPrefab("Chunk").gameObject;
        foreach (ChunkSave chunkSave in gridSave.chunks)
        {
            Vector3 vec = chunkSave.gridPos.ToVec();
            MyGrid.chunks.Add(Instantiate(chunkPref, new(vec.x, 1, vec.z), Quaternion.identity, MyGrid.sceneReferences.levels[0].chunks).GetComponent<Chunk>());
            MyGrid.chunks[^1].Load(chunkSave);
        }
    }

    void CreatePipes(IProgress<int> progress)
    {
        GameObject pipePref = MyGrid.buildPrefabs.GetPrefab("Fluid Pipe base").gameObject;
        for (int x = 0; x < gridSave.height; x++)
        {
            for (int z = 0; z < gridSave.width; z++)
            {
                if (gridSave.pipes[x,z] != null)
                {
                    Instantiate(pipePref, new(x, 1.6f, z), Quaternion.identity, MyGrid.sceneReferences.levels[0].pipes)
                        .GetComponent<Pipe>().Load(gridSave.pipes[x, z]);
                }
                progress.Report(progressGlobal += 2);
            }
        }
    }

    /// <summary>
    /// Instantiates and places all the buildings.
    /// </summary>
    /// <param name="progress"></param>
    void CreateBuilding(IProgress<int> progress)
    {
        foreach (BSave save in gridSave.buildings) // for each saved building
        {
            Building _pref = MyGrid.buildPrefabs.GetPrefab(save.prefabName) as Building; // find the prefab
            // create the prefab
            Building b = Instantiate(_pref,
                new(save.gridPos.x, 1, save.gridPos.z),
                Quaternion.Euler(0, save.rotationY, 0),
                MyGrid.sceneReferences.levels[0].buildings);

            // fill the prefab with saved Data
            b.Load(save);
            MyGrid.PlaceBuild(b, true);
            progress.Report(progressGlobal += buildWeigth);
        }
    }
    
    //////////////////////////////////////////////////////////////////
    //-----------------------------Humans---------------------------//
    //////////////////////////////////////////////////////////////////
    void FillHumans(IProgress<int> progress)
    {
        // Destroys all humans 
        for (int i = MyGrid.sceneReferences.humans.GetChild(0).childCount-1; i >= 0; i--)
        {
            Destroy(MyGrid.sceneReferences.humans.GetChild(0).GetChild(i).gameObject);
        }
        GameObject humanPrefab = MyGrid.specialPrefabs.GetPrefab("Human").gameObject;
        Humans humans = MyGrid.sceneReferences.humans.GetComponent<Humans>();
        JobQueue jobQueue = humans.GetComponent<JobQueue>();
        humans.humen = new();
        foreach (HumanSave h in humanSaves)
        {
            int parent = h.workplaceId > -1 ? 1 : 0;
            Human human = GameObject.Instantiate(humanPrefab,
                new Vector3(h.gridPos.x, 1, h.gridPos.z), 
                Quaternion.identity,
                MyGrid.sceneReferences.humans.GetChild(parent)).GetComponent<Human>();
            human.transform.GetChild(1).GetComponent<MeshRenderer>().material.color = h.color.ConvertColor();
            human.id = h.id;
            human.jData = new(h.jobSave, human);
            human.name = h.name;
            human.inventory = h.inventory;
            human.specialization = h.specs;
            human.sleep = h.sleep;
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

    void FillResearches(Progress<int> progress)
    {
        MyGrid.canvasManager.research.LoadGame(researchSaves);
    }

    void AfterLevelLoad(bool newGame)
    {
        transform.parent.GetChild(1).GetComponent<AudioListener>().enabled = false;

        MyGrid.sceneReferences.humans.GetComponent<Humans>().GetHumans();
        MyGrid.canvasManager.gameObject.SetActive(true);
        MyGrid.canvasManager.buildMenu.GetChild(2).GetComponent<PreBuildInfo>().SetUp();
        MyGrid.sceneReferences.GetComponent<SaveController>().activeFolder = folderName;
        MyGrid.canvasManager.stats.GetChild(1).GetChild(1).GetComponent<TimeButtons>().tick = MyGrid.sceneReferences.GetComponent<Tick>();
        

        Camera.main.GetComponent<PhysicsRaycaster>().eventMask = MyGrid.gridTiles.defaultMask;
        Camera.main.GetComponent<PhysicsRaycaster>().enabled = true;
        Camera.main.GetComponent<Physics2DRaycaster>().enabled = true;
        Camera.main.GetComponent<AudioListener>().enabled = true;

        MyRes.ActivateResources(newGame);
        MyGrid.sceneReferences.GetComponent<Tick>().AwakeTicks();
        SceneManager.UnloadSceneAsync("LoadingScreen");
    }

}
