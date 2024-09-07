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

    public static ClickableObject[,] grid;
    public static Pipe[,] pipeGrid;

    public static ResourceHolder buildPrefabs;
    public static ResourceHolder tilePrefabs;
    public static ResourceHolder specialPrefabs;
    public static SceneReferences sceneReferences;
    ////////////////////////////////////////////////////////////
    //------------------------Start---------------------------//
    ////////////////////////////////////////////////////////////
    public static StringBuilder PrepGrid(Transform t)
    {
        ClearGrid();
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
    public static void ClearGrid()
    {
        buildings = new List<Building>();
        chunks = new List<Chunk>();
        fluidNetworks = new();
        grid = null;
        pipeGrid = null;
    }
    static void FillRoads(Transform objects)
    {
        for (int j = 0; j < objects.childCount; j++)
        {
            GridPos vec = new(objects.GetChild(j).transform.localPosition);
            grid[(int)vec.x, (int)vec.z] = objects.GetChild(j).GetComponent<Road>();
        }
    }
    static void FillRocks(Transform objects)
    {
        for (int j = 0; j < objects.childCount; j++)
        {
            GridPos vec = new(objects.GetChild(j).transform.localPosition);
            grid[(int)vec.x, (int)vec.z] = objects.GetChild(j).GetComponent<Rock>();
            grid[(int)vec.x, (int)vec.z].UniqueID();
        }
    }
    static void FillWater(Transform objects)
    {
        for (int j = 0; j < objects.childCount; j++)
        {
            GridPos vec = new(objects.GetChild(j).transform.localPosition);
            grid[(int)vec.x, (int)vec.z] = objects.GetChild(j).GetComponent<Water>();
        }
    }
    static void FillBuildings(Transform objects)
    {
        foreach (Building building in objects.GetComponentsInChildren<Building>())
        {
            if (!building.GetComponent<BuildPipe>())
            {
                PlaceBuild(building);
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
        
        if(building.id == -1)
            building.UniqueID();
        buildings.Add(building);
        // assigns sizes acording to rotation
        // TODO

        //BuildObject buildObject = new();
        //bool canBuild = true;
        GridPos gridPos = new(building.transform.position - CheckRotation(building.build.blueprint.moveBy, building.transform.rotation.eulerAngles.y).ToVec());
        for (int i = 0; i < building.build.blueprint.itemList.Count; i++)
        {
            NeededGridItem item = building.build.blueprint.itemList[i];
            GridPos itemPos = CheckRotation(item.pos, building.transform.rotation.eulerAngles.y, true);
            switch (item.itemType)
            {
                case GridItemType.Road:
                case GridItemType.Anchor:
                    if (load)
                    {
                        GameObject.Instantiate(tilePrefabs.GetPrefab("Road"), new(itemPos.x + gridPos.x, 0, gridPos.z - itemPos.z), Quaternion.identity , sceneReferences.roads);
                    }
                    grid[(int)(itemPos.x + gridPos.x), (int)(gridPos.z - itemPos.z)] = building;
                    break;
                case GridItemType.Entrance:
                    Road r = grid[(int)(itemPos.x + gridPos.x), (int)(gridPos.z - itemPos.z)].GetComponent<Road>();
                    if (r)
                    {
                        r.entryPoints.Add(building.id);
                        continue;
                    }
                    break;
            }
            //overlay.GetChild(i).GetComponent<>
        }
        /*
        //Vector2 v = CheckRotation(building.build, building.transform.rotation.eulerAngles.y);
        float _x = 0;//v.x;
        float _z = 0;// v.y;
        // calculates builds position(lower left corner)
        Vector3 vec = new Vector3(
            building.transform.position.x - ((_x - 1) / 2), 
            building.transform.position.y, 
            building.transform.position.z - ((_z - 1) / 2));

        // adds buildings to the grid
        for (int x = 0; x < _x; x++)
        {
            int vecX = Mathf.FloorToInt(vec.x) + x;
            if(0 <= vecX && vecX < width) // inside range
            {
                for (int z = 0; z < _z; z++)
                {
                    int vecZ = Mathf.FloorToInt(vec.z) + z;
                    if (0 <= vecZ && vecZ < height) // inside range
                    {
                        grid[vecX, vecZ] = building;
                        if (roadPref)
                        {
                            GameObject.Instantiate(roadPref, new(vecX, 0, vecZ), Quaternion.identity, building.transform.parent.parent.GetChild(1));
                        }
                    }
                }
            }
        }
        int i = 0;

        if(building.transform.childCount > 0 && building.transform.GetChild(0).childCount > 0)
        {
            // adds entry points
            foreach (Transform entryPoint in building.transform.GetChild(0).GetComponentsInChildren<Transform>())
            {
                i++;
                if (i == 1) continue;
                GridPos _vec = new(entryPoint.transform.position);
                if ((0 <= _vec.x && 0 <= _vec.z) && (_vec.x < width && _vec.z < height))
                {
                    Road r = grid[(int)_vec.x, (int)_vec.z] as Road;
                    if (r != null)
                    {
                        r.entryPoints.Add(building.id);
                    }
                }
            }
        }*/
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
        sceneReferences.OverlayCanvas.CreateBuildGrid(building);
        // checks all Parts of a building
        Transform overlay = sceneReferences.OverlayCanvas.overlayParent;
        for (int i = 0; i < building.build.blueprint.itemList.Count; i++)
        {
            NeededGridItem item = building.build.blueprint.itemList[i];
            GridPos itemPos = CheckRotation(item.pos, building.transform.rotation.eulerAngles.y, true);
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
                    break;
                default:
                    continue;
            }
            if (grid[(int)(itemPos.x + gridPos.x), (int)(gridPos.z - itemPos.z)].GetComponent<Rock>())
            {
                tile.localPosition = new(tile.localPosition.x, tile.localPosition.y, 2.01f);
                if(item.itemType != GridItemType.Anchor)
                    tile.GetComponent<Image>().color = errC;
                canBuild = false;
            }
            else
            {
                tile.localPosition = new(tile.localPosition.x, tile.localPosition.y, 0);
                Road r = grid[(int)(itemPos.x + gridPos.x), (int)(gridPos.z - itemPos.z)].GetComponent<Road>();
                if (r)
                {
                    tile.GetComponent<Image>().color = c;
                }
                else
                {
                    tile.GetComponent<Image>().color = errC;
                    canBuild = false;
                }
            }
            //overlay.GetChild(i).GetComponent<>
        }
        // assigns sizes acording to rotation
        //TODO
        /*Vector2 v = CheckRotation(building.build, building.transform.rotation.eulerAngles.y);
        float _x = 0;// v.x;
        float _z = 0;// v.y;
        // calculates builds position(lower left corner)
        Vector3 vec = new Vector3(
            building.transform.position.x - ((_x - 1) / 2),
            building.transform.position.y,
            building.transform.position.z - ((_z - 1) / 2));
      
        List<Vector2Int> tempP = new();
        int i;
        for (int x = 0; x < _x; x++)
        {
            int vecX = Mathf.FloorToInt(vec.x) + x;
            if (0 <= vecX && vecX < width) // inside range
            {
                for (int z = 0; z < _z; z++)
                {
                    int vecZ = Mathf.FloorToInt(vec.z) + z;
                    if (0 <= vecZ && vecZ < height) // inside range
                    {
                        Road r = grid[vecX, vecZ].GetComponent<Road>();
                        if(r != null)
                        {
                            tempP.Add(new(vecX, vecZ));
                            foreach (int id in r.entryPoints)
                            {
                                bool found = false;
                                i = 0;
                                foreach(Transform pos in buildings[buildings.Select(q => q.id).ToList().IndexOf(id)].transform.GetChild(0).GetComponentsInChildren<Transform>())
                                {
                                    i++;
                                    if (i == 1)
                                        continue;
                                    Vector2Int _v = Vector2Int.RoundToInt(new(pos.transform.position.x, pos.transform.position.z));
                                    if (!tempP.Contains(_v))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    return false;
                                }
                            }
                        }
                        else return false;
                    }
                    else return false;
                }
            }
            else return false;
        }

        i = 0;
        // checks all Entry Points of a building
        foreach (Transform entryPoint in building.transform.GetChild(0).GetComponentsInChildren<Transform>())
        {
            i++;
            if (i == 1) continue;
            GridPos gridPos = new(entryPoint.transform.position);
            if ((0 <= gridPos.x && 0 <= gridPos.z) && (gridPos.x < width && gridPos.z < height))
            {
                Road r = grid[(int)gridPos.x, (int)gridPos.z] as Road;
                if (r != null)
                {
                    return true;
                }
            }
        }*/
        // change for building without entry points
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

    ////////////////////////////////////////////////////////////////
    public static void UpdateGrid(GridPos gridPos, ClickableObject item)
    {
        grid[(int)gridPos.x, (int)gridPos.z] = item;
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
        GameObject replacement = GameObject.Instantiate(tilePrefabs.GetPrefab("Road").gameObject, new Vector3(vec.x, rock.transform.localPosition.y, vec.z), Quaternion.identity, gridTiles.transform); // creates a road on the place of tiles
        replacement.name = replacement.name.Replace("(Clone)", "");
        replacement.transform.parent = GameObject.Find(replacement.name + "s").transform;
        UpdateGrid(new(vec), replacement.GetComponent<Road>() ? replacement.GetComponent<Road>() : replacement.GetComponent<Water>());
        gridTiles.Remove(rock);
        GameObject.Destroy(rock.gameObject); // destroys object
    }

    public static void RemoveBuilding(Building building)
    {
        // TODO
        /*Vector2 size = CheckRotation(building.build, building.transform.eulerAngles.y);
        //GameObject roadPref = sce
        for (int x = Mathf.CeilToInt(building.transform.position.x - size.x / 2); x < Mathf.CeilToInt(building.transform.position.x + size.x/2); x++)
        {
            for (int z = Mathf.CeilToInt(building.transform.position.z - size.y / 2); z < Mathf.CeilToInt(building.transform.position.z + size.y / 2); z++)
            {
                grid[x, z] = (GameObject.Instantiate(tilePrefabs.GetPrefab("Road").gameObject, new(x, 0, z), Quaternion.identity, building.transform.parent.parent.GetChild(1)) as GameObject).GetComponent<Road>();
            }
        }*/
        if(building.GetType() != typeof(Pipe))
        {
            foreach (Transform entryPoint in building.transform.GetChild(0).GetComponentsInChildren<Transform>().Skip(1))
            {
                int x = Mathf.CeilToInt(entryPoint.position.x);
                int z = Mathf.CeilToInt(entryPoint.position.z);
                Road gR = (grid[x, z] as Road);
                gR?.entryPoints.RemoveAll(q => q == building.id);
                Debug.Log(grid[x, z].PrintText());
            }
        }
        buildings.Remove(building);
        Debug.Log(PrintGrid());
    }
}
