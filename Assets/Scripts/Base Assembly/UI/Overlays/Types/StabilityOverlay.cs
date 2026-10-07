using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StabilityOverlay : GridTileOverlay, IAfterLoad 
{
    public float MaxIntegrity { private set; get; }

    public void AfterLoad()
    {
        SceneRefs.World.AttachChange(UpdateGradientIntegrity);
    }

    void UpdateGradientIntegrity(int _, int newLevel)
    {
        // TODO
        //MaxIntegrity = GridWorld.GetLevel(newLevel).movementAndStability.cavein.MaxIntegrity;
    }

    public override float Evaluate(GridTile tile)
    {
        return tile.Stability / MaxIntegrity;// MyGrid.GetGroundLevelData(MyGrid.currentLevel).stability.cavein.MaxIntegrity;// MaxIntegrity;
    }
}

