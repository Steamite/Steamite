using System;
using UnityEngine;


public class Stability : MonoBehaviour
{
    GroundLevel groundLevel;

    public Cavein cavein;

    event Action OnChange;
    public void Init(GroundLevel groundLevel)
    {
        this.groundLevel = groundLevel;
        cavein.Init(groundLevel, this);
        OnChange = null;
    }

    public void RegisterChange(Action a)
        => OnChange += a;

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
            new(x, y), 
            size,
            (x, y, am) => ChangeGrid(x, y, am, add));
        radiusUtil.DoRadius();

        OnChange?.Invoke();
        if (add == false)
        {
            return cavein.Cave(new(x, y));
        }
        return true;
    }

    private void ChangeGrid(int x, int y, int size, bool add)
    {
        groundLevel.ChangeGridStability(x, y, size, add);
    }
}