using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
/// <summary>Util class for managment of each different level.</summary>
public class GroundLevel : MonoBehaviour, IUpdatable
{
    #region Variables
    /// <summary>grid witdth(x)</summary>
    [Header("Grid")] 
    public int width = 21;
    /// <summary>grid height(y)</summary>
    public int height = 21;

    /// <summary>grid itself</summary>
    GridTile[,] grid;

    [SerializeField] Stability stability;

    /// <summary>Rock holder</summary>
    [Header("Reference")] public Transform rocks;
    /// <summary>Road holder</summary>
    public Transform roads;
    /// <summary>Water holder</summary>
    public Transform waters;
    /// <summary>Vein holder</summary>
    public Transform veins;
    /// <summary>Chunk holder</summary>
    public Transform chunks;
    /// <summary>Building holder</summary>
    public Transform buildings;
    /// <summary>Pipe holder</summary>
    public Transform pipes;

    /// <summary>If the level is unlocked(has a elevator).</summary>
    bool unlocked;
    [CreateProperty] public bool Unlocked { get => unlocked; private set { unlocked = value; UIUpdate(nameof(Unlocked)); } }

    public Elevator ConnectingElevator { get; private set; }

    public event EventHandler<UnityEngine.UIElements.BindablePropertyChangedEventArgs> propertyChanged;

    #endregion

    public void UIUpdate(string property = "")
    {
        propertyChanged?.Invoke(this, new UnityEngine.UIElements.BindablePropertyChangedEventArgs(property));
    }

#if UNITY_EDITOR
    [SerializeField] bool showIntegrity = true;
    private void OnDrawGizmos()
    {
        if (EditorApplication.isPlaying)
        {
            Vector3Int vec = new Vector3Int(0, (int)transform.position.y + 1, 0);
            for (int i = 0; i < width; i++)
            {
                vec.x = i;
                for (int j = 0; j < height; j++)
                {
                    vec.z = j;
                    if (!showIntegrity)
                        SetGridColor(vec);
                    else
                        SetIntegrityColor(vec);
                }
            }
        }
    }

    void SetGridColor(Vector3Int vec)
    {
        
        switch (grid[vec.x, vec.z].TileBase)
        {
            case Rock:
                Gizmos.color = Color.black;
                break;
            case Building:
                Gizmos.color = Color.darkGreen;
                break;
            case Road:
                Gizmos.color = new(0, 0, 0, 0);
                break;
            case Water:
                Gizmos.color = Color.darkBlue;
                break;
            case Vein:
                Gizmos.color = Color.blanchedAlmond;
                break;
        }
        Gizmos.DrawCube(vec, new(1, 1, 1));
    }
    void SetIntegrityColor(Vector3Int vec)
    {
        Gizmos.color = new(1, 1, 1, 1);
        Handles.Label(vec, grid[vec.x, vec.z].Stability.ToString());
        //Gizmos.DrawSphere(vec, );
    }
#endif
    #region Unlocking
    public void SetUnlocked(Elevator elevator)
    {
        if (Unlocked)
        {
            //Debug.LogError("Already unlocked!");
            return;
        }
        Unlocked = true;
        ConnectingElevator = elevator;
    }
    #endregion


    #region Base Grid operations
    /// <summary>
    /// Returns contents of a tile on the <paramref name="gp"/> position.
    /// </summary>
    /// <param name="gp">Position of interest.</param>
    /// <param name="isPipe">Look into the pipe grid.</param>
    /// <returns>The content of the tile.</returns>
    public ClickableObject GetGridItem(GridPos gp, bool isPipe = false)
    {
        int x = Mathf.RoundToInt(gp.x);
        int y = Mathf.RoundToInt(gp.z);
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogWarning($"(Get)Index outside of grid bounds, x: {x}, y :{y}.");
            return null;
        }

