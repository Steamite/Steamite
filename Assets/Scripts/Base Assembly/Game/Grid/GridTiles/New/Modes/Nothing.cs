using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class NothingNew : GridTilesModeNew
{
    [Header("Colors")]
    [SerializeField] Color hoverColor;
    [SerializeField] Color selectionColor;

    private void Awake()
    {
        SetHoverColor(hoverColor);
    }

    public override void DownObject()
    {
        var tile = controller.world.GetActiveTile(controller.hoverMousePosition.x, controller.hoverMousePosition.y, out int index);

        switch (controller.hoverObjectType)
        {
            case TileType.Wall:
                InfoWindowAdapter.Window.Open(tile, InfoMode.Rock);

                controller.SetSelectionMaterial(controller.hoverMousePosition, 0, selectionColor, 0.0001f);

                controller.clickedType = tile.Type;
                controller.clickedIndex = index;

                Debug.Log(tile);
                break;
            case TileType.Road:
            case TileType.Empty:
                InfoWindowAdapter.Window.Open(tile, InfoMode.None);

                controller.SetSelectionMaterial(controller.hoverMousePosition, 0, selectionColor, -0.0001f);

                controller.clickedType = tile.Type;
                controller.clickedIndex = index;

                break;
            case TileType.Building:
            case TileType.Elevator:
                InfoWindowAdapter.Window.Open(tile, InfoMode.Rock);
                controller.SetSelectionMaterial(controller.hoverMousePosition, 1, selectionColor, +0.0001f);
                break;

        }
    }
}
