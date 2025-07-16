using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>Util class for managment of each different level.</summary>
public class GroundLevel : MonoBehaviour, IUpdatable
{
    #region Variables
    /// <summary>grid witdth(x)</summary>
    [Header("Grid")] public int width = 21;
    /// <summary>grid height(y)</summary>
    public int height = 21;

    /// <summary>grid itself</summary>
    ClickableObject[,] grid;
    /// <summary>pipes on the grid</summary>
    Pipe[,] pipeGrid;

    /// <summary>Rock holder</summary>
    [Header("Reference")] public Transform rocks;
    /// <summary>Road holder</summary>
    public Transform roads;
    /// <summary>Water holder</summary>
    public Transform waters;
    /// <summary>Chunk holder</summary>
    public Transform chunks;
    /// <summary>Building holder</summary>
    public Transform buildings;
    /// <summary>Pipe holder</summary>
    public Transform pipes;

    /// <summary>Entrypoint overlay</summary>
    public UIOverlay overlays;

    /// <summary>If the level is unlocked(has a elevator).</summary>
    bool unlocked;
    [CreateProperty] public bool Unlocked { get => unlocked; set { unlocked = value; UIUpdate(nameof(Unlocked)); } }

    public event EventHandler<UnityEngine.UIElements.BindablePropertyChangedEventArgs> propertyChanged;

    #endregion

