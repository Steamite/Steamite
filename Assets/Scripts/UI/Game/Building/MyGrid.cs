using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class MyGrid
{
    public const int NUMBER_OF_LEVELS = 5;
    public static List<Building> buildings = new();
    public static List<Chunk> chunks = new();
    public static List<Human> humans = new();
    public static List<FluidNetwork> fluidNetworks = new();

    static GroundLevel[] levels;

    public static int currentLevel { get; private set; }
    public static string startSceneName;
    
    static Action<int, int> GridChange;

    public static void Init()
    {
        GridChange += (int _, int _) => SceneRefs.gridTiles.ChangeSelMode(SelectionMode.nothing);
        GridChange += (int i, int newI) => SceneRefs.humans.SwitchLevel(i, newI);
        ChangeGridLevel(2);
    }

    #region Grid Access
    public static ClickableObject GetGridItem(GridPos gp, bool isPipe = false)
    {
        return levels[gp.y].GetGridItem(gp, isPipe);
    }

    public static void SetGridItem(GridPos gp, ClickableObject clickable, bool isPipe = false)
    {
        levels[gp.y].SetGridItem(gp, clickable, isPipe);
    }

    public static UIOverlay GetOverlay(int lIndex = -1)
    {
        if (lIndex == -1)
            lIndex = currentLevel;
        return levels[lIndex].overlays;
    }
    public static void PrepGridLists()
    {
        buildings = new List<Building>();
        chunks = new List<Chunk>();
        fluidNetworks = new();
        levels = new GroundLevel[5];
    }

    public static StringBuilder PrintGrid()
    {
        StringBuilder grids = new();
        int i = 0;
        foreach(GroundLevel level in levels.Where(q => q))
        {
            grids.AppendLine($"Grid {i}:");
            grids.Append(level.PrintGrid());
            i++;
        }
        return grids;
    }
    #endregion Grid Access

    #region Grid Creation
    public static StringBuilder CreateGrid(GroundLevel level, GroundLevel mainLevel)
    {
        PrepGridLists();
        GameObject.Find("Scene").GetComponent<SceneRefs>().Init();
        GameObject.Find("UI canvas").GetComponent<CanvasManager>().Init();
        currentLevel = 2;
        for (int i = 0; i < 5; i++)
        {
            // creates an empty ground level
            GroundLevel _level;
            if (i == 2)
                _level = GameObject.Instantiate(mainLevel, new Vector3(0, i * 2, 0), Quaternion.identity, SceneRefs.gridTiles.transform);
            else
                _level = GameObject.Instantiate(level, new Vector3(0, i * 2, 0), Quaternion.identity, SceneRefs.gridTiles.transform);
            levels[i] = _level;
            _level.CreateGrid();
        }
        return PrintGrid();
    }
    #endregion Grid Creation

    #region Grid Updating
    public static void PlaceBuild(Building building, bool load = false)
    {
        if (building.id == -1)
            building.UniqueID();
        
        GridPos gridPos = building.GetPos();
        levels[gridPos.y].PlaceBuild(building, gridPos, load);
    }

    public static void RemoveRock(Rock rock)
    {
        SceneRefs.gridTiles.toBeDigged.Remove(rock); // removes from list
        SceneRefs.gridTiles.markedTiles.Remove(rock);
        SceneRefs.gridTiles.
            DestroyUnselect(rock);
        
        SceneRefs.objectFactory.CreateRoad(rock.GetPos(), true);
        GameObject.Destroy(rock.gameObject); // destroys object
    }
    public static void RemoveBuilding(Building building)
    {
        if (building is Pipe)
        {

        }
        else
        {
            SceneRefs.gridTiles.DestroyUnselect(building);
            GridPos gridPos = building.GetPos();
            GridPos p = Rotate(building.build.blueprint.moveBy, building.transform.rotation.eulerAngles.y);
            gridPos.x -= p.x;
            gridPos.z -= p.z;
            //canvasManager.overlays.AddBuildingOverlay(gridPos, building.id);
            if (building.build.blueprint.itemList.Count > 0)
            {
                levels[gridPos.y].RemoveBuilding(building, gridPos);
            }
        }
        /*if (building.GetType() != typeof(Pipe))
        {
            foreach ((RectTransform t in SceneRefs.OverlayCanvas.buildingOverlays.First(q => q.name == building.id.ToString()).GetComponentsInChildren<Image>().Select(q => q.transform)))
            {
                int x = Mathf.CeilToInt(entryPoint.position.x);
                int z = Mathf.CeilToInt(entryPoint.position.z);
                Road gR = (grid[x, z] as Road);
                gR?.entryPoints.RemoveAll(q => q == building.id);
                Debug.Log(grid[x, z].PrintText());
            }
        }*/
        buildings.Remove(building);
        Debug.Log(PrintGrid());
    }

    #endregion Grid Updating

    #region Checking
    public static bool CanPlace(Building building)
    {
        if(building is Pipe)
        {
            GridPos pos = new(building.transform.position);
            return levels[pos.y].CanPlace(building as Pipe, pos);
        }
        else
        {
            GridPos gridPos = building.GetPos();
            return levels[gridPos.y].CanPlace(building, gridPos);
        }
    }

    #endregion Checking

    #region Level References
    // Gives References to part of the requested level.

    public static Transform FindLevelChunks(int lIndex) => levels[lIndex].chunks;
    public static Transform FindLevelRoads(int lIndex) => levels[lIndex].roads;
    public static Transform FindLevelWater(int lIndex) => levels[lIndex].water;
    public static Transform FindLevelRocks(int lIndex) => levels[lIndex].rocks;
    public static Transform FindLevelBuildings(int lIndex) => levels[lIndex].buildings;

    #endregion

    public static GridPos Rotate(GridPos offset, float rotation, bool isTile = false)
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

    static public int ChangeGridLevel(int newLevel)
    {
        GridChange?.Invoke(currentLevel, newLevel);
        levels[currentLevel].gameObject.SetActive(false);
        currentLevel = newLevel;
        levels[currentLevel].gameObject.SetActive(true);
        return currentLevel;
    }

    public static GridSave Save(int i)
    {
        GroundLevel level = levels[i];
        GridSave gridSave = new(level.height, level.width);
        GridPos gp = new();
        for(int x = 0; x < gridSave.height; x++)
        {
            gp.x = x;
            for(int y = 0; y < gridSave.width; y++)
            {
                gp.z = y;
                ClickableObject click = level.GetGridItem(gp, false);
                gridSave.grid[x, y] = (click is Building) ? null : click?.Save();
                gridSave.pipes[x, y] = level.GetGridItem(gp, true)?.Save();
            }
        }
        return gridSave;
    }

    public static void Load(GridSave gridSave, GroundLevel templateLevel, int i)
    {
        GroundLevel groundLevel = GameObject.Instantiate(templateLevel, new Vector3(0, ClickabeObjectFactory.LEVEL_HEIGHT * i, 0), Quaternion.identity, SceneRefs.gridTiles.transform);
        levels[i] = groundLevel;
        groundLevel.ClearGrid();
        groundLevel.gameObject.SetActive(i == 2);
        for(int x = 0; x < gridSave.width; x++)
        {
            for (int z = 0; z < gridSave.width; z++)
            {
                switch (gridSave.grid[x, z])
                {
                    case RockSave:
                        SceneRefs.objectFactory.CreateSavedRock(gridSave.grid[x, z] as RockSave, new (x, i, z));
                        break;
                    case WaterSave:
                        SceneRefs.objectFactory.CreateSavedWater(gridSave.grid[x, z] as WaterSave, new(x, i, z));
                        break;
                    default :
                        SceneRefs.objectFactory.CreateRoad(new(x, i, z), true);
                        break;
                }
            }
        }
        SceneRefs.humans.GetComponent<JobQueue>().toBeDug = SceneRefs.gridTiles.toBeDigged.ToList();
    }

   
}
