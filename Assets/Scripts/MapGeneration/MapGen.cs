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

    int gridSize;
    int randomSeed;
    MapTile[,] map;
    Color[] resPixels;
    Color[] rockPixels;


    [Header("Resource data")]
    [SerializeField] List<MinableRes> minableResources;


    int minCenter;
    int maxCenter;

    string lastSeed = "";
    /// <summary> Converts input seed into usable values. </summary>
    /// <param name="s">HEX string to convert to a DEC int.</param>
    int HexaToDec(string s)
    {
        int length = s.Length;
        int result = 0;
        for (int i = 0; i < length; i++)
        {
            int cInt = s[i];
            if (cInt >= 48 && cInt <= 57)
            {
                result += (cInt - 48) * (int)Mathf.Pow(16, length - i - 1);
            }
            else if (cInt >= 65 && cInt <= 70)
            {
                result += (cInt - 55) * (int)Mathf.Pow(16, length - i - 1);
            }
            else
            {
                // if incorrect input fix it to a middle value
                result += 8 * (int)Mathf.Pow(16, length - i - 1);
            }
        }
        return result;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Update()
    {
        if (seed != lastSeed)
            Generate();
    }

    void Generate()
    {
        lastSeed = seed;
        ParseInput();

        #region visualization init

        resPixels = new Color[gridSize * gridSize];
        resourceTex = new Texture2D(gridSize, gridSize);
        resRenderer.material.mainTexture = resourceTex;

        rockPixels = new Color[gridSize * gridSize];
        rockTex = new Texture2D(gridSize, gridSize);
        rockRenderer.material.mainTexture = rockTex;

        #endregion

        #region Generation

        map = new MapTile[gridSize, gridSize];
        minCenter = (gridSize / 2) - 5;
        maxCenter = (gridSize / 2) + 5;

        CreateGrid();
        PerlinNoise();

        #endregion

        #region Visualization print

        resourceTex.SetPixels(resPixels);
        resourceTex.Apply();

        rockTex.SetPixels(rockPixels);
        rockTex.Apply();

        #endregion

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Rock r = Instantiate(rockPrefab, new(x, 1.5f, y), Quaternion.identity, rockTransform);
                r.GetComponent<Renderer>().material.color = resPixels[y * gridSize + x];
                r.rockYield = map[x, y].resource;
                r.integrity = map[x, y].hardness;
                r.UniqueID();
            }
        }
    }

    void ParseInput()
    {
        // Input preformating
        bool add = seed.Length < 5;
        while (seed.Length != 5)
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
        gridSize = HexaToDec("" + seed[0] + seed[1]);
        randomSeed = HexaToDec("" + seed[2] + seed[3] + seed[4]);

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
                if (map[x, y] == null)
                {
                    map[x, y] = new();
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
                f = (Mathf.RoundToInt(f) / 20f);
                rockPixels[y * gridSize + x] = new(f, f, f, 1);
            }
        }
    }
    #endregion
}
