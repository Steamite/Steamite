using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class NothingNew : GridTilesModeNew
{
    [Header("Colors")]
    [SerializeField] Color selectionColor;

    public override void DownObject()
    {
        var tile = controller.world.GetActiveTile(
            controller.hoverWorldPosition.x, 
            controller.hoverWorldPosition.y, 
            out int index);
        HandleSelectionTypes(tile, index, controller.hoverWorldPosition);
    }

    public void RefreshDownedTile()
    {
        var tile = controller.world.gridLevels[0].renderGrid[controller.clickedIndex];
        HandleSelectionTypes(tile, controller.clickedIndex, controller.clickedWorldPosition);
    }

    void HandleSelectionTypes(LevelGridTile tile, int index, int2 worldPos)
    {
        switch ((TileType)tile.Type)
        {
            case TileType.Wall:
                InfoWindowAdapter.Window.Open(tile, InfoMode.Rock);

                controller.SetSelectionMaterial(worldPos, 0, selectionColor, 0.0001f);

                controller.clickedWorldPosition = worldPos;
                controller.clickedType = tile.Type;
                controller.clickedIndex = index;
                Debug.Log(tile);
                break;
            case TileType.Road:
            case TileType.Empty:
                InfoWindowAdapter.Window.Open(tile, InfoMode.None);

                controller.SetSelectionMaterial(worldPos, 0, selectionColor, -0.0001f);

                controller.clickedWorldPosition = worldPos;
                controller.clickedType = tile.Type;
                controller.clickedIndex = index;
                break;
            case TileType.Building:
            case TileType.Elevator:
                InfoWindowAdapter.Window.Open(tile, InfoMode.Rock);
                controller.SetBuildingHighligh(worldPos, false, tile.Amount, selectionColor);
                break;
        }
    }
}
