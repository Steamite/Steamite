using System;
using System.Collections.Generic;
using System.Linq;
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

public class Stability : MonoBehaviour
{
    [SerializeField] GroundLevel groundLevel;
    [SerializeField] List<CaveinData> StabilityChanceKeys;

    public bool DecreaseStability(Rock rock)
    {
        var changedRockPositions = ChangeStability(rock, false);
        
        /*var effectedTiles = changedRockPositions.Select(
            q => groundLevel.GetGridTile(q.x, q.y))
            .ToList();

        for (int i = 0; i < effectedTiles.Count; i++)
        {
            GridTile tile = effectedTiles[i];
            int roll = UnityEngine.Random.Range(0, 101);
            int stability = tile.Stability;
            int threshold = CalculateThreshold(stability, out int index);

            if (roll < threshold)
            {
                Cavein(changedRockPositions[i], roll, index, stability, threshold);
                return true;
            }
        }*/
        
        GridPos pos = rock.GetPos();
        return Cave(pos);
    }
    bool Cave(GridPos pos)
    {
        var tile = groundLevel.GetGridTile((int)pos.x, (int)pos.z);
        int roll = UnityEngine.Random.Range(0, 101);
        int stability = tile.Stability;
        int threshold = CalculateThreshold(stability, out int index);

        if (roll < threshold)
        {
            Cavein(new((int)pos.x, (int)pos.z), roll, index, stability, threshold);
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

    private void Cavein(Vector2Int position, int roll, int index, int stability, int threshold)
    {
        Debug.Log(
            $"CAVEIN!!!" +
            $"chance: {threshold}; roll: {roll}" +
            $"stability: {stability}; pos: {position}");

        int size = StabilityChanceKeys[index].Size;
        int integrity = StabilityChanceKeys[index].IntegrityGain;

        int x = position.x;
        int y = position.y;

        for (int i = 1; i < size; i++)
        {
            LineCavein(x, y + size - i, i, integrity - (size - i));
            LineCavein(x, y - size + i, i, integrity - (size - i));
        }
        LineCavein(x, y, size, integrity);
    }
  

    void LineCavein(int x, int y, int valueOnCenter, int integrity)
    {
        CaveinModifier(x, y, integrity);
        int increaseVal;
        for (int i = 1; i < valueOnCenter; i++)
        {
            increaseVal = integrity - i;
            CaveinModifier(x + i, y, increaseVal);
            CaveinModifier(x - i, y, increaseVal);
        }
    }

    void CaveinModifier(int x, int y, int increaseVal)
    {
        if (!groundLevel.CheckBounds(x, y) || increaseVal <= 0)
            return;
        GridTile tile = groundLevel.GetGridTile(x, y);
        if (tile.TileBase is Rock rock)
        {
            rock.originalIntegrity += increaseVal;
            rock.Integrity += increaseVal;
            ManageStability(x, y, increaseVal, true);
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

    void ManageStability(int x, int y, int size, bool add)
    {
        List<Vector2Int> tiles = new();
        for (int i = 1; i < size; i++)
        {
            LineStability(x, y + size - i, i, add, tiles);
            LineStability(x, y - size + i, i, add, tiles);
        }
        LineStability(x, y, size, add, tiles);
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
            LineStability(x, y + size - i, i, add, tiles);
            LineStability(x, y - size + i, i, add, tiles);
        }
        LineStability(x, y, size, add, tiles);
        return tiles;
    }

    void LineStability(
        int x, 
        int y, 
        int valueOnCenter, 
        bool add, 
        List<Vector2Int> tiles)
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