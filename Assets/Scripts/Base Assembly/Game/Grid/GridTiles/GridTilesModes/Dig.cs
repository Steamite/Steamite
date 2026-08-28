using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Dig : GridTilesMode
{
    public Color RemoveColor;

    [SerializeField] public Color ToBeDugColor;

    public override void EnterObject(ClickableObject enterObject)
    {
        Rock _r = enterObject as Rock;
        if (gridTiles.Drag)
        {
            gridTiles.CalcTiles(gridTiles.ActivePos);
            return;
        }
        else if (!_r)
            return;

        if (_r.toBeDug)
            enterObject.Highlight(RemoveColor, false);
        else
            enterObject.Highlight(highlightColor);
    }

    public override void ExitObject(ClickableObject exitObject)
    {
        if (exitObject is Rock rock)
        {
            if (gridTiles.Drag)
                return;
            rock.Highlight(default);
        }
    }

    public override void DownObject()
    {
        if (gridTiles.ActiveObject is Rock rock)
        {
            gridTiles.InitDig(rock);
            gridTiles.Drag = true;
        }
    }

    public override void UpObject()
    {
        if(gridTiles.ActiveObject is Rock rock)
        {
            gridTiles.DigMark(rock);
            gridTiles.Drag = false;
            gridTiles.Enter();
        }
    }

    public override void ExitMod()
    {
        gridTiles.ClearDig();
        gridTiles.Drag = false;
    }
}
