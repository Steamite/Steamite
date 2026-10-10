using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class NothingNew : GridTilesModeNew
{
#if UNITY_EDITOR
    [SerializeField] bool select;
#endif

    public Color SelectionColor => highlightColor * 2;

    public override void EnterObject(int type, int index)
    {/*
        Color color;

        if ( .selected)// if active
            color = SelectionColor; // WHITE
        else
            color = highlightColor; // WHITE / 3

        if (enterObject is Rock r && r.toBeDug) // if rock is to be dug
            color += gridTiles.ToBeDugColor; // DUGCOLOR
        else if (enterObject is Building b && b.Deconstructing)
            color += Color.red / 2;

        enterObject.Highlight(color);*/
    }
/*
    public override void ExitObject(ClickableObject exitObject)
    {
        Color c = default;

        if (exitObject == gridTiles.ActiveObject)
            gridTiles.ActiveObject = null;

        if (exitObject is Building b && b.Deconstructing)
            c += Color.red / 2;

        exitObject.Highlight(c);
    }*/

    public override void DownObject()
    {
        var tile = controller.world.GetActiveTile(controller.hoverMousePosition.x, controller.hoverMousePosition.y, out int index);

        switch (controller.hoverObjectType)
        {
            case TileType.Wall:
                InfoWindow.Window.Open(tile, InfoMode.Rock);

                controller.clickedType = tile.Type;
                controller.clickedIndex = index;
                Debug.Log(tile);
                break;
            case TileType.Road:
            case TileType.Empty:
                InfoWindow.Window.Open(tile, InfoMode.None);
                break;
            case TileType.Building:
            case TileType.Elevator:
                InfoWindow.Window.Open(tile, InfoMode.Rock);
                break;

        }

        /*
        if (gridTiles.ActiveObject is Road road)
            return;
        if (gridTiles.ActiveObject is Rock r && r.Hidden)
            return;

        gridTiles.DeselectObjects();
*/
        /*
                if (gridTiles.SelectedObject is Rock activeRock)
                {
                    if (activeRock && activeRock.toBeDug)
                        c = gridTiles.ToBeDugColor;
                    gridTiles.SelectedObject.Highlight(c);
                    gridTiles.SelectedObject.selected = false;
                }
                if (rock && rock.toBeDug) // rock to be dug
                    c = gridTiles.ToBeDugColor + highlightColor * 2; // YELLOW + RED
                else
                    c = highlightColor * 3; // WHITE
                gridTiles.ActiveObject.Highlight(c);
        */
        /*gridTiles.SelectedObject = gridTiles.ActiveObject;
        gridTiles.SelectedObject.selected = true;
        gridTiles.SelectedObject.OpenWindow();
        gridTiles.Enter();*/

    }

    public override void UpObject()
    {
        // nothing
    }
}
