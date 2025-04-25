using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Basic highlight color(for selection).</summary>
public static class MyGrid
{
    #region Variables
    /// <summary>Number of Levels in game.</summary>
    public const int NUMBER_OF_LEVELS = 5;
    /// <summary>List of all buildings on all levels.</summary>
    public static List<Building> buildings = new();
    /// <summary>List of all chunks on all levels.</summary>
    public static List<Chunk> chunks = new();
    /// <summary>List of all fluid networks on all levels.</summary>
    public static List<FluidNetwork> fluidNetworks = new();

    /// <summary>All levels.</summary>
    static GroundLevel[] levels;

    /// <summary>Event for switching between levels.</summary>
    static Action<int, int> GridChange;
    /// <summary>World name(for saving).</summary>
    public static string worldName;
    public static string startSceneName;
    #endregion

    #region Getters
    /// <summary>Get current active level.</summary>
    public static int currentLevel { get; private set; }
    /// <summary>Get grid size.(from one of the levels)</summary>
    public static int gridSize { get { return levels[currentLevel].height; } }
    #endregion

    [RuntimeInitializeOnLoadMethod]
    static void ReloadDomain() => GridChange = null;

    /// <summary>
    /// Links <see cref="GridChange"/> events. And changes the active level.
    /// </summary>
    public static void Init()
    {
        GridChange += (int _, int _) => SceneRefs.gridTiles.ChangeSelMode(ControlMode.nothing);
        GridChange += (int _, int _) => SceneRefs.gridTiles.Exit(SceneRefs.gridTiles.activeObject);
        GridChange += (int i, int newI) => SceneRefs.humans.SwitchLevel(i, newI);
        ChangeGridLevel(0);
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
        currentLevel = 2;
    }
    #endregion Grid Access

    #region Grid Creation
    public static void CreateGrid(GroundLevel level, GroundLevel mainLevel)
    {
        PrepGridLists();
        for (int i = 0; i < 5; i++)
        {
            // creates an empty ground level
            GroundLevel _level;
            if (i == 0)
            {
                _level = GameObject.Instantiate(mainLevel,
                    new Vector3(0, i * 2, 0),
                    Quaternion.identity,
                    SceneRefs.gridTiles.transform);
                _level.unlocked = true;
            }
            else
            {
                _level = GameObject.Instantiate(level,
                    new Vector3(0, i * 2, 0),
                    Quaternion.identity,
                    SceneRefs.gridTiles.transform);
                _level.unlocked = false;
            }
            levels[i] = _level;
            _level.CreateGrid();
        }
    }

    public static GroundLevel AddEmptyGridLevel(GroundLevel templateLevel, int i, int gridSize)
    {
        levels[i] = GameObject.Instantiate(templateLevel, new Vector3(0, i * 2, 0), Quaternion.identity, SceneRefs.gridTiles.transform);
        levels[i].CreateGrid(gridSize);

        return levels[i];
    }
    #endregion Grid Creation

    #region Grid Updating
    /// <summary>
    /// Updates the building grid.
    /// </summary>
    /// <param name="building"></param>
    /// <param name="load"></param>
    public static void SetBuilding(Building building, bool load = false)
    {
        GridPos gridPos = building.GetPos();
        levels[gridPos.y].PlaceBuild(building, gridPos, load);
    }

    public static void UnsetRock(Rock rock)
    {
        SceneRefs.jobQueue.toBeDug.Remove(rock); // removes from list
        SceneRefs.gridTiles.markedTiles.Remove(rock);
        SceneRefs.gridTiles.DestroyUnselect(rock);
        GameObject.Destroy(rock.gameObject);
    }

    public static void UnsetBuilding(Building building)
    {
        if (building is Pipe)
        {

        }
        else
        {
            SceneRefs.gridTiles.DestroyUnselect(building);
            if (building.blueprint.itemList.Count > 0)
            {
                GridPos gridPos = building.GetPos();
                levels[gridPos.y].UnsetBuilding(building, gridPos);
            }
        }
        buildings.Remove(building);
        GameObject.Destroy(building.gameObject);
    }

    public static void UnlockLevel(int levelToUnlock)
    {
        if (!IsUnlocked(levelToUnlock))
        {
            levels[levelToUnlock].unlocked = true;
        }
    }

    #endregion Grid Updating

    #region Checking
    public static bool CanPlace(Building building)
    {
        if (building is Pipe)
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

    public static bool IsUnlocked(int lIndex) => levels[lIndex].unlocked;
    #endregion

    /// <summary>
    /// Rotates the <paramref name="offset"/> by rotation.
    /// </summary>
    /// <param name="offset">Original value.</param>
    /// <param name="rotation">Dermining value</param>
    /// <param name="isTile">If it's a building or a tile.</param>
    /// <returns>Rotated <paramref name="offset"/>.</returns>
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
                return new(offset.x, offset.z);
        }
        return gp;
    }

    /// <summary>
    /// Only way to change active level view. Triggers the <see cref="GridChange"/> event.
    /// </summary>
    /// <param name="newLevel">New level to switch to.</param>
    static public void ChangeGridLevel(int newLevel)
    {
        GridChange?.Invoke(currentLevel, newLevel);
        levels[currentLevel].gameObject.SetActive(false);
        currentLevel = newLevel;
        levels[currentLevel].gameObject.SetActive(true);
    }

    #region Saving
    public static GridSave Save(int i)
    {
        GroundLevel level = levels[i];
        GridSave gridSave = new(level.height, level.width);
        GridPos gp = new();
        for (int x = 0; x < gridSave.height; x++)
        {
            gp.x = x;
            for (int y = 0; y < gridSave.width; y++)
            {
                gp.z = y;
                ClickableObject click = level.GetGridItem(gp, false);
                gridSave.grid[x, y] = (click is Building) ? null : click?.Save();
                gridSave.pipes[x, y] = level.GetGridItem(gp, true)?.Save();
            }
        }
        return gridSave;
    }

    public static void Load(GridSave gridSave, GroundLevel templateLevel, int i, List<MinableRes> rockData)
    {
        GroundLevel groundLevel = GameObject.Instantiate(templateLevel, new Vector3(0, ClickableObjectFactory.LEVEL_HEIGHT * i, 0), Quaternion.identity, SceneRefs.gridTiles.transform);
        levels[i] = groundLevel;
        groundLevel.ClearGrid(gridSave.width);
        groundLevel.gameObject.SetActive(i == 2);
        for (int x = 0; x < gridSave.width; x++)
        {
            for (int z = 0; z < gridSave.width; z++)
            {
                switch (gridSave.grid[x, z])
                {
                    case RockSave:
                        SceneRefs.objectFactory.CreateSavedRock(gridSave.grid[x, z] as RockSave, new(x, i, z), rockData);
                        break;
                    case WaterSave:
                        SceneRefs.objectFactory.CreateSavedWater(gridSave.grid[x, z] as WaterSave, new(x, i, z));
                        break;
                    default:
                        SceneRefs.objectFactory.CreateRoad(new(x, i, z), true);
                        break;
                }
            }
        }
    }
    #endregion
}
