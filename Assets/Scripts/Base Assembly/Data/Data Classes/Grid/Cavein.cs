using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class Cavein
{
    [SerializeField] List<CaveinData> StabilityChanceKeys;

    GroundLevel Level;
    Stability Stability;

    public void Init(GroundLevel groundLevel, Stability stability)
    {
        Level = groundLevel;
        Stability = stability;
    }

    public bool Cave(Vector3Int center)
    {
        var tile = Level.GetGridTile(center.x, center.z);
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

        for (int j = 0; j < StabilityChanceKeys.Count; j++)
        {
            float stabilityKey = StabilityChanceKeys[j].Stability;
            if (stability <= stabilityKey)
            {
                if (j == 0)
                    threshold = StabilityChanceKeys[j].Chance;
                else
                {
                    int prevX = StabilityChanceKeys[j - 1].Stability;
                    stabilityKey -= prevX;
                    stability -= prevX;
                    float t = stability / stabilityKey;

                    threshold = Mathf.CeilToInt(
                        Mathf.Lerp(
                            StabilityChanceKeys[j - 1].Chance,
                            StabilityChanceKeys[j].Chance,
                            t));

                    index = j;
                }
                break;
            }
        }
        return threshold;
    }

    private void CaveDestroy(Vector3Int position, int roll, int index, int stability, int threshold)
    {
        Debug.Log(
            $"CAVEIN!!!" +
            $"chance: {threshold}; roll: {roll}" +
            $"stability: {stability}; pos: {position}");

        int size = StabilityChanceKeys[index].Size;
        int integrity = StabilityChanceKeys[index].IntegrityGain;

        int x = position.x;
        int y = position.y;

        RadiusUtil util = new(
            position, 
            size,
            CaveinModifier);
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
