using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LightTransport;
using Random = UnityEngine.Random;

/// <summary>Class generating new maps.</summary>
public class MapGenNew : MonoBehaviour
{
    #region Variables
    /// <summary>Determines the richness of the each node richness.</summary>
    [Header("Generation Parameters")][SerializeField] int veinRichness = 1;
    /// <summary>Number of nodes in each vein.</summary>
    [SerializeField] int veinSize = 1;
    /// <summary>Number of veins.</summary>
    [SerializeField] int veinCount = 1;
    /// <summary>Random seed for the random generator.</summary>
    [SerializeField] int randomSeed;
    /// <summary>Seed containing all generation parameters.</summary>
    [SerializeField] string seed = "";

    /// <summary>Minimal to remove chance.</summary>
    [Header("NEG SPIRAL")][SerializeField] int minToRemove = 2;
    /// <summary>Maximal to remove chance.</summary>
    [SerializeField] int maxToRemove = 25;

    /// <summary>Minimal to add chance.</summary>
    [Header("POS SPIRAL")][SerializeField] int minToAdd = 1;
    /// <summary>Maximal to add chance.</summary>
    [SerializeField] int maxToAdd = 6;

    [Header("Map params")][SerializeField] int2[] levelDimensions;

    /// <summary>Chosen size of grid./summary>
    int2 gridSize;
    /// <summary>Number of the generated level./summary>
    int level;
    /// <summary>Level that's being generated at the moment./summary>
    NativeArray<LevelGridTile> map;

    /// <summary>List of resources that are posible to spawn./summary>
    [Header("Resource data")][SerializeField] public List<MinableRes> minableResources;

    [SerializeField] ElevatorData elevator;
    #endregion

    public async Awaitable<WorldSaveNew> Generate(string _seed)
    {
        seed = _seed;
        ParseInput();

        ElevatorData save = elevator;
        save.CreateMainElevator(gridSize.x, gridSize.y);

        WorldSaveNew world = new()
        {
            chunkDimensions = 16,
            objectsSave = new BuildsAndChunksSaveNew(
                    new BuildingDataNew[] {
                        save
                    },
                    new ChunkSave[] { }
                ),
            gridSave = new LevelGridSave[5]
        };

        await MakeGrids(world);


        return world;
    }

    async Awaitable MakeGrids(WorldSaveNew world)
    {
        int2 levelDim = gridSize.x * world.chunkDimensions;
        int levelSize = levelDim.x * levelDim.y;

        await Awaitable.BackgroundThreadAsync();

        for (level = 0; level < MyGrid.NUMBER_OF_LEVELS; level++)
        {
            NativeArray<LevelGridTile> grid = new(levelSize, Allocator.Persistent);
            LevelGridSave levelSave = new(gridSize.x, grid);

            map = grid;
            CreateGrid(levelDim, levelSize);

            world.gridSave[level] = levelSave;
        }
        await Awaitable.MainThreadAsync();
    }

