using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Util class for managment of each different level.</summary>
public class GroundLevel : MonoBehaviour
{
    #region Variables
    /// <summary>grid witdth(x)</summary>
    [Header("Grid")]public int width = 21;
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
    public Transform water;
    /// <summary>Chunk holder</summary>
    public Transform chunks;
    /// <summary>Building holder</summary>
    public Transform buildings;
    /// <summary>Pipe holder</summary>
    public Transform pipes;

    /// <summary>Entrypoint overlay</summary>
    public UIOverlay overlays;

    /// <summary>If the level is unlocked(has a elevator).</summary>
    public bool unlocked;
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
            Debug.LogError($"(Get)Index outside of grid bounds, x: {x}, y :{y}.");
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
        if (clickable.GetType() == typeof(Road))
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
            pipeGrid[x, y] = (Pipe)clickable;
        else
            grid[x, y] = clickable;
    }

    /// <summary>
    /// Creates a new <see cref="grid"/> and <see cref="pipeGrid"/>.
    /// </summary>
    /// <param name="gridSize">Size for the new grid.</param>
    public void ClearGrid(int gridSize = -1)
    {
        if(gridSize > -1)
        {
            width = gridSize;
            height = gridSize;
        }

        grid = new ClickableObject[width, height];
        pipeGrid = new Pipe[width, height];
    }
    #endregion Base Grid operations

    #region Creation

    /// <summary>
    /// Registers all parts of the grid.
    /// </summary>
    /// <param name="gridSize">Size of the grid.</param>
    public void CreateGrid(int gridSize = -1)
    {
        ClearGrid(gridSize);
        
        FillRoads(); // adds roads
        FillRocks(); // adds ores
        FillWater(); // adds water
        FillBuildings(); // adds Buildings and (entry points)

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
    void FillRocks()
    {
        for (int j = 0; j < rocks.childCount; j++)
        {
            GridPos vec = new(rocks.GetChild(j).transform.localPosition);
            Rock rock = rocks.GetChild(j).GetComponent<Rock>();
            SetGridItem(vec, rock);
            rock.UniqueID();
            if (rock.rockYield.ammount.Sum() == 0)
            {
                rock.ColorWithIntegrity();
            }
        }
    }

    /// <summary>Registers all instantiated waters.</summary>
    void FillWater()
    {
        for (int j = 0; j < water.childCount; j++)
        {
            GridPos vec = new(water.GetChild(j).transform.localPosition);
            SetGridItem(vec, water.GetChild(j).GetComponent<Water>());
        }
    }

    /// <summary>Registers all instantiated buildings.</summary>
    void FillBuildings()
    {
        foreach (Building building in buildings.GetComponentsInChildren<Building>())
        {
            building.UniqueID();
            if (!building.GetComponent<BuildPipe>())
            {
                PlaceBuild(building, load: true);
                if (building.GetComponent<ProductionBuilding>() && building.constructed)
                {
                    building.GetComponent<ProductionBuilding>().RefreshStatus();
                    building.GetComponent<ProductionBuilding>().RequestRestock();
                }
            }
            else
                building.GetComponent<BuildPipe>().PlacePipe();
        }
    }
    #endregion

    #endregion Creation

    #region Adding to Grid
    /// <summary>
    /// Used for placing the building when ordering construction, or loading a level. <br/>
    /// Registers the building and updates entry points.
    /// </summary>
    /// <param name="building">Building thats being placed.</param>
    /// <param name="gridPos">building anchor position.</param>
    /// <param name="load">If load is true creates, creates new roads and doesn't recycle entrypoints.</param>
    public void PlaceBuild(Building building, GridPos gridPos = null, bool load = false)
    {
        MyGrid.buildings.Add(building);
        if (gridPos == null)
            gridPos = building.GetPos();
        overlays.AddBuildingOverlay(gridPos, building.id);
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
                    if (load)
                        SceneRefs.objectFactory.CreateRoad(new(x, gridPos.y, y), false);
                    overlays.ToggleEntryPoints(r);
                    SetGridItem(new(x, y), building);
                    break;
                case GridItemType.Entrance:
                    if (load)
                        overlays.Add(new(itemPos.x, itemPos.z), -1);
                    else
                        overlays.Add(new(itemPos.x, itemPos.z), i);
                    r?.entryPoints.Add(building.id);
                    overlays.buildingOverlays[^1].GetComponentsInChildren<Image>().ToList()[^1].gameObject.SetActive(r != null);
                    break;
            }
        }
        if (!load)
            overlays.HideGlobalEntryPoints();
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
        bool canPlace = !pipeGrid[(int)pos.x, (int)pos.z] || pipeGrid[(int)pos.x, (int)pos.z].id == pipe.id;
        for (int i = 0; i < 4; i++) // checks in every direction
        {
            GridPos checkVec = new();
            switch (i)
            {
                case 0:
                    checkVec = new(pos.x + 1, pos.z);
                    if (checkVec.x == width)
                        continue;
                    break;
                case 1:
                    checkVec = new(pos.x - 1, pos.z);
                    if (checkVec.x < 0)
                        continue;
                    break;
                case 2:
                    checkVec = new(pos.x, pos.z + 1);
                    if (checkVec.z == height)
                        continue;
                    break;
                case 3:
                    checkVec = new(pos.x, pos.z - 1);
                    if (checkVec.z < 0)
                        continue;
                    break;
            }
            Pipe connectedPipe = pipeGrid[(int)checkVec.x, (int)checkVec.z];
            if (connectedPipe && canPlace)
            {
                foreach (PipePart connection in pipe.transform.GetComponentsInChildren<PipePart>())
                {
                    if (connection.name == $"{i}")
                    {
                        connection.connectedPipe.DisconnectPipe(i % 2 == 0 ? i + 1 : i - 1, false);
                    }
                }
                pipe.ConnectPipe(i, connectedPipe, true);
            }
            else
            {
                pipe.DisconnectPipe(i, true);
            }
        }
        return false;
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
        List<Road> roads = new();
        List<Image> images = new();
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
                    break;
                case GridItemType.Anchor:
                    c = new(1, 0.843f, 0, 0.25f);
                    errC = new(1, 0.643f, 0, 0.25f);
                    break;
                case GridItemType.Entrance:
                    c = new(0.5f, 0.5f, 0.5f, 0.25f);
                    errC = new(1f, 0.3f, 0.3f, 0.25f);
                    entrances.Add(tile.GetComponent<Image>());
                    activeEntrances++;
                    break;
                default:
                    continue;
            }
            ClickableObject clickable = GetGridItem(itemPos);
            if (!clickable)
                continue;
            if (clickable.GetComponent<Rock>())
            {
                tile.localPosition = new(tile.localPosition.x, tile.localPosition.y, 2.01f);
                if (item.itemType == GridItemType.Entrance)
                {

                }
                else if (item.itemType != GridItemType.Anchor)
                {
                    tile.GetComponent<Image>().color = errC;
                    canBuild = false;
                }
                else
                    canBuild = false;
            }
            else
            {
                tile.localPosition = new(tile.localPosition.x, tile.localPosition.y, 0);
                Road r = clickable.GetComponent<Road>();
                if (r)
                {
                    if (r.entryPoints.Count > 0 && (item.itemType == GridItemType.Road || item.itemType == GridItemType.Anchor))
                    {
                        roads.Add(r);
                        images.Add(tile.GetComponent<Image>());
                    }
                    tile.GetComponent<Image>().color = c;
                    continue;
                }
                else if (item.itemType != GridItemType.Entrance)
                {
                    tile.GetComponent<Image>().color = errC;
                    canBuild = false;
                }
            }
            if (item.itemType == GridItemType.Entrance)
                activeEntrances--;
        }

        if (!CheckEntranceObscursion(roads, images))
            canBuild = false;
        foreach (Image image in entrances)
        {
            if (activeEntrances == -1)
            {
                image.color = new(1f, 0.3f, 0.3f, 0.25f);
                canBuild = false;
            }
            else
                image.color = new(0.5f, 0.5f, 0.5f, 0.25f);
        }
        return canBuild;
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
                if (!buildings.Keys.Contains(i))
                    buildings.Add(i, overlays.buildingOverlays.First(q => q.name == i.ToString()).GetComponentsInChildren<Image>().Where(q => q.enabled).Count());
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
}
