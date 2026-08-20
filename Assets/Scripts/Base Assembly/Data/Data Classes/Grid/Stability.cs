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

    [SerializeField] Cavein cavein;
    
    private void Awake()
    {
        cavein.Init(groundLevel, this);
    }

    public bool ChangeStability(Rock rock, bool add)
    {
        int size = Mathf.RoundToInt(rock.originalIntegrity);
        GridPos center = rock.GetPos();
        int x = (int)center.x;
        int y = (int)center.z;

        return ChangeStability(x, y, size, add);
    }

    public bool ChangeStability(int x, int y, int size, bool add)
    {
        RadiusUtil radiusUtil = new(
            new(x, groundLevel.Level, y), 
            size,
            (x, y, am) => ChangeGrid(x, y, am, add));

        if(add == false)
        {
            return cavein.Cave(new(x, groundLevel.Level, y));
        }
        return true;
    }

    private void ChangeGrid(int x, int y, int size, bool add)
    {
        groundLevel.ChangeGridStability(x, y, size, add);
    }

}