    /// <summary>Extracts parameters from the <see cref="seed"/>.</summary>
    void ParseInput()
    {
        // Input preformating
        bool add = seed.Length < 8;
        while (seed.Length != 8)
        {
            if (add)
            {
                seed += "8";
            }
            else
            {
                seed = seed.Remove(seed.Length - 1);
            }
        }
        seed = seed.ToUpper();

        gridSize = levelDimensions[MyMath.HexToDec("" + seed[0]) % 3];
        veinRichness = MyMath.HexToDec("" + seed[1]) % 3;
        veinSize = MyMath.HexToDec("" + seed[2]) % 3;
        veinCount = MyMath.HexToDec("" + seed[3]) % 3;
        randomSeed = MyMath.HexToDec("" + seed[4] + seed[5] + seed[6] + seed[7]);

        Random.InitState(randomSeed);
    }

    
    /// <summary>Generates veins on a level.</summary>
    void CreateGrid(int2 levelDim, int size)
    {
        int x = 0;
        int y = 0;

        int2 centerRadius = new int2(5, 5);

        int2 center = levelDim / 2;
        int2 minPos = center - centerRadius;
        int2 maxPos = center + centerRadius;


        for (int i = 0; i < size; i++)
        {
            if (x == 0 || y == 0 || x == (levelDim.x-1) || y == (levelDim.y-1))
            {
                // keep outside wall
            }
            else if (x > minPos.x && x < maxPos.x && y > minPos.y && y < maxPos.y)
            {
                map[i] = LevelGridTile.Road;
            }
            else
            {
                /*if (x == 1 && y == 1)
                    map[i] = LevelGridTile.Road;
                else*/
                map[i] = LevelGridTile.Wall(10);
            }

            x++;
            if (x == levelDim.x)
            {
                x = 0;
                y++;
            }
        }
    }
}
    /*
    /// <summary>
    /// Creates a resource vein.
    /// </summary>
    /// <param name="minable">Sets the parameters of the vein.</param>
    void SpiralVein(MinableRes minable)
    {
        int x, z;

        do
        {
            x = Random.Range(1, gridSize);
            z = Random.Range(1, gridSize);
        }
        while (!CanPlace(x, z));
        int _veinSize = minable.size[level].Value(veinSize);

        while (_veinSize > 0)
            AddTiles(ref _veinSize, minable, x, z + 1);
    }

    /// <summary>
    /// Starts a series of placing.
    /// </summary>
    /// <param name="numberOfTiles">Number of tiles that are to be placed.</param>
    /// <param name="minable">Vein parameters.</param>
    /// <param name="x">Position to start.</param>
    /// <param name="z">Position to start</param>
    void AddTiles(ref int numberOfTiles, MinableRes minable, int x, int z)
    {
        int maxX = 0;
        int maxY = 1;
        int rockChance = 100;

        int lastnum = numberOfTiles;
        while (rockChance > 10 && numberOfTiles > 0 && maxX < 15 && maxY < 15)
        {
            maxY++;
            if (SpiralRow(ref x, ref z, maxY, ref rockChance, ref numberOfTiles, false, -1, minable))
                break;
            maxX++;
            if (SpiralRow(ref x, ref z, maxX, ref rockChance, ref numberOfTiles, true, 1, minable))
                break;
            if (SpiralRow(ref x, ref z, maxY, ref rockChance, ref numberOfTiles, false, 1, minable))
                break;
            if (SpiralRow(ref x, ref z, maxX, ref rockChance, ref numberOfTiles, true, -1, minable))
                break;
            if (lastnum == numberOfTiles)
                numberOfTiles = 0;
        }
    }

    /// <summary>
    /// Creates a serie for tiles.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="maxCord">Max Offset</param>
    /// <param name="rockChance">Chance for placing the tile.</param>
    /// <param name="numberOfTiles"><inheritdoc cref="AddTiles(ref int, MinableRes, int, int)"  path="/param[@name='numberOfTiles']"/></param>
    /// <param name="changeX">horizontal/vertical placement</param>
    /// <param name="mod">Value direction</param>
    /// <param name="minable"><inheritdoc cref="AddTiles(ref int, MinableRes, int, int)"  path="/param[@name='minable']"/></param>
    /// <returns></returns>
    bool SpiralRow(ref int x, ref int y, int maxCord, ref int rockChance, ref int numberOfTiles, bool changeX, int mod, MinableRes minable)
    {
        for (int i = 0; i < maxCord; i++)
        {
            int rand = Random.Range(1, 101);
            if (rand < rockChance)
            {
                if (changeX)
                    x += mod;
                else
                    y += mod;
                if (InBounds(x, y) && CanPlace(x, y))
                {
                    map[x, y] = new(minable, minable.richness[level].Value(veinRichness));
                    numberOfTiles--;
                    if (numberOfTiles <= 0)
                        return true;
                    rockChance -= Random.Range(minToRemove, maxToRemove);
                    continue;
                }
            }
            rockChance += Random.Range(minToAdd, maxToAdd);
        }
        return false;
    }

    /// <summary>
    /// Checks if the place is free for placing.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    bool CanPlace(int x, int y)
    {
        if (map[x, y] == null)
        {
            if (!((x > minCenter && x < maxCenter) && (y > minCenter && y < maxCenter)))
                return true;
        }
        return false;
    }
    bool InBounds(int x, int y)
    {
        return (x > 0 && x < gridSize && y > 0 && y < gridSize);
    }

    #endregion Chunks

    #region Integrity
    void PerlinNoise(int level)
    {
        float randomX = Random.Range(-500f, 500f);
        float randomY = Random.Range(-500f, 500f);
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                bool changeColor = false;
                if (map[x, y] == null)
                {
                    changeColor = true;
                    if (CanPlace(x, y))
                        map[x, y] = new();
                    else
                        continue;
                }
                float f = CalculateRockIntegrity(
                    level,
                    Mathf.PerlinNoise(randomX + x / (float)gridSize, randomY + y / (float)gridSize));

                map[x, y].hardness += Mathf.RoundToInt(f);

                if (changeColor)
                    map[x, y].color = new();
            }
        }
    }
    float CalculateRockIntegrity(int level, float f)
    {
        switch (level)
        {
            case 0:
                f = Mathf.Clamp(f, 0, 0.39f);
                break;
            case 1:
                f = Mathf.Clamp(f, 0.2f, 0.59f);
                break;
            case 2:
                f = Mathf.Clamp(f, 0.4f, 0.79f);
                break;
            case 3:
                f = Mathf.Clamp(f, 0.6f, 0.99f);
                break;
            case 4:
                f = Mathf.Clamp(f, 0.7f, 1f);
                break;
        }

        f = f switch
        {
            < 0.2f => 1,
            < 0.4f => 3,
            < 0.6f => 5,
            < 0.8f => 10,
            _ => 15,
        };
        return f;
    }
    #endregion
}
*/