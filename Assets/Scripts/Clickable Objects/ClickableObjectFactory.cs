using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>Instantiates and fills new Clickable Objects.</summary>
public class ClickableObjectFactory : MonoBehaviour, IBeforeLoad
{
    #region Y grid offset
    /// <summary>Height of each level.</summary>
    public const int LEVEL_HEIGHT = 2;

    public const int BUILD_OFFSET = 1;
    public const int CHUNK_OFFSET = 1;

    public const float HUMAN_OFFSET = 0.47f;
    public const float ROCK_OFFSET = 1.5f;
    public const float ROAD_OFFSET = 0.45f;
    #endregion

    #region Prefabs
    public BuildingData buildPrefabs;
    public ResourceHolder tilePrefabs;
    public ResourceHolder specialPrefabs;
    #endregion

    #region Tiles
    /// <summary>
    /// Creates a new <see cref="Road"/> and registers it on Grid.
    /// </summary>
    /// <param name="gp">Position to create at.</param>
    /// <param name="doSet">If the road should be registered.</param>
    public void CreateRoad(GridPos gp, bool doSet)
    {
        Road replacement = Instantiate(
            tilePrefabs.GetPrefab("Road").gameObject,
            gp.ToVec(ROAD_OFFSET),
            Quaternion.identity,
            MyGrid.FindLevelRoads(gp.y)).GetComponent<Road>(); // creates a road on the place of tiles

        replacement.objectName = replacement.objectName.Replace("(Clone)", "");
        if (doSet)
            MyGrid.SetGridItem(gp, replacement);
    }

    /// <summary>
    /// Creates a new <see cref="Rock"/>, sets it's color and all data.
    /// </summary>
    /// <param name="gp">Position to create at.</param>
    /// <param name="color">Rock color.</param>
    /// <param name="resource">Resource resource yeild.</param>
    /// <param name="hardness">Rock hardness.</param>
    /// <param name="_name">Rock name.</param>
    public void CreateRock(GridPos gp, Color color, Resource resource, int hardness, string _name)
    {
        Rock r = Instantiate(
            tilePrefabs.GetPrefab("Dirt"),
            gp.ToVec(ROCK_OFFSET),
            Quaternion.identity,
            MyGrid.FindLevelRocks(gp.y)).GetComponent<Rock>();
        r.rockYield = resource;
        r.Integrity = hardness;
        r.originalIntegrity = hardness;
        r.objectName = _name;
        if (_name == "Dirt")
            r.ColorWithIntegrity();
        else
            r.GetComponent<Renderer>().material.color = color;
        r.UniqueID();
        MyGrid.SetGridItem(gp, r);
    }

    #endregion

    #region Objects
    /// <summary>
    /// Creates a new elevator and marks main based on input.
    /// </summary>
    /// <param name="gp">Position to create at.</param>
    /// <param name="isMain">Mark it as main or not.</param>
    /// <returns></returns>
    public Elevator CreateElevator(GridPos gp, bool isMain = false)
    {
        Elevator el = Instantiate(
            buildPrefabs.GetBuilding("Elevator"),
            gp.ToVec(BUILD_OFFSET),
            Quaternion.identity,
            MyGrid.FindLevelBuildings(gp.y)).GetComponent<Elevator>();
        el.constructed = true;
        if (isMain)
            Elevator.main = el;
        el.isMain = isMain;
        el.objectName = el.objectName.Replace("(Clone)", "");
        MyGrid.SetBuilding(el, true);

        return el;
    }

    /// <summary>
    /// Creates a chunk and fills it with resources.
    /// </summary>
    /// <param name="gp">Position to create at.</param>
    /// <param name="resources">Resource fill.</param>
    /// <param name="updateGlobalResource">Do you want to add the resources to the global resource counter?</param>
    /// <returns></returns>
    public Chunk CreateChunk(GridPos gp, Resource resources, bool updateGlobalResource)
    {
        if (resources.Sum() > 0)
        {
            Building building = MyGrid.GetGridItem(gp) as Building;
            if (building)
                gp = PathFinder.BuildingStep(gp, building.gameObject, -1);

            Chunk chunk = Instantiate(
                specialPrefabs.GetPrefab("Chunk"),
                gp.ToVec(CHUNK_OFFSET),
                Quaternion.identity,
                MyGrid.FindLevelChunks(gp.y)).GetComponent<Chunk>();
            chunk.Init(resources, updateGlobalResource);
            return chunk;
        }
        return null;
    }

