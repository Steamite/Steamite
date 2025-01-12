using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GroundLevel : MonoBehaviour
{
    [Header("Grid")]
    public int width = 21;
    public int height = 21;
    
    ClickableObject[,] grid;
    Pipe[,] pipeGrid;

    [Header("Reference")]
    public Transform rocks;
    public Transform roads;
    public Transform water;
    public Transform chunks;
    public Transform buildings;
    public Transform pipes;
    public UIOverlay overlays;

    public bool unlocked;
    #region Base Grid operations
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
    public void CreateGrid(int gridSize = -1)
    {
        ClearGrid(gridSize);
        
        FillRocks(rocks); // adds ores
        FillRoads(roads); // adds roads
        FillWater(water); // adds water
        FillBuildings(buildings); // adds Buildings and (entry points)
        FillPipes(pipes); // adds Pipes

        gameObject.SetActive(false);
    }
    #region specific Creations
    void FillRoads(Transform objects)
    {
        for (int j = 0; j < objects.childCount; j++)
        {
            GridPos vec = new(objects.GetChild(j).transform.localPosition);
            SetGridItem(vec, objects.GetChild(j).GetComponent<Road>());
        }
    }
    void FillRocks(Transform objects)
    {
        for (int j = 0; j < objects.childCount; j++)
        {
            GridPos vec = new(objects.GetChild(j).transform.localPosition);
            Rock rock = objects.GetChild(j).GetComponent<Rock>();
            SetGridItem(vec, rock);
            GetGridItem(vec).UniqueID();
            if (rock.rockYield.ammount.Sum() == 0)
            {
                rock.ColorWithIntegrity();
            }
        }
    }
    void FillWater(Transform objects)
    {
        for (int j = 0; j < objects.childCount; j++)
        {
            GridPos vec = new(objects.GetChild(j).transform.localPosition);
            SetGridItem(vec, objects.GetChild(j).GetComponent<Water>());
        }
    }
    void FillBuildings(Transform objects)
    {
        foreach (Building building in objects.GetComponentsInChildren<Building>())
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
    void FillPipes(Transform objects)
    {
        foreach (Pipe pipe in objects.GetComponentsInChildren<Pipe>())
        {
            pipe.PlacePipe();
        }
    }
    #endregion

    #endregion Creation

    #region Adding to Grid
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

    public void UnsetBuilding(Building building, GridPos gridPos)
    {
        overlays.Remove(building.id, gridPos);
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
    public bool CanPlace(Building building, GridPos gridPos)
    {
        bool canBuild = true;
        overlays.CreateBuildGrid(building);
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
