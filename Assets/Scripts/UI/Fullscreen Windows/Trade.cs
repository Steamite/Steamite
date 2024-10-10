using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trade : FullscreenWindow
{
    StartingLocation location;

    public void Init(StartingLocation _location)
    {
        location = _location;
        MyGrid.sceneReferences.GetComponent<Tick>().timeController.weekEnd += PassiveProduction;
    }

    public void PassiveProduction()
    {
        Resource r = new(-1);
        r.type.Add(ResourceType.Food);
        r.ammount.Add(location.foodProduction*5);
        MyRes.DeliverToElevator(r);
    }
}
