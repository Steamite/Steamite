using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>Instantiates and fills new Clickable Objects.</summary>
public class ClickableObjectFactory : MonoBehaviour, IBeforeLoad
{
    #region Y grid offset
    /// <summary>Height of each level.</summary>
    public const int LEVEL_HEIGHT = 2;

    public const int BUILD_OFFSET = 1;
    public const int PIPE_OFFSET = 2;
    public const int CHUNK_OFFSET = 1;

    public const float HUMAN_OFFSET = 0.47f;
    public const float ROCK_OFFSET = 0.5f;
    public const float ROAD_OFFSET = 0.45f;
    #endregion

    #region Prefabs
    public BuildingData buildPrefabs;
    public PrefabHolder tilePrefabs;
    public PrefabHolder specialPrefabs;
    public PipePart PipeConnectionPrefab;

    public ProductionRecipeHolder recipeData;
    #endregion
    Material pipeMaterial;
    public List<int> CenterElevatorIds { get; private set; }


    #region Tiles
    /// <summary>
    /// Creates a new <see cref="Road"/> and registers it on Grid.
    /// </summary>
    /// <param name="gp">Position to create at.</param>
    /// <param name="doSet">If the road should be registered.</param>
    public Road CreateRoad(GridPos gp, bool loading = false)
    {
        Road replacement = Instantiate(
            tilePrefabs.GetPrefab<Road>("Road"),
            gp.ToVec(ROAD_OFFSET),
            Quaternion.identity,
            MyGrid.FindLevelRoads(gp.y)); // creates a road on the place of tiles

        replacement.objectName = replacement.objectName.Replace("(Clone)", "");
        MyGrid.SetGridItem(gp, replacement);
        if (loading)
        {
            replacement.RevealRocks();
        }
        return replacement;
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
            tilePrefabs.GetPrefab<Rock>("Dirt"),
            gp.ToVec(ROCK_OFFSET),
            Quaternion.identity,
            MyGrid.FindLevelRocks(gp.y));
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
    public Elevator CreateElevator(GridPos gp, int rotation = 0)
    {
        Elevator el = Instantiate(
            buildPrefabs.GetBuilding("Elevator"),
            gp.ToVec(BUILD_OFFSET),
            Quaternion.Euler(0, rotation, 0),
            MyGrid.FindLevelBuildings(gp.y)).GetComponent<Elevator>();
        el.constructed = true;
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
                specialPrefabs.GetPrefab<Chunk>("Chunk"),
                gp.ToVec(CHUNK_OFFSET),
                Quaternion.identity,
                MyGrid.FindLevelChunks(gp.y));
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
        Human h = Instantiate(
            specialPrefabs.GetPrefab<Human>("Human"),
            gp.ToVec(HUMAN_OFFSET),
            Quaternion.identity,
            SceneRefs.Humans.transform.GetChild(0).transform);
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
    public void CreateSavedBuilding(BuildingSave save)
    {
        GridPos rotate = MyGrid.Rotate(save.blueprint.moveBy, save.rotationY);
        Building b = Instantiate(
            buildPrefabs.GetObjectBySaveIndex(save.prefabConnection).building,
            new Vector3(save.gridPos.x + rotate.x, (save.gridPos.y * LEVEL_HEIGHT) + BUILD_OFFSET, save.gridPos.z + rotate.z),
            Quaternion.Euler(0, save.rotationY, 0),
            MyGrid.FindLevelBuildings(save.gridPos.y));
        b.Load(save);

        if (b is IFluidWork fluid)
        {
            fluid.CreatePipes(true);
        }
        if (b is IResourceProduction prod)
        {
            prod.Init(b.constructed, recipeData);
        }


        MyGrid.SetBuilding(b, true);
    }

    /// <summary>Loads a Rock.</summary>
    public Rock CreateSavedRock(RockSave save, GridPos gp, List<MinableRes> resData, Material dirtMat)
    {
        string prefName = "Dirt";
        if(save.yeild.types.Count > 0)
            prefName = "Ore";

        Rock rock = Instantiate(
            tilePrefabs.GetPrefab<Rock>(prefName),
            gp.ToVec(ROCK_OFFSET),
            Quaternion.identity,
            MyGrid.FindLevelRocks(gp.y));

        rock.Load(save);
        if (rock.rockYield != null && rock.rockYield.Sum() > 0)
        {
            rock.Name = save.objectName;
            MinableRes res = resData.FirstOrDefault(q => q.name == rock.Name);
            if (res == null)
                res = resData.FirstOrDefault(q => q.name == rock.rockYield.types[rock.rockYield.ammounts.IndexOf(rock.rockYield.ammounts.Max())].ToString());
            rock.GetComponent<MeshRenderer>().materials[0].SetColor("_Normal_Color", res.color);
        }
        else
            rock.ColorWithIntegrity();

        MyGrid.SetGridItem(gp, rock);
        if (rock.toBeDug)
        {
            SceneRefs.JobQueue.toBeDug.Add(rock);
            rock.Highlight(SceneRefs.GridTiles.toBeDugColor);
        }
        return rock;
    }

