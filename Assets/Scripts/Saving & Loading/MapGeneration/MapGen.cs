using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapGen : MonoBehaviour
{
    [Header("Generation Parameters")]
    [SerializeField] int veinRichness = 1;
    [SerializeField] int veinSize = 1;
    [SerializeField] int veinCount = 1;
    [SerializeField] int randomSeed;
    [SerializeField] string seed = "";

    [Header("Spiral Parameters")]
    [SerializeField] int minToRemove = 2;
    [SerializeField] int maxToRemove = 25;
    [SerializeField] int minToAdd = 1;
    [SerializeField] int maxToAdd = 6;

    [Header("Map params")]
    [SerializeField] int[] gridSizes;

    int gridSize;
    int level;
    MapTile[,] map;
    Color[] resPixels;

    [Header("Resource data")]
    [SerializeField] List<MinableRes> minableResources;
    [SerializeField] Material dirt;


    int minCenter;
    int maxCenter;

    string lastSeed = "";

    public void Generate(string _seed = null, GroundLevel templateLevel = null)
    {
        seed = _seed;
        ParseInput();

        MyGrid.PrepGridLists();
        #region Generation


        minCenter = (gridSize / 2) - 5;
        maxCenter = (gridSize / 2) + 5;
        for(level = 0; level < 5; level++)
        {
            MyGrid.AddEmptyGridLevel(templateLevel, level, gridSize);
            resPixels = new Color[gridSize * gridSize];
            map = new MapTile[gridSize, gridSize];
            CreateGrid();
            PerlinNoise(level);

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (map[x, y] != null)
                        SceneRefs.objectFactory.CreateRock(new(x, level, y), resPixels[y * gridSize + x], map[x, y].resource, map[x, y].hardness, map[x, y].name);
                    else
                    {
                        SceneRefs.objectFactory.CreateRoad(new(x, level, y), true);
                    }
                }
            }
            SceneRefs.objectFactory.CreateElevator(new(gridSize / 2, level, gridSize / 2), level == 2);
        }
        #endregion
    }

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
        gridSize = gridSizes[MyMath.HexToDec("" + seed[0]) % 3];
        veinRichness = MyMath.HexToDec("" + seed[1]) % 3;
        veinSize = MyMath.HexToDec("" + seed[2]) % 3;
        veinCount = MyMath.HexToDec("" + seed[3]) % 3;
        randomSeed = MyMath.HexToDec("" + seed[4] + seed[5] + seed[6] + seed[7]);

        Random.InitState(randomSeed);
    }

    #region Chunks
    void CreateGrid()
    {
        //Visualization
        foreach (MinableRes minable in minableResources)
        {
            int _veinCount = minable.count[level].Value(veinCount);
            for (int i = 0; i < _veinCount; i++)
            {
                CreateAGroup(minable);
            }
        }


    }
    void CreateAGroup(MinableRes minable)
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
            CreateTiles(ref _veinSize, minable, x, z + 1);
    }
    void CreateTiles(ref int numberOfTiles, MinableRes minable, int x, int z)
    {
        int maxX = 0;
        int maxY = 1;
        int rockChance = 100;

        int lastnum = numberOfTiles;
        while (rockChance > 10 && numberOfTiles > 0 && maxX < 15 && maxY < 15)
        {
            maxY++;
            if (Spiral(ref x, ref z, maxY, ref rockChance, ref numberOfTiles, false, -1, minable))
                break;
            maxX++;
            if (Spiral(ref x, ref z, maxX, ref rockChance, ref numberOfTiles, true, 1, minable))
                break;
            if (Spiral(ref x, ref z, maxY, ref rockChance, ref numberOfTiles, false, 1, minable))
                break;
            if (Spiral(ref x, ref z, maxX, ref rockChance, ref numberOfTiles, true, -1, minable))
                break;
            if (lastnum == numberOfTiles)
                numberOfTiles = 0;
        }
    }
    bool Spiral(ref int x, ref int y, int maxCord, ref int rockChance, ref int numberOfTiles, bool changeX, int mod, MinableRes minable)
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
                    resPixels[y * gridSize + x] = minable.color;
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

    bool CanPlace(int x, int y)
    {
        if (map[x, y] == null)
            if (!((x > minCenter && x < maxCenter) && (y > minCenter && y < maxCenter)))
                return true;
        return false;
    }
    bool InBounds(int x, int y)
    {
        return (x > 0 && x < gridSize && y > 0 && y < gridSize);
    }

    #endregion Chunks

    #region Rocks
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
                    resPixels[y * gridSize + x] = new(dirt.color.r / f *2, dirt.color.g / f*2, dirt.color.b / f*2, 1);
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

        switch (f)
        {
            case < 0.2f:
                f = 1;
                break;
            case < 0.4f:
                f = 3;
                break;
            case < 0.6f:
                f = 5;
                break;
            case < 0.8f:
                f = 10;
                break;
            default:
                f = 15;
                break;
        }
        return f;
    }
    #endregion
}
