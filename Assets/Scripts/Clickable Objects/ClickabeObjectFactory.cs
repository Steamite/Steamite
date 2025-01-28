using System;
using System.Linq;
using UnityEngine;

/// <summary>Instantiates and fills new Clickable Objects.</summary>
public class ClickabeObjectFactory : MonoBehaviour
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
    public ResourceHolder buildPrefabs;
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

        replacement.name = replacement.name.Replace("(Clone)", "");
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
        r.GetComponent<Renderer>().material.color = color;
        r.rockYield = resource;
        r.Integrity = hardness;
        r.name = _name;
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
            buildPrefabs.GetPrefab("Elevator"), 
            gp.ToVec(BUILD_OFFSET), 
            Quaternion.identity, 
            MyGrid.FindLevelBuildings(gp.y)).GetComponent<Elevator>();
        el.constructed = true;
        el.main = isMain;
        el.name = el.name.Replace("(Clone)", "");
        MyGrid.SetBuilding(el, true);

        return el;
    }

    /// <summary>
    /// Creates a chunk and fills it with resources.
    /// </summary>
    /// <param name="gp">Position to create at.</param>
    /// <param name="resources">Resource fill.</param>
    /// <returns></returns>
    public Chunk CreateAChunk(GridPos gp, Resource resources)
    {
        if(resources.ammount.Sum() > 0)
        {
            Chunk chunk = Instantiate(
                specialPrefabs.GetPrefab("Chunk"), 
                gp.ToVec(CHUNK_OFFSET), 
                Quaternion.identity, 
                MyGrid.FindLevelChunks(gp.y)).GetComponent<Chunk>();
            chunk.Init(resources);
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
    public Human CreateAHuman(GridPos gp, Material material, int i)
    {
        Human h = (Human)Instantiate(
            specialPrefabs.GetPrefab("Human"), 
            gp.ToVec(HUMAN_OFFSET), 
            Quaternion.identity, 
            SceneRefs.humans.transform.GetChild(0).transform);
        h.UniqueID();
        // color for debug
        h.transform.GetChild(1).GetComponent<MeshRenderer>().material = material;
        h.gameObject.name = $"Human {(i == 0 ? "Red" : i == 1 ? "Yellow" : "White")}";
        return h;
    }
    #endregion

    #region Loading Game
    /// <summary>Loads a building.</summary>
    public Building CreateSavedBuilding(BSave save)
    {
        Building _pref = buildPrefabs.GetPrefab(save.prefabName) as Building; // find the prefab

        GridPos rotate = MyGrid.Rotate(save.blueprint.moveBy, transform.rotation.eulerAngles.y);
        Building b = Instantiate(_pref,
            new Vector3(save.gridPos.x + rotate.x, (save.gridPos.y * LEVEL_HEIGHT) + BUILD_OFFSET, save.gridPos.z + rotate.z),
            Quaternion.Euler(0, save.rotationY, 0),
            MyGrid.FindLevelBuildings(save.gridPos.y));
        b.Load(save);
        MyGrid.SetBuilding(b, true);
        return b;
    }

    /// <summary>Loads a Rock.</summary>
    public Rock CreateSavedRock(RockSave save, GridPos gp)
    {
        Rock rock = Instantiate(
            tilePrefabs.GetPrefab(save.oreName), 
            gp.ToVec(ROCK_OFFSET), 
            Quaternion.identity, 
            MyGrid.FindLevelRocks(gp.y)).GetComponent<Rock>();
        rock.Load(save);

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
        human.transform.GetChild(1).GetComponent<MeshRenderer>().material.color = save.color.ConvertColor();
        human.id = save.id;
        human.SetJob(new(save.jobSave, human));
        human.name = save.name;
        MyRes.ManageRes(human.Inventory, save.inventory, 1);
        human.specialization = save.specs;
        if (human.Job.path.Count > 0)
            human.ChangeAction(HumanActions.Move);
        else
            human.Decide();
        // house assigment
        if (save.houseID != -1)
            MyGrid.buildings.Where(q => q.id == save.houseID).
                SingleOrDefault().GetComponent<House>().ManageAssigned(human, true);

        // workplace assigment
        if (save.workplaceId != -1)
            MyGrid.buildings.Where(q => q.id == save.workplaceId).
                SingleOrDefault().GetComponent<ProductionBuilding>().ManageAssigned(human, true);
        return human;
    }
    #endregion Loading Game
}
