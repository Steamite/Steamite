using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class House : AssignBuilding
{
    ///////////////////////////////////////////////////
    ///////////////////Overrides///////////////////////
    ///////////////////////////////////////////////////
    #region Window
    protected override void SetupWindow(InfoWindow info, List<string> toEnable)
    {
        base.SetupWindow(info, toEnable);
        info.ToggleChildElems(info.constructed, new() {"assigned"});
    }
    protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
    }
    #endregion
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
