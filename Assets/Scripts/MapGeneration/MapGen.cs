using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapGen : MonoBehaviour
{
    Texture2D resourceTex;
    Texture2D rockTex;
    [SerializeField] Renderer resRenderer;
    [SerializeField] Renderer rockRenderer;

    [SerializeField] Transform rockTransform;
    [SerializeField] Rock rockPrefab;

    [Header("Seed")]
    [SerializeField] string seed = "";

    [Header("Parameters")]
    [SerializeField] int minToRemove = 2;
    [SerializeField] int maxToRemove = 25;
    [SerializeField] int minToAdd = 1;
    [SerializeField] int maxToAdd = 6;

    [Header("Map params")]
    [SerializeField] int[] gridSizes;


    int gridSize;
    int randomSeed;
    MapTile[,] map;
    Color[] resPixels;
    Color[] rockPixels;


    [Header("Resource data")]
    [SerializeField] List<MinableRes> minableResources;
    [SerializeField] Material dirt;


    int minCenter;
    int maxCenter;

    string lastSeed = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created

#if UNITY_EDITOR
    void Update()
    {
        if (seed != lastSeed && resRenderer)
            Generate();
    }
#endif

    // TODO
    public void Generate(string _seed = null, GroundLevel templateLevel = null)
    {
        seed = _seed;
        ParseInput();

        MyGrid.PrepGridLists();
        #region Generation


        minCenter = (gridSize / 2) - 5;
        maxCenter = (gridSize / 2) + 5;
        for(int i = 0; i < 5; i++)
        {
            MyGrid.AddEmptyGridLevel(templateLevel, i, gridSize);
            resPixels = new Color[gridSize * gridSize];
            rockPixels = new Color[gridSize * gridSize];
            map = new MapTile[gridSize, gridSize];
            CreateGrid();
            PerlinNoise();

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (map[x, y] != null)
                        SceneRefs.objectFactory.CreateRock(new(x, i, y), resPixels[y * gridSize + x], map[x, y].resource, map[x, y].hardness, map[x, y].name);
                    else
                    {
                        SceneRefs.objectFactory.CreateRoad(new(x, i, y), true);
                    }
                }
            }
            SceneRefs.objectFactory.CreateElevator(new(gridSize / 2, i, gridSize / 2), i == 2);
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
        //gridSize = MyMath.HexToDec("" + seed[1]);
        randomSeed = MyMath.HexToDec("" + seed[4] + seed[5] + seed[6] + seed[7]);

        Random.InitState(randomSeed);
    }

    #region Chunks
    void CreateGrid()
    {
        //Visualization
        foreach (MinableRes minable in minableResources)
        {
            int rand = Random.Range(minable.minGroups, minable.maxGroups);
            for (int i = 0; i < rand; i++)
            {
                CreateAGroup(minable);
            }
        }


    }
    void CreateAGroup(MinableRes minable)
    {
        int x, y;

        do
        {
            x = Random.Range(1, gridSize);
            y = Random.Range(1, gridSize);
        }
        while (!CanPlace(x, y));
        int numberOfTiles = Random.Range(minable.minNodes, minable.maxNodes);

        while (numberOfTiles > 0)
            CreateTiles(ref numberOfTiles, minable, x, y + 1);
    }
    void CreateTiles(ref int numberOfTiles, MinableRes minable, int x, int y)
    {
        int maxX = 0;
        int maxY = 1;
        int rockChance = 100;

        int lastnum = numberOfTiles;
        while (rockChance > 10 && numberOfTiles > 0 && maxX < 15 && maxY < 15)
        {
            maxY++;
            if (Spiral(ref x, ref y, maxY, ref rockChance, ref numberOfTiles, false, -1, minable))
                break;
            maxX++;
            if (Spiral(ref x, ref y, maxX, ref rockChance, ref numberOfTiles, true, 1, minable))
                break;
            if (Spiral(ref x, ref y, maxY, ref rockChance, ref numberOfTiles, false, 1, minable))
                break;
            if (Spiral(ref x, ref y, maxX, ref rockChance, ref numberOfTiles, true, -1, minable))
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
                    map[x, y] = new(minable);
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
    void PerlinNoise()
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
                float f = Mathf.PerlinNoise(randomX + x / (float)gridSize, randomY + y / (float)gridSize);
                if (f < 0.2f)
                    f = 1;
                else if (f < 0.4f)
                    f = 3;
                else if (f < 0.6f)
                    f = 5;
                else if (f < 0.8f)
                    f = 10;
                else
                    f = 15;
                map[x, y].hardness += Mathf.RoundToInt(f);

                if (changeColor)
                    resPixels[y * gridSize + x] = new(dirt.color.r / f *2, dirt.color.g / f*2, dirt.color.b / f*2, 1);
            }
        }
    }
    #endregion
}