    /// <summary>Loads a Water.</summary>
    public void CreateSavedWater(WaterSave save, GridPos gp)
    {
        Water newSource = Instantiate(
             tilePrefabs.GetPrefab<Water>("Water"),
             gp.ToVec(ROAD_OFFSET),
             Quaternion.identity,
             MyGrid.FindLevelWater(gp.y));
        newSource.Load(save);
        MyGrid.SetGridItem(gp, newSource);
    }

    public void CreateSavedVein(VeinSave save)
    {
        Vein vein = Instantiate(
            tilePrefabs.GetPrefab<Vein>("Vein"),
            save.gridPos.ToVec(save.sizeX / 4f, ROCK_OFFSET, save.sizeZ / 4f),
            Quaternion.identity,
            MyGrid.FindLevelVeins(save.gridPos.y));
        vein.Load(save);
        int posX = Mathf.FloorToInt(save.gridPos.x);
        int posZ = Mathf.FloorToInt(save.gridPos.z);
        for (int x = 0; x < vein.xSize; x++)
        {
            for (int z = 0; z < vein.zSize; z++)
            {
                MyGrid.SetGridItem(new(posX + x, save.gridPos.y, posZ + z), vein);
            }
        }
    }

    /// <summary>Loads a Human.</summary>
    public Human CreateSavedHuman(HumanSave save)
    {
        int parent = save.workplaceId > -1 ? 1 : 0;
        Human human = Instantiate(
            specialPrefabs.GetPrefab<Human>("Human"),
            save.gridPos.ToVec(HUMAN_OFFSET),
            Quaternion.Euler(0, save.rotation, 0),
            SceneRefs.Humans.transform.GetChild(parent));
        human.Load(save);
        return human;
    }

    public void CreateSavedPipe(ClickableObjectSave save, GridPos pos)
    {
        if (save == null)
            return;

        Pipe pipe = Instantiate(
            buildPrefabs.GetPipe(),
            pos.ToVec(PIPE_OFFSET),
            Quaternion.identity,
            MyGrid.FindLevelPipes(pos.y));
        pipe.Load(save);
    }

    public BuildPipe CreateBuildingPipe(GridPos localPos, IFluidWork building)
    {
        BuildPipe pipe = Instantiate(
            buildPrefabs.GetBuilding(BuildPipe.BUILD_PIPE_PREF_NAME) as BuildPipe,
            localPos.ToVec(PIPE_OFFSET),
            Quaternion.identity,
            ((Building)building).transform);
        pipe.RecalculatePipeTransform();
        pipe.connectedBuilding = building;
        pipe.UniqueID();
        return pipe;
    }
    #endregion Loading Game

    public async Task BeforeInit()
    {
        CenterElevatorIds = new();
        buildPrefabs = await Addressables.LoadAssetAsync<BuildingData>(BuildingData.PATH).Task;
        buildPrefabs.Init();
        recipeData = await Addressables.LoadAssetAsync<ProductionRecipeHolder>(ProductionRecipeHolder.PATH).Task;
        recipeData.Init();
        pipeMaterial = buildPrefabs.GetBuilding(BuildPipe.BUILD_PIPE_PREF_NAME).GetComponent<Renderer>().sharedMaterial;
    }

    public List<Material> GetModelMaterials(Building building)
    {
        return buildPrefabs.GetCategByID(building.prefabConnection.categoryId).Objects.FirstOrDefault(q => q.id == building.prefabConnection.objectId).materials;
    }

    public Material GetPipeMaterial()
    {
        return pipeMaterial;
    }

    public void CreateObjectUnderRock(Rock rock)
    {
        if (rock.hiddenSave.assignedType == HiddenType.Nothing)
            CreateRoad(rock.GetPos(), true);
        else
        {
            switch (rock.hiddenSave.assignedType)
            {
                case HiddenType.Water:
                    CreateSavedWater(new(
                        rock.hiddenSave.ammount),
                        rock.GetPos());

                    break;
                default:
                    Debug.LogWarning("Don't know what to do!!!");
                    break;
            }
        }
          
    }
}