        if (isPipe)
            return grid[x, y].Pipe;
        else
            return grid[x, y].TileBase;
    }

    public bool CheckBounds(int x, int y) => !(x < 0 || x >= width || y < 0 || y >= height);

    /// <summary>
    /// Updates the grid by replacing the content of a tile.
    /// </summary>
    /// <param name="gp">Position of interest.</param>
    /// <param name="clickable">New content.</param>
    /// <param name="isPipe">Change in pipeGrid.</param>
    public void SetGridItem(GridPos gp, ClickableObject clickable, bool isPipe = false)
    {
        int x = Mathf.RoundToInt(gp.x);
        int y = Mathf.RoundToInt(gp.z);
        if (!CheckBounds(x, y))
        {
            Debug.LogError($"(Set)Index outside of grid bounds, x: {x}, y :{y}.");
            return; 
        }

        if (isPipe)
        {
            grid[x, y].Pipe = (Pipe)clickable;
            return;
        }

        switch (clickable)
        {
            case Rock rock:
                if (grid[x, y].TileBase is Road prevRoad)
                    Destroy(prevRoad.gameObject);
                stability.IncereaseStability(rock);
                break;
/*
            case Road road:
                ClickableObject prev = grid[x, y].TileBase;
                if (prev is Rock)
                {
                    UpdateEffects();
                }
                break;*/
        }

        grid[x, y].TileBase = clickable;
        UpdateEffects();

        /*
        if (clickable is not Road)
            return;

// # TODO return?
        foreach (Transform t in overlays.GetImagesOnPos(gp))
        {
            t.gameObject.SetActive(true);
            (clickable as Road).entryPoints.Add(t.parent.GetComponent<GroupOverlay>().building);
            t.localPosition = new(t.localPosition.x, t.localPosition.y, 0);
        }*/
    }

    public bool TestCavein(Rock rock)
    {
        return stability.DecreaseStability(rock);
    }

    public bool ChangeGridStability(int x, int y, int change, bool add)
    {
        if(CheckBounds(x, y))
        {
            if (add)
                grid[x, y].IncreaseStability(change);
            else
                grid[x, y].DecreaseStability(change);

            return true;
        }
        return false;
    }

    

    void UpdateEffects()
    {
        foreach (var item in MyGrid.EffectBuildings)
        {
            item.RecalculateRange();
        }
    }



    


    /// <summary>
    /// Creates a new <see cref="grid"/> and <see cref="pipeGrid"/>.
    /// </summary>
    /// <param name="gridSize">Size for the new grid.</param>
    public void ClearGrid(int gridSize = -1)
    {
        if (gridSize > -1)
        {
            width = gridSize;
            height = gridSize;
        }

        grid = new GridTile[width, height];
    }
    #endregion Base Grid operations

    #region Adding to Grid
    /// <summary>
    /// Used for placing the building when ordering construction, or loading a level. <br/>
    /// Registers the building and updates entry points.
    /// </summary>
    /// <param name="building">Building thats being placed.</param>
    /// <param name="gridPos">building anchor position.</param>
    /// <param name="load">If load is true creates, creates new roads and doesn't recycle entrypoints.</param>
    public void RegisterBuilding(Building building, GridPos gridPos, bool load = false)
    {
        MyGrid.Buildings.Add(building);
        if(building is Pub pub)
        {
            MyGrid.EffectBuildings.Add(pub);
        }
        RectTransform overlayGroup = SceneRefs.Overlays
            .entryPoints.Create(building);
        List<EntryTile> entryTiles = new();

        for (int i = building.blueprint.itemList.Count - 1; i > -1; i--)
        {
            NeededGridItem item = building.blueprint.itemList[i];
            GridPos itemPos = item.pos.Rotate(building.transform.rotation.eulerAngles.y, true);
            int x = (int)(itemPos.x + gridPos.x);
            int y = (int)(gridPos.z - itemPos.z);
            GridPos worldPos = new(x, y);
            Road road = GetGridItem(worldPos) as Road;

            
            switch (item.itemType)
            {
                case GridItemType.Road:
                case GridItemType.Anchor:
                case GridItemType.Pipe:
                    SceneRefs.Overlays.entryPoints.ToggleEntryPoints(road, false);
                    SetGridItem(worldPos, building);
                    break;
                case GridItemType.Entrance:
                    bool isActive = false;
                    if (road != null)
                    {
                        road.entryPoints.Add(building);
                        road.RegisterEffects(building);
                        isActive = true;
                    }

                    entryTiles.Add(new(worldPos, isActive));
                    if (load)
                    {
                        SceneRefs.Overlays.entryPoints.AddNew(
                            new(itemPos.x, itemPos.z),
                            overlayGroup);
                    }
                    else
                    {
                        SceneRefs.Overlays.entryPoints.AddFromIndicator(
                            overlayGroup,
                            i);
                    }
                    break;
                case GridItemType.WaterSource:
                case GridItemType.ResourceSource:
                    if((building as NeedSourceProduction).Source == null)
                        (building as NeedSourceProduction).Source = GetGridItem(new(x, y)) as TileSource;
                    break;
            }
        }
        building.entryPoints = new(entryTiles, overlayGroup);
        if (!load)
            SceneRefs.Overlays.blueprintIndicator.DestroyBuilingTiles();
    }
    #endregion Adding to Grid

    #region Removing from Grid
    /// <summary>
    /// Unregisters the building and entrypoints.
    /// </summary>
    /// <param name="building">Building being removed.</param>
    /// <param name="gridPos">Building position</param>
    public void UnsetBuilding(Building building, GridPos gridPos)
    {
        SceneRefs.Overlays.entryPoints.Remove(building);
        List<Road> _roads = roads.GetComponentsInChildren<Road>().ToList();
        for (int i = building.blueprint.itemList.Count - 1; i > -1; i--)
        {
            NeededGridItem item = building.blueprint.itemList[i];
            GridPos itemPos = item.pos.Rotate(building.transform.rotation.eulerAngles.y, true);
            int x = (int)(itemPos.x + gridPos.x);
            int y = (int)(gridPos.z - itemPos.z);

            switch (item.itemType)
            {
                case GridItemType.Road:
                case GridItemType.Anchor:
                case GridItemType.Pipe:
                    Road r = _roads.FirstOrDefault(q => q.GetPos().Equals(new GridPos(x, gridPos.y, y)));
                    r.entryPoints = new();
                    SetGridItem(new(x, y), r);
                    break;
            }
        }
    }
    #endregion Removing from Grid

    #region Game initialization

    /// <summary>
    /// Registers all parts of the grid.
    /// </summary>
    /// <param name="gridSize">Size of the grid.</param>
    public void CreateGrid(WorldSave save, int level)
    {
        GridSave grid = new(width, height);
        save.gridSave[level] = grid;//= new ClickableObjectSave[width, height];
        CreateRocks(save.gridSave[level]); // adds ores
        CreateWater(save.gridSave[level]); // adds water
        CreateVeins(save, level);
        CreateBuildings(save, level); // adds Buildings
        gameObject.SetActive(false);
    }


    #region specific Creations
    /// <summary>Registers all instantiated roads.</summary>
    /*void FillRoads()
    {
        for (int j = 0; j < roads.childCount; j++)
        {
            GridPos vec = new(roads.GetChild(j).transform.localPosition);
            SetGridItem(vec, roads.GetChild(j).GetComponent<Road>());
        }
    }*/

    /// <summary>Registers all instantiated rocks.</summary>
    void CreateRocks(GridSave save)
    {
        for (int j = 0; j < rocks.childCount; j++)
        {
            Rock rock = rocks.GetChild(j).GetComponent<Rock>();
            GridPos vec = rock.GetPos();
            rock.id = j + 1;
            var rSave = rock.Save() as RockSave;
            rSave.originalIntegrity = rSave.integrity;
            save.grid[Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.z)] = rSave;
        }
    }

    /// <summary>Registers all instantiated sources.</summary>
    void CreateWater(GridSave save)
    {
        for (int j = 0; j < waters.childCount; j++)
        {
            Water water = waters.GetChild(j).GetComponent<Water>();
            water.id = j + 1;
            GridPos vec = water.GetPos();
            save.grid[Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.z)] = water.Save();
        }
    }

    private void CreateVeins(WorldSave save, int level)
    {
        int idCounter = save.objectsSave.veins.Length + 1;
        List<VeinSave> veinList = veins.GetComponentsInChildren<Vein>().Select(vein =>
        {
            GridPos vec = vein.GetPos();
            int x = Mathf.FloorToInt(vec.x);
            int z = Mathf.FloorToInt(vec.z);
            vein.id = idCounter++;
            for (int j = 0; j < vein.xSize; j++)
                for (int k = 0; k < vein.zSize; k++)
                    save.gridSave[level].grid[x + j, z + k] = new() { id = -1 };
            return vein.Save() as VeinSave;
        }).ToList();
        veinList.AddRange(save.objectsSave.veins);
        save.objectsSave.veins = veinList.ToArray();
    }

    /// <summary>Registers all instantiated buildings.</summary>
    void CreateBuildings(WorldSave save, int level)
    {
        List<BuildingSave> buildingList = buildings.GetComponentsInChildren<Building>().Select(building =>
        {
            if (!building.IsWorking)
                building.maximalProgress = building.CalculateMaxProgress();

            if (building is IStorage storage)
                storage.SetupStorage(50);

            BuildingSave bSave = building.Save() as BuildingSave;
            bSave.gridPos.y = level;
            return bSave;
        }).ToList();
        buildingList.AddRange(save.objectsSave.buildings);
        save.objectsSave.buildings = buildingList.ToArray();
    }

    public GridTile[,] GetGrid()
    {
        return grid;
    }

    public GridTile GetGridTile(int x, int z)
    {
        return grid[x, z];
    }

    #endregion

    #endregion Creation

}
