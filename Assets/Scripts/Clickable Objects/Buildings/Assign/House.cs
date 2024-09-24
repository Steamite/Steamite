using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : AssignBuilding
{
    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can house up to {limit} workers";
        return strings;
    }
    public override void OrderDeconstruct()
    {
        base.OrderDeconstruct();
        if (build.constructed)
        {
            foreach (Human h in assigned)
            {
                h.home = null;
                if (h.nightTime)
                {
                    h.GoHome();
                }
            }
        }
    }
}
