using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Basic highlight color(for selection).</summary>
public static class MyGrid
{
    #region Variables
    /// <summary>Number of Levels in game.</summary>
    public const int NUMBER_OF_LEVELS = 5;


    public static List<Pipe> Pipes => pipes;
    static List<Pipe> pipes = new();

    /// <inheritdoc cref="buildings"/>
    public static List<Building> Buildings => buildings;
    /// <summary>List of all buildings on all levels.</summary>
    static List<Building> buildings = new();
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
    public static int gridSize(int level) => levels[level].height;
    #endregion

    [RuntimeInitializeOnLoadMethod]
    static void ReloadDomain() => GridChange = null;

    /// <summary>
    /// Links <see cref="GridChange"/> events. And changes the active level.
    /// </summary>
    public static void Init()
    {
        GridChange += (int _, int _) => SceneRefs.GridTiles.ChangeSelMode(ControlMode.nothing);
        GridChange += (int _, int _) => SceneRefs.GridTiles.Exit(SceneRefs.GridTiles.activeObject);
        GridChange += (int i, int newI) => SceneRefs.Humans.SwitchLevel(i, newI);
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

    public static List<Building> GetBuildings(Func<Building, bool> predicate) => Buildings.Where(predicate).ToList();
    public static List<T> GetBuildings<T>(Func<T, bool> predicate) => Buildings.Where(q => q is T).Cast<T>().Where(predicate).ToList();
    public static Building GetBuilding(Func<Building, bool> predicate) => Buildings.FirstOrDefault(predicate);
    public static Pipe GetPipes(Func<Pipe, bool> predicate) => Pipes.FirstOrDefault(predicate);
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
        pipes = new();
    }
    #endregion Grid Access

    #region Grid Updating
    /// <summary>
    /// Updates the building grid.
    /// </summary>
    /// <param name="building"></param>
    /// <param name="load"></param>
    public static void SetBuilding(Building building, bool load = false)
    {
        if (building is Pipe)
        {
            pipes.Add(building as Pipe);
        }
        else
        {
            GridPos gridPos = building.GetPos();
            levels[gridPos.y].RegisterBuilding(building, gridPos, load);
        }
    }

    public static void UnsetRock(Rock rock)
    {
        SceneRefs.JobQueue.toBeDug.Remove(rock); // removes from list
        SceneRefs.GridTiles.tempMarkedTiles.Remove(rock);
        SceneRefs.GridTiles.DestroyUnselect(rock);
        GameObject.Destroy(rock.gameObject);
    }

    public static void UnsetBuilding(Building building)
    {
        GridPos gridPos = building.GetPos();
        if (building is Pipe)
        {
            pipes.Remove(building as Pipe);
            if (building.id != -1 || levels[gridPos.y].GetGridItem(gridPos, true)?.id == -1)
                levels[gridPos.y].SetGridItem(gridPos, null, true);
        }
        else
        {
            SceneRefs.GridTiles.DestroyUnselect(building);
            if (building.blueprint.itemList.Count > 0)
            {
                levels[gridPos.y].UnsetBuilding(building, gridPos);
            }
            buildings.Remove(building);
        }
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
        GridPos gridPos = building.GetPos();
        if (building is Pipe)
            return levels[gridPos.y].CanPlace(building as Pipe, gridPos);
        else
            return levels[gridPos.y].CanPlace(building, gridPos);
    }

    #endregion Checking

    #region Level References
    // Gives References to part of the requested level.

    public static Transform FindLevelChunks(int lIndex) => levels[lIndex].chunks;
    public static Transform FindLevelRoads(int lIndex) => levels[lIndex].roads;
    public static Transform FindLevelWater(int lIndex) => levels[lIndex].waters;
    public static Transform FindLevelRocks(int lIndex) => levels[lIndex].rocks;
    public static Transform FindLevelBuildings(int lIndex) => levels[lIndex].buildings;
    public static Transform FindLevelPipes(int lIndex) => levels[lIndex].pipes;

    public static Transform FindLevelChunks() => levels[currentLevel].chunks;
    public static Transform FindLevelRoads() => levels[currentLevel].roads;
    public static Transform FindLevelWater() => levels[currentLevel].waters;
    public static Transform FindLevelRocks() => levels[currentLevel].rocks;
    public static Transform FindLevelBuildings() => levels[currentLevel].buildings;
    public static Transform FindLevelPipes() => levels[currentLevel].pipes;



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
                gp = new(offset.x, offset.z);
                break;
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
                ClickableObject click = level.GetGridItem(gp);
                gridSave.grid[x, y] = (click is Building) ? null : click?.Save();
                gridSave.pipes[x, y] = level.GetGridItem(gp, true)?.Save();
            }
        }
        return gridSave;
    }

    public static void Load(GridSave gridSave, GroundLevel templateLevel, int i, List<MinableRes> rockData, Material dirtMat)
    {
        GroundLevel groundLevel = GameObject.Instantiate(templateLevel, new Vector3(0, ClickableObjectFactory.LEVEL_HEIGHT * i, 0), Quaternion.identity, SceneRefs.GridTiles.transform);
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
                        SceneRefs.ObjectFactory.CreateSavedRock(gridSave.grid[x, z] as RockSave, new(x, i, z), rockData, dirtMat);
                        break;
                    case WaterSave:
                        SceneRefs.ObjectFactory.CreateSavedWater(gridSave.grid[x, z] as WaterSave, new(x, i, z));
                        break;
                    default:
                        SceneRefs.ObjectFactory.CreateRoad(new(x, i, z), true);
                        break;
                }
                if (gridSave.pipes[x, z] != null)
                {
                    SceneRefs.ObjectFactory.CreateSavedPipe(gridSave.pipes[x, z], new GridPos(x, i, z));
                }
            }
        }
    }

    #endregion
}
