using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct CaveinData
{
    public int Stability;
    public int Chance;
    public int Size;
    [FormerlySerializedAs("Value")]
    public int IntegrityGain;
}

[Serializable]
public class Cavein
{
    [SerializeField] List<CaveinData> stabilityChanceKeys;


    GroundLevel Level;
    Stability Stability;


    public List<CaveinData> StabilityChanceKeys => stabilityChanceKeys;
    public float MaxIntegrity => stabilityChanceKeys[^1].Stability;

    public void Init(GroundLevel groundLevel, Stability stability)
    {
        Level = groundLevel;
        Stability = stability;
    }

    public bool Cave(Vector2Int center)
    {
        var tile = Level.GetGridTile(center.x, center.y);
        int roll = UnityEngine.Random.Range(0, 101);
        int stability = tile.Stability;
        int threshold = CalculateThreshold(stability, out int index);

        if (roll < threshold)
        {
            CaveDestroy(center, roll, index, stability, threshold);
            return true;
        }
        return false;
    }


    int CalculateThreshold(int stability, out int index)
    {
        int threshold = 0;
        index = 0;

        for (int j = 0; j < stabilityChanceKeys.Count; j++)
        {
            float stabilityKey = stabilityChanceKeys[j].Stability;
            if (stability <= stabilityKey)
            {
                if (j == 0)
                    threshold = stabilityChanceKeys[j].Chance;
                else
                {
                    int prevX = stabilityChanceKeys[j - 1].Stability;
                    stabilityKey -= prevX;
                    stability -= prevX;
                    float t = stability / stabilityKey;

                    threshold = Mathf.CeilToInt(
                        Mathf.Lerp(
                            stabilityChanceKeys[j - 1].Chance,
                            stabilityChanceKeys[j].Chance,
                            t));

                    index = j;
                }
                break;
            }
        }
        return threshold;
    }

    private void CaveDestroy(Vector2Int position, int roll, int index, int stability, int threshold)
    {
        Debug.Log(
            $"CAVEIN!!!" +
            $"chance: {threshold}; roll: {roll}" +
            $"stability: {stability}; pos: {position}");

        int size = stabilityChanceKeys[index].Size;
        int integrity = stabilityChanceKeys[index].IntegrityGain;
/*
        int x = position.x;
        int y = position.y;*/

        RadiusUtil util = new(
            position, 
            size,
            CaveinModifier);
        util.DoRadius();
    }


    void CaveinModifier(int x, int y, int increaseVal)
    {
        if (!Level.CheckBounds(x, y) || increaseVal <= 0)
            return;
        GridTile tile = Level.GetGridTile(x, y);
        if (tile.TileBase is Rock rock)
        {
            rock.originalIntegrity += increaseVal;
            rock.Integrity += increaseVal;
            Stability.ChangeStability(x, y, increaseVal, true);
        }
        else
        {
            Rock r = SceneRefs.ObjectFactory.CreateRock(
                new(x, y),
                new(),
                new(),
                increaseVal,
                "Dirt");

            r.Unhide();
        }
    }
}