    /// <summary>
    /// Creates a Human, assigns a unique ID and hat color.
    /// </summary>
    /// <param name="gp">Position to create at.</param>
    /// <param name="material">Hat material.</param>
    /// <param name="i">Int for name.</param>
    /// <returns></returns>
    public Human CreateHuman(GridPos gp, Material material, int i)
    {
        Human h = (Human)Instantiate(
            specialPrefabs.GetPrefab("Human"),
            gp.ToVec(HUMAN_OFFSET),
            Quaternion.identity,
            SceneRefs.humans.transform.GetChild(0).transform);
        h.UniqueID();
        h.Inventory = new(20);
        // color for debug
        h.transform.GetChild(1).GetComponent<MeshRenderer>().material = material;
        h.objectName = $"Human {(i == 0 ? "Red" : i == 1 ? "Yellow" : "White")}";
        return h;
    }
    #endregion

    #region Loading Game
    /// <summary>Loads a building.</summary>
    public Building CreateSavedBuilding(BSave save)
    {
        GridPos rotate = MyGrid.Rotate(save.blueprint.moveBy, save.rotationY);
        Building b = Instantiate(
            buildPrefabs.GetBuilding(save.categoryID, save.wrapperID),
            new Vector3(save.gridPos.x + rotate.x, (save.gridPos.y * LEVEL_HEIGHT) + BUILD_OFFSET, save.gridPos.z + rotate.z),
            Quaternion.Euler(0, save.rotationY, 0),
            MyGrid.FindLevelBuildings(save.gridPos.y));
        b.Load(save);
        MyGrid.SetBuilding(b, true);
        return b;
    }

    /// <summary>Loads a Rock.</summary>
    public Rock CreateSavedRock(RockSave save, GridPos gp, List<MinableRes> resData)
    {
        Rock rock = Instantiate(
            tilePrefabs.GetPrefab("Dirt"),
            gp.ToVec(ROCK_OFFSET),
            Quaternion.identity,
            MyGrid.FindLevelRocks(gp.y)).GetComponent<Rock>();
        rock.Load(save);
        if(rock.rockYield != null)
            rock.GetComponent<MeshRenderer>().material.color = resData.FirstOrDefault(q => q.name == save.objectName).color;
        else
            rock.ColorWithIntegrity();

    MyGrid.SetGridItem(gp, rock);
        if (rock.toBeDug)
        {
            SceneRefs.jobQueue.toBeDug.Add(rock);
            SceneRefs.gridTiles.HighLight(SceneRefs.gridTiles.toBeDugColor, rock.gameObject);
        }
        return rock;
    }

    /// <summary>Loads a Water.</summary>
    public Water CreateSavedWater(WaterSave save, GridPos gp)
    {
        Water water = Instantiate(
             tilePrefabs.GetPrefab("Water").gameObject,
             gp.ToVec(),
             Quaternion.identity,
             MyGrid.FindLevelWater(gp.y)).GetComponent<Water>();
        water.Load(save);
        MyGrid.SetGridItem(gp, water);
        return water;
    }

    /// <summary>Loads a Human.</summary>
    public Human CreateSavedHuman(HumanSave save)
    {
        int parent = save.workplaceId > -1 ? 1 : 0;
        Human human = Instantiate(
            specialPrefabs.GetPrefab("Human"),
            save.gridPos.ToVec(HUMAN_OFFSET),
            Quaternion.identity,
            SceneRefs.humans.transform.GetChild(parent)).GetComponent<Human>();
        human.Load(save);
        return human;
    }

    #endregion Loading Game
    public IEnumerator Init()
    {
        AsyncOperationHandle<BuildingData> buttons = Addressables.LoadAssetAsync<BuildingData>("Assets/Game Data/Research && Building/Build Data.asset");
        if (!buttons.IsDone)
            yield return buttons;
        buildPrefabs = buttons.Result;
        buildPrefabs.Categories.SelectMany(q => q.Objects).Select(q => q.building).ToList().ForEach(q => q.InitModifiers());
    }
}
