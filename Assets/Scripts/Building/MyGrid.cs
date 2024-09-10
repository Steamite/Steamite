using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public static class MyGrid
{
    public static readonly int width = 21;
    public static readonly int height = 21;
    public static List<Building> buildings = new();
    public static List<Chunk> chunks = new();
    public static List<Human> humans = new();
    public static List<FluidNetwork> fluidNetworks = new();
    public static GridTiles gridTiles;

    static ClickableObject[,] grid;
    public static Pipe[,] pipeGrid;

    public static ResourceHolder buildPrefabs;
    public static ResourceHolder tilePrefabs;
    public static ResourceHolder specialPrefabs;
    public static SceneReferences sceneReferences;

    ////////////////////////////////////////////////////////////
    //------------------------GetSet--------------------------//
    ////////////////////////////////////////////////////////////
    public static ClickableObject GetGridItem(GridPos gp)
    {
        int x = Mathf.RoundToInt(gp.x);
        int y = Mathf.RoundToInt(gp.z);
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogError($"(Get)Index outside of grid bounds, x: {x}, y :{y}.");
            return null;
        }
        return grid[x, y];
    }

    public static void SetGridItem(GridPos gp, ClickableObject clickable)
    {
        int x = Mathf.RoundToInt(gp.x);
        int y = Mathf.RoundToInt(gp.z);
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogError($"(Set)Index outside of grid bounds, x: {x}, y :{y}.");
            return;
        }
        if (clickable.GetComponent<Road>())
        {
            foreach (Transform tran in sceneReferences.overlay.buildingOverlays)
            {
                for(int i = 0; i < tran.childCount; i++)
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
        grid[x, y] = clickable;
    }

    public static void ClearGrid()
    {
        buildings = new List<Building>();
        chunks = new List<Chunk>();
        grid = new ClickableObject[width, height];
        pipeGrid = new Pipe[width, height];
        fluidNetworks = new();
    }
    ////////////////////////////////////////////////////////////
    //------------------------Start---------------------------//
    ////////////////////////////////////////////////////////////
    public static StringBuilder PrepGrid(Transform t, SceneReferences _sceneReferences)
    {
        ClearGrid();
        sceneReferences = _sceneReferences;
        gridTiles = t.GetComponent<GridTiles>();
        grid = new ClickableObject[width, height];
        pipeGrid = new Pipe[width, height];
        for (int i = 0; i < t.childCount; i++)
        {
            Transform objects = t.GetChild(i);
            FillRocks(objects.GetChild(0).GetChild(0)); // adds ores
            FillRoads(objects.GetChild(0).GetChild(1)); // adds roads
            FillWater(objects.GetChild(0).GetChild(2)); // adds water
            FillBuildings(objects.GetChild(0).GetChild(3)); // adds Buildings and (entry points)
        }
        return PrintGrid();
    }
    static void FillRoads(Transform objects)
    {
        for (int j = 0; j < objects.childCount; j++)
        {
            GridPos vec = new(objects.GetChild(j).transform.localPosition);
            SetGridItem(vec, objects.GetChild(j).GetComponent<Road>());
        }
    }
    static void FillRocks(Transform objects)
    {
        for (int j = 0; j < objects.childCount; j++)
        {
            GridPos vec = new(objects.GetChild(j).transform.localPosition);
            SetGridItem(vec, objects.GetChild(j).GetComponent<Rock>());
            GetGridItem(vec).UniqueID();
        }
    }
    static void FillWater(Transform objects)
    {
        for (int j = 0; j < objects.childCount; j++)
        {
            GridPos vec = new(objects.GetChild(j).transform.localPosition);
            SetGridItem(vec, objects.GetChild(j).GetComponent<Water>());
        }
    }
    static void FillBuildings(Transform objects)
    {
        foreach (Building building in objects.GetComponentsInChildren<Building>())
        {
            if (!building.GetComponent<BuildPipe>())
            {
                PlaceBuild(building, true);
                if (building.GetComponent<ProductionBuilding>() && building.build.constructed)
                {
                    building.GetComponent<ProductionBuilding>().RefreshStatus();
                    building.GetComponent<ProductionBuilding>().RequestRestock();
                }
            }
            else
                building.GetComponent<BuildPipe>().PlacePipe();
        }
        foreach (Pipe pipe in objects.transform.parent.GetChild(4).GetComponentsInChildren<Pipe>())
        {
            pipe.PlacePipe();
        }
    }

    ////////////////////////////////////////////////////////////
    //------------------------Updating------------------------//
    ////////////////////////////////////////////////////////////
    public static void PlaceBuild(Building building, bool load = false)
    {

        if (building.id == -1)
            building.UniqueID();
        buildings.Add(building);
        // assigns sizes acording to rotation
        // TODO

        //BuildObject buildObject = new();
        //bool canBuild = true;
        GridPos gridPos = new(building.transform.position - CheckRotation(building.build.blueprint.moveBy, building.transform.rotation.eulerAngles.y).ToVec());
        sceneReferences.overlay.AddBuildingOverlay(gridPos, building.id);
        for (int i = building.build.blueprint.itemList.Count - 1; i > -1; i--)
        {
            NeededGridItem item = building.build.blueprint.itemList[i];
            GridPos itemPos = CheckRotation(item.pos, building.transform.rotation.eulerAngles.y, true);
            int x = (int)(itemPos.x + gridPos.x);
            int y = (int)(gridPos.z - itemPos.z);
            Road r = GetGridItem(new(x, y))?.GetComponent<Road>();
            switch (item.itemType)
            {
                case GridItemType.Road:
                case GridItemType.Anchor:
                    if (load)
                        GameObject.Instantiate(tilePrefabs.GetPrefab("Road"), new(x, 0.45f, y), Quaternion.identity, sceneReferences.roads);
                    sceneReferences.overlay.ToggleEntryPoints(r);
                    SetGridItem(new(x,y), building);
                    break;
                case GridItemType.Entrance:
                    if (load)
                        sceneReferences.overlay.Add(new(itemPos.x, itemPos.z), -1);
                    else
                        sceneReferences.overlay.Add(new(itemPos.x, itemPos.z), i);
                    if (r)
                    {
                        sceneReferences.overlay.buildingOverlays[^1].GetComponentsInChildren<Image>().ToList()[^1].gameObject.SetActive(true);
                        r.entryPoints.Add(building.id);
                        continue;
                    }
                    sceneReferences.overlay.buildingOverlays[^1].GetComponentsInChildren<Image>().ToList()[^1].gameObject.SetActive(false);
                    break;
            }
        }
        if (!load)
            sceneReferences.overlay.DeleteBuildGrid();

    }

    ////////////////////////////////////////////////////////////
    //------------------------Checking------------------------//
    ////////////////////////////////////////////////////////////
    public static bool CanPlace(Pipe pipe)
    {
        GridPos pos = new(pipe.transform.position);
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
        return canPlace;
    }
    public static bool CanPlace(Building building)
    {
        BuildObject buildObject = new();
        bool canBuild = true;
        GridPos gridPos = new(building.transform.position - CheckRotation(building.build.blueprint.moveBy, building.transform.rotation.eulerAngles.y).ToVec());
        sceneReferences.overlay.CreateBuildGrid(building);
        // checks all Parts of a building
        Transform overlay = sceneReferences.overlay.overlayParent;
        List<Road> roads = new();
        List<Image> images = new();
        List<Image> entrances = new();
        int activeEntrances = -1;
        for (int i = 0; i < building.build.blueprint.itemList.Count; i++)
        {
            NeededGridItem item = building.build.blueprint.itemList[i];
            GridPos itemPos = CheckRotation(item.pos, building.transform.rotation.eulerAngles.y, true);
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
                    if(r.entryPoints.Count > 0 && (item.itemType == GridItemType.Road || item.itemType == GridItemType.Anchor))
                    {
                        roads.Add(r);
                        images.Add(tile.GetComponent<Image>());
                    }
                    tile.GetComponent<Image>().color = c;
                    continue;
                }
                else if(item.itemType != GridItemType.Entrance)
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
        foreach(Image image in entrances)
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

    ////////////////////////////////////////////////////////////
    //----------------------Miscelanious----------------------//
    ////////////////////////////////////////////////////////////

    public static GridPos CheckRotation(GridPos offset, float rotation, bool isTile = false)
    {
        GridPos gp;
        switch (rotation)
        {
            case 90:
                if (isTile)
                    gp = new(-offset.z, offset.x);
                else
                    gp = new(offset.z, -offset.x);
                break;
            case 180:
                gp = new(-offset.x, -offset.z);
                break;
            case 270:
                if (isTile)
                    gp = new(offset.z, -offset.x);
                else
                    gp = new(-offset.z, offset.x);
                break;
            default:
                gp = new(offset.x, offset.z);
                break;
        }
        return gp;
    }
    public static StringBuilder PrintGrid()
    {
        // prints the entire grid
        StringBuilder s = new();
        for (int z = height - 1; z > 0; z--) // start at top
        {
            string _s = "";
            for (int x = 0; x < width; x++) // start from left
            {
                ClickableObject item = grid[x, z];
                if (item != null)
                {
                    _s += item.PrintText() + ";";
                    continue;
                }
                _s += "_;";
            }
            s.AppendLine(_s);
        }
        return s;
    }

    /// <summary>
    /// Called when mining out rocks, removes it and replaces it with a set prefab.
    /// </summary>
    /// <param name="rock">Rock to replace.</param>
    public static void RemoveTiles(Rock rock)
    {
        Vector3 vec = rock.transform.position;
        gridTiles.toBeDigged.Remove(rock); // removes from list
        gridTiles.markedTiles.Remove(rock);
        GameObject replacement = GameObject.Instantiate(tilePrefabs.GetPrefab("Road").gameObject, new Vector3(vec.x, rock.transform.localPosition.y + 0.45f, vec.z), Quaternion.identity, gridTiles.transform); // creates a road on the place of tiles
        replacement.name = replacement.name.Replace("(Clone)", "");
        replacement.transform.parent = GameObject.Find(replacement.name + "s").transform;
        SetGridItem(new(vec), replacement.GetComponent<Road>() ? replacement.GetComponent<Road>() : replacement.GetComponent<Water>());
        gridTiles.Remove(rock);
        GameObject.Destroy(rock.gameObject); // destroys object
    }

    public static void RemoveBuilding(Building building)
    {
        if (building is Pipe)
        {

        }
        else
        {
            GridPos gridPos = new(building.transform.position - CheckRotation(building.build.blueprint.moveBy, building.transform.rotation.eulerAngles.y).ToVec());
            sceneReferences.overlay.AddBuildingOverlay(gridPos, building.id);
            if(building.build.blueprint.itemList.Count > 0)
            {
                List<Road> roads = sceneReferences.roads.GetComponentsInChildren<Road>().ToList();
                for (int i = building.build.blueprint.itemList.Count - 1; i > -1; i--)
                {
                    NeededGridItem item = building.build.blueprint.itemList[i];
                    GridPos itemPos = CheckRotation(item.pos, building.transform.rotation.eulerAngles.y, true);
                    int x = (int)(itemPos.x + gridPos.x);
                    int y = (int)(gridPos.z - itemPos.z);

                    switch (item.itemType)
                    {
                        case GridItemType.Road:
                        case GridItemType.Anchor:
                            Road r = roads.FirstOrDefault(q=> new GridPos(q.transform.position).Equals(new GridPos(x,y)));
                            r.entryPoints = new();
                            SetGridItem(new(x, y), r);
                            break;
                    }
                }
            }
        }
        if (building.GetType() != typeof(Pipe))
        {
            /*foreach ((RectTransform t in MyGrid.sceneReferences.OverlayCanvas.buildingOverlays.First(q => q.name == building.id.ToString()).GetComponentsInChildren<Image>().Select(q => q.transform)))
            {
                int x = Mathf.CeilToInt(entryPoint.position.x);
                int z = Mathf.CeilToInt(entryPoint.position.z);
                Road gR = (grid[x, z] as Road);
                gR?.entryPoints.RemoveAll(q => q == building.id);
                Debug.Log(grid[x, z].PrintText());
            }*/
        }
        buildings.Remove(building);
        Debug.Log(PrintGrid());
    }

    static bool CheckEntranceObscursion(List<Road> roads, List<Image> tiles)
    {
        Dictionary<int, int> buildings = new Dictionary<int, int>();
        bool ok = true;

        foreach (Road road in roads)
        {
            foreach (int i in road.entryPoints)
            {
                if (!buildings.Keys.Contains(i))
                    buildings.Add(i, sceneReferences.overlay.buildingOverlays.First(q => q.name == i.ToString()).GetComponentsInChildren<Image>().Where(q => q.enabled).Count());
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

}
