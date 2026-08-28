using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
public class Deconstruct : GridTilesMode
{
    public override void EnterObject(ClickableObject enterObject)
    {
        if (enterObject is Building _b)
        {
            Color color;

            if (_b.Deconstructing)
                color = Color.red / 2;
            else
                color = Color.red;

            _b.Highlight(color);
        }
    }

    public override void ExitObject(ClickableObject exitObject)
    {
        if (exitObject is Building b)
        {
            if (b.Deconstructing)
                b.Highlight(Color.red * 0.75f);
            else
                b.Highlight(default);
        }
    }

    public override void DownObject()
    {
        if (gridTiles.ActiveObject is Building building)
        {
            building.OrderDeconstruct();

            Color c;
            if (building && !building.Deconstructing)
                c = highlightColor;
            else
                c = highlightColor / 2;

            building.Highlight(c);
        }
    }
}