    public void UIUpdate(string property = "")
    {
        propertyChanged?.Invoke(this, new UnityEngine.UIElements.BindablePropertyChangedEventArgs(property));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (EditorApplication.isPlaying)
        {
            Vector3 vec = new Vector3(0, transform.position.y + 1, 0);
            for (int i = 0; i < width; i++)
            {
                vec.x = i;
                for (int j = 0; j < height; j++)
                {
                    vec.z = j;
                    switch (grid[i, j])
                    {
                        case Rock:
                            Gizmos.color = Color.black;
                            break;
                        case Building:
                            Gizmos.color = Color.darkGreen;
                            break;
                        case Road:
                            continue;
                        case Water:
                            Gizmos.color = Color.darkBlue;
                            break;
                    }
                    Gizmos.DrawCube(vec, new(1, 1, 1));
                }
            }
        }
    }
#endif

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
            return pipeGrid[x, y];
        else
            return grid[x, y];
    }

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
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogError($"(Set)Index outside of grid bounds, x: {x}, y :{y}.");
            return;
        }
        if (clickable?.GetType() == typeof(Road))
        {
            foreach (Transform tran in overlays.buildingOverlays)
            {
                for (int i = 0; i < tran.childCount; i++)
                {
                    Transform t = tran.GetChild(i);
                    if (t.gameObject.activeSelf == false && new GridPos(t.position).Equals(gp))
                    {
                        t.gameObject.SetActive(true);
                        (clickable as Road).entryPoints.Add(int.Parse(t.parent.name));
                        t.localPosition = new(t.localPosition.x, t.localPosition.y, 0);
                    }
                }
            }
        }
        if (isPipe)
        {
            if (pipeGrid[x, y] == null)
                pipeGrid[x, y] = (Pipe)clickable;
        }
        else
            grid[x, y] = clickable;
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

        grid = new ClickableObject[width, height];
        pipeGrid = new Pipe[width, height];
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
    public void RegisterBuilding(Building building, GridPos gridPos = null, bool load = false)
    {
        MyGrid.Buildings.Add(building);
        if (gridPos == null)
            gridPos = building.GetPos();
        overlays.AddBuildingOverlay(gridPos, building.id);

        List<Image> tiles = overlays.buildingOverlays[^1].GetComponentsInChildren<Image>().ToList();
        for (int i = building.blueprint.itemList.Count - 1; i > -1; i--)
        {
            NeededGridItem item = building.blueprint.itemList[i];
            GridPos itemPos = MyGrid.Rotate(item.pos, building.transform.rotation.eulerAngles.y, true);
            int x = (int)(itemPos.x + gridPos.x);
            int y = (int)(gridPos.z - itemPos.z);
            Road r = GetGridItem(new(x, y))?.GetComponent<Road>();
            switch (item.itemType)
            {
                case GridItemType.Road:
                case GridItemType.Anchor:
                case GridItemType.Pipe:
                    overlays.ToggleEntryPoints(r);
                    SetGridItem(new(x, y), building);
                    break;
                case GridItemType.Entrance:
                    overlays.Add(new(itemPos.x, itemPos.z), load ? -1 : i);
                    r?.entryPoints.Add(building.id);
                    break;
                case GridItemType.Water:
                    (building as WaterPump).waterSource = GetGridItem(new(x, y)) as Water;
                    break;
            }
        }
        if (!load)
            overlays.DestroyBuilingTiles();
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
        overlays.Remove(building.id, gridPos.y);
        List<Road> _roads = roads.GetComponentsInChildren<Road>().ToList();
        for (int i = building.blueprint.itemList.Count - 1; i > -1; i--)
        {
            NeededGridItem item = building.blueprint.itemList[i];
            GridPos itemPos = MyGrid.Rotate(item.pos, building.transform.rotation.eulerAngles.y, true);
            int x = (int)(itemPos.x + gridPos.x);
            int y = (int)(gridPos.z - itemPos.z);

            switch (item.itemType)
            {
                case GridItemType.Road:
                case GridItemType.Anchor:
                    Road r = _roads.FirstOrDefault(q => q.GetPos().Equals(new GridPos(x, gridPos.y, y)));
                    r.entryPoints = new();
                    SetGridItem(new(x, y), r);
                    break;
            }
        }
    }
    #endregion Removing from Grid

    #region Checks
    /// <summary>
    /// Checks if a pipe can be placed on the <paramref name="pos"/> position. <br/>
    /// Must not be placed over a different pipe.
    /// </summary>
    /// <param name="pipe">Pipe to place (used here for visual effects)</param>
    /// <param name="pos">Position to check if available.</param>
    /// <returns>If it's ok to build there or not.</returns>
    public bool CanPlace(Pipe pipe, GridPos pos)
    {
        bool canPlace = pipeGrid[(int)pos.x, (int)pos.z] == null && GetGridItem(pos) is Road;
        pipe.FindConnections(canPlace);
        return canPlace;
    }

    /// <summary>
    /// Checks if a <see cref="Building"/> can be placed at <paramref name="gridPos"/>. <br/>
    /// Iterates though all tiles in blueprint and marks their state.
    /// </summary>
    /// <param name="building">Building that's being placed.</param>
    /// <param name="gridPos">Anchor position</param>
    /// <returns>If it's ok to build there or not.</returns>
    public bool CanPlace(Building building, GridPos gridPos)
    {
        bool canBuild = true;
        overlays.MovePlaceOverlay(building);
        // checks all Parts of a building
        Transform overlay = overlays.overlayParent;
        List<Road> foreignObscuredRoads = new();
        List<Image> foreignEntryOverlay = new();
        List<Image> entrances = new();
        int activeEntrances = -1;
        for (int i = 0; i < building.blueprint.itemList.Count; i++)
        {
            NeededGridItem item = building.blueprint.itemList[i];
            GridPos itemPos = MyGrid.Rotate(item.pos, building.transform.rotation.eulerAngles.y, true);
            itemPos.x += gridPos.x;
            itemPos.z = gridPos.z - itemPos.z;
            Transform tile = overlay.GetChild(i);
            Color errC, c;
            switch (item.itemType)
            {
                case GridItemType.Road:
                    c = new(0, 1, 0, 0.25f);
                    errC = new(1, 0, 0, 0.25f);
                    CheckMassObscursion(
                        itemPos,
                        tile.GetComponent<Image>(),
                        c,
                        errC,
                        ref canBuild,
                        foreignObscuredRoads,
                        foreignEntryOverlay);
                    break;
                case GridItemType.Anchor:
                    c = new(1, 0.843f, 0, 0.25f);
                    errC = new(1, 0.643f, 0, 0.25f);
                    CheckMassObscursion(
                        itemPos,
                        tile.GetComponent<Image>(),
                        c,
                        errC,
                        ref canBuild,
                        foreignObscuredRoads,
                        foreignEntryOverlay);
                    break;
                case GridItemType.Entrance:
                    entrances.Add(tile.GetComponent<Image>());
                    if (GetGridItem(itemPos) is Road)
                        activeEntrances++;
                    break;
                case GridItemType.Water:
                    c = new(0.211765f, 0.1686275f, 1, 0.25f);
                    errC = new(0.8f, 0.2196079f, 1, 0.25f);
                    CheckWaterObscursion(
                        itemPos,
                        tile.GetComponent<Image>(),
                        c,
                        errC,
                        ref canBuild);
                    break;
                case GridItemType.Pipe:
                    c = new(1f, 0.5490196f, 0f, 0.25f);
                    errC = new(1, 0, 0, 0.25f);
                    CheckMassObscursion(
                        itemPos,
                        tile.GetComponent<Image>(),
                        c,
                        errC,
                        ref canBuild,
                        foreignObscuredRoads,
                        foreignEntryOverlay);
                    break;
                default:
                    continue;
            }
            // Move the tile up or down
            ClickableObject clickableObject = GetGridItem(itemPos);
            if (clickableObject is Rock)
                tile.localPosition = new(tile.localPosition.x, tile.localPosition.y, 2.01f);
            else
                tile.localPosition = new(tile.localPosition.x, tile.localPosition.y, 0);

        }

        if (!CheckEntranceObscursion(foreignObscuredRoads, foreignEntryOverlay))
            canBuild = false;
        foreach (Image entrance in entrances)
        {
            if (activeEntrances == -1)
            {
                entrance.color = new(1f, 0.3f, 0.3f, 0.25f);
                canBuild = false;
            }
            else
                entrance.color = new(0.5f, 0.5f, 0.5f, 0.25f);
        }
        return canBuild;
    }

    void CheckMassObscursion(GridPos pos, Image image, Color baseColor, Color errColor, ref bool canBuild, List<Road> _roads, List<Image> images)
    {
        ClickableObject clickable = GetGridItem(pos);
        if (clickable != null && clickable is Road)
        {
            if(GetGridItem(pos, true) == null)
            {
                Road road = clickable as Road;
                if (road.entryPoints.Count > 0)
                {
                    _roads.Add(road);
                    images.Add(image);
                }
                image.color = baseColor;
                return;
            }
        }
        image.color = errColor;
        canBuild = false;
    }


    void CheckWaterObscursion(GridPos pos, Image image, Color baseColor, Color errColor, ref bool canBuild)
    {
        ClickableObject clickable = GetGridItem(pos);
        if (clickable != null && clickable is Water)
        {
            image.color = baseColor;
        }
        else
        {
            image.color = errColor;
            canBuild = false;
        }
    }

    /// <summary>
    /// Checks if the current building is not obscurring the last entry point of another building.
    /// </summary>
    /// <param name="roads">Road tiles that the building is occupying.</param>
    /// <param name="tiles">All building tiles to mark the states.</param>
    /// <returns>If it's ok to build there or not.</returns>
    bool CheckEntranceObscursion(List<Road> roads, List<Image> tiles)
    {
        Dictionary<int, int> buildings = new();
        bool ok = true;

        foreach (Road road in roads)
        {
            foreach (int i in road.entryPoints)
            {
                    buildings.TryAdd(i, overlays.buildingOverlays.First(q => q.name == i.ToString()).GetComponentsInChildren<Image>().Where(q => q.enabled).Count());
                buildings[i]--;
                if (buildings[i] == 0)
                {
                    List<int> ids = new();
                    for (int j = 0; j < roads.Count; j++)
                    {
                        if (roads[j].entryPoints.Contains(i))
                        {
                            //entries[j].gameObject.SetActive(false);
                            tiles[j].color = new(1, 0, 0, 0.5f);
                            ok = false;
                        }
                    }
                }
            }
        }
        return ok;
    }
    #endregion Checks

    #region Game initialization

    /// <summary>
    /// Registers all parts of the grid.
    /// </summary>
    /// <param name="gridSize">Size of the grid.</param>
    public void CreateGrid(WorldSave save, int level)
    {
        bool _unlocked = false;
        for (int i = 0; i < buildings.childCount; i++)
        {
            if (buildings.GetChild(i).GetComponent<Elevator>())
            {
                _unlocked = true;
                break;
            }
        }

        GridSave grid = new(width, height, _unlocked);
        save.gridSave[level] = grid;//= new ClickableObjectSave[width, height];
        FillRocks(save.gridSave[level]); // adds ores
        FillWater(save.gridSave[level]); // adds water
        FillBuildings(save, level); // adds Buildings and (entry points)

        gameObject.SetActive(false);
    }

    #region specific Creations
    /// <summary>Registers all instantiated roads.</summary>
    void FillRoads()
    {
        for (int j = 0; j < roads.childCount; j++)
        {
            GridPos vec = new(roads.GetChild(j).transform.localPosition);
            SetGridItem(vec, roads.GetChild(j).GetComponent<Road>());
        }
    }

    /// <summary>Registers all instantiated rocks.</summary>
    void FillRocks(GridSave save)
    {
        for (int j = 0; j < rocks.childCount; j++)
        {
            Rock rock = rocks.GetChild(j).GetComponent<Rock>();
            GridPos vec = rock.GetPos();
            rock.UniqueID();
            save.grid[Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.z)] = rock.Save();
        }
    }

    /// <summary>Registers all instantiated waters.</summary>
    void FillWater(GridSave save)
    {
        for (int j = 0; j < waters.childCount; j++)
        {
            Water water = waters.GetChild(j).GetComponent<Water>();
            GridPos vec = water.GetPos();
            save.grid[Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.z)] = water.Save();
        }
    }

    /// <summary>Registers all instantiated buildings.</summary>
    void FillBuildings(WorldSave save, int level)
    {
        List<BSave> buildingList = buildings.GetComponentsInChildren<Building>().Select(q =>
        {
            if (!q.constructed)
                q.maximalProgress = q.CalculateMaxProgress();
            if (q is IStorage)
                ((IStorage)q).SetupStorage();

            BSave bSave = q.Save() as BSave;
            bSave.gridPos.y = level;
            return bSave;
        }).ToList();
        buildingList.AddRange(save.objectsSave.buildings);
        save.objectsSave.buildings = buildingList.ToArray();
    }

    #endregion

    #endregion Creation

}
