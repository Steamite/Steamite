using Params;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGeneration : MonoBehaviour, IToolkitController
{
    VisualElement mapParams;

    bool changeSeed = true;

    #region generation props
    string seed = "";
    [CreateProperty]
    public string Seed
    {
        get { return seed; }
        set
        {
            UpdateEnums(value);
            seed = value;
        }
    }

    int mapSize = 1;
    [CreateProperty]
    public int MapSize
    {
        get { return mapSize; }
        set
        {
            mapSize = value;
            UpdateSeed(0, value);
        }
    }

    int veinSize = 1;
    [CreateProperty]
    public int VeinSize
    {
        get { return veinSize; }
        set
        {
            veinSize = value;
            UpdateSeed(1, value);
        }
    }

    int veinRichness = 1;
    [CreateProperty]
    public int VeinRichness
    {
        get { return veinSize; }
        set
        {
            veinSize = value;
            UpdateSeed(2, value);
        }
    }

    int veinCount = 1;
    [CreateProperty]
    public int VeinCount
    {
        get { return veinCount; }
        set
        {
            veinCount = value;
            UpdateSeed(3, value);
        }
    }


    #endregion

    List<EnumGeneratioParameter> parameters;

    public void Init(VisualElement _root)
    {
        mapParams = _root.Q<VisualElement>("Map-Parameters");

        seed = mapParams.Q<StringGenerationParameter>("Seed").Link(this);

        parameters = new();

        // map size
        parameters.Add(mapParams.Q<EnumGeneratioParameter>("Map-Size"));
        mapSize = parameters[^1].Link(this);

        // vein size
        parameters.Add(mapParams.Q<EnumGeneratioParameter>("Size"));
        veinSize = parameters[^1].Link(this);

        // vein richness
        parameters.Add(mapParams.Q<EnumGeneratioParameter>("Richness"));
        veinRichness = parameters[^1].Link(this);

        // vein count
        parameters.Add(mapParams.Q<EnumGeneratioParameter>("Number"));
        veinCount = parameters[^1].Link(this);
    }

    void UpdateSeed(int index, int value)
    {
        if (changeSeed)
        {
            string _seed = "";
            for (int i = 0; i < seed.Length; i++)
            {
                if (i == index)
                {
                    int r = (UnityEngine.Random.Range(0, 5) * 3) + value;
                    if (r > 9)
                    {
                        _seed += (char)(r + 55);
                    }
                    else
                        _seed += r;
                    continue;
                }
                _seed += seed[i];
            }

            seed = _seed;
            mapParams.Q<StringGenerationParameter>("Seed").Change(_seed);
        }
    }

    void UpdateEnums(string s)
    {
        for(int i = 0; i < 4 && i < s.Length; i++)
        {
            changeSeed = false;
            parameters[i].Change(MyMath.HexToEnum(s[i]));
            changeSeed = true;
        }
    }
}
