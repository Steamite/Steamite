using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StabilityOverlay : GridTileOverlay, IAfterLoad 
{
    public float MaxIntegrity { private set; get; }

    public void AfterLoad()
    {
        MyGrid.AddToGridChange(UpdateGradientIntegrity);
        UpdateGradientIntegrity(-1, MyGrid.currentLevel);
    }

    void UpdateGradientIntegrity(int _, int newLevel)
    {
        MaxIntegrity = MyGrid.GetGroundLevelData(newLevel).stability.cavein.MaxIntegrity;
    }

    public override float Evaluate(GridTile tile)
    {
        return tile.Stability / MaxIntegrity;// MyGrid.GetGroundLevelData(MyGrid.currentLevel).stability.cavein.MaxIntegrity;// MaxIntegrity;
    }
}

