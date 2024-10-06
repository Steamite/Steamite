using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class House : AssignBuilding
{
    ///////////////////////////////////////////////////
    ///////////////////Overrides///////////////////////
    ///////////////////////////////////////////////////
    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info = null;
        // if selected
        if ((info = base.OpenWindow(setUp)) != null)
        {
            // if to be setup
            if (setUp)
            {
                info.cTransform.GetChild(info.cTransform.childCount - 1).GetComponent<TMP_Text>().text = $"Occupancy: {assigned.Count} / {limit}";
                info.cTransform.GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "Tenants"; // worker table
            }
            // update
        }
        return info;
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
                    h.Night();
                }
            }
        }
    }

    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can house up to {limit} workers";
        return strings;
    }
}
