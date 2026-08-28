using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class Upgrade : GridTilesMode
{
    public override void EnterObject(ClickableObject enterObject)
    {
        Building building = enterObject as Building;
        if (building == null)
            return;

        if (building.CanUpgrade())
            building.Highlight(Color.darkGreen);
        else
            building.Highlight(Color.red);
    }
    public override void ExitObject(ClickableObject exitObject)
    {
        Building building = exitObject as Building;
        if (building == null)
            building.Highlight(default);// return;
    }

    public override void DownObject()
    {
        Building building = gridTiles.ActiveObject as Building;
        building.StartUpgrade();
    }
}
