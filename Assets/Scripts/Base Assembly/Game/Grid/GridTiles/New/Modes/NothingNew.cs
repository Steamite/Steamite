using Grid.New;
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
        var tile = controller.world.currentLevel.renderGrid[controller.clickedIndex];
        HandleSelectionTypes(tile, controller.clickedIndex, controller.clickedWorldPosition);
    }

    void HandleSelectionTypes(LevelGridTile tile, int localIndex, int2 worldPos)
    {
        switch ((TileType)tile.Type)
        {
            case TileType.Wall:
                InfoWindowAdapter.Window.Open(localIndex, InfoMode.Rock);

                controller.SetSelectionMaterial(worldPos, 0, selectionColor, 0.0001f);

                controller.clickedWorldPosition = worldPos;
                controller.clickedType = tile.Type;
                controller.clickedIndex = localIndex;
                Debug.Log(tile);
                break;
            case TileType.Road:
            case TileType.Empty:
                InfoWindowAdapter.Window.Open(tile, InfoMode.None);

                controller.SetSelectionMaterial(worldPos, 0, selectionColor, -0.0001f);

                controller.clickedWorldPosition = worldPos;
                controller.clickedType = tile.Type;
                controller.clickedIndex = localIndex;

                GetComponent<IMousePosSetter>().SetPos(worldPos);
                break;
            case TileType.Building:
            case TileType.Elevator:
            case TileType.PassableBuilding:
                InfoWindowAdapter.Window.Open(tile, InfoMode.Rock);
                controller.SetBuildingHighligh(worldPos, false, tile.Amount, selectionColor);
                break;
        }
    }
}
