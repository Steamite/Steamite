using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public struct CaveinData
{
    public int Stability;
    public int Chance;

    public int Size;
    public int Value;
}

public class Stability : MonoBehaviour
{
    [SerializeField] GroundLevel groundLevel;
    [SerializeField] List<CaveinData> StabilityChanceKeys;

    public void DecreaseStability(Rock rock)
    {
        var changedRockPositions = ChangeStability(rock, false);
        var effectedTiles = changedRockPositions.Select(
            q => groundLevel.GetGridTile(q.x, q.y))
            .ToList();

        for (int i = 0; i < effectedTiles.Count; i++)
        {
            GridTile tile = effectedTiles[i];
            int roll = UnityEngine.Random.Range(0, 101);
            int stability = tile.Stability;
            int threshold = CalculateThreshold(stability);

            if (roll < threshold)
            {
                Cavein(changedRockPositions, i, roll, stability, threshold);
                return;
            }
        }
    }

    private void Cavein(List<Vector2Int> changedRockPositions, int i, int roll, int stability, int threshold)
    {
        Debug.Log(
            $"CAVEIN!!!" +
            $"chance: {threshold}; roll: {roll}" +
            $"stability: {stability}; pos: {changedRockPositions[i]}");
        

    }

    int CalculateThreshold(int stability)
    {
        int threshold = 0;
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
                }
                break;
            }

        }
        return threshold;
    }

    public void IncereaseStability(Rock rock)
        => ChangeStability(rock, true);

    List<Vector2Int> ChangeStability(Rock rock, bool add)
    {
        List<Vector2Int> tiles = new();
        int size = Mathf.RoundToInt(rock.originalIntegrity);
        GridPos center = rock.GetPos();
        int x = (int)center.x;
        int y = (int)center.z;
        for (int i = 1; i < size; i++)
        {
            Line(x, y + size - i, i, add, tiles);
            Line(x, y - size + i, i, add, tiles);
        }
        Line(x, y, size, add, tiles);

        return tiles;
    }

    void Line(int x, int y, int valueOnCenter, bool add, List<Vector2Int> tiles)
    {
        ModifyIntegrity(x, y, valueOnCenter, add, tiles);
        int increaseVal;
        for (int i = 1; i < valueOnCenter; i++)
        {
            increaseVal = valueOnCenter - i;
            ModifyIntegrity(x + i, y, increaseVal, add, tiles);
            ModifyIntegrity(x - i, y, increaseVal, add, tiles);
        }
    }

    void ModifyIntegrity(int x, int y, int change, bool add, List<Vector2Int> tiles)
    {
        if (groundLevel.ChangeGridStability(x, y, change, add))
        {
            tiles.Add(new(x, y));
        }
    }

}