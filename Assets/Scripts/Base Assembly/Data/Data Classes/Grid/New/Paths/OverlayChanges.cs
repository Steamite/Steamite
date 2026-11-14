using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class OverlayChanges
{
    /// <summary>
    /// Entrance overlay Tile
    /// </summary>
    /// <param name="world"></param>
    /// <param name="levelRenderer"></param>
    /// <param name="add"></param>
    public static void SetEntrance(int2 world, bool add, ref LevelComponent level, ref OverlayComponent overlay)
    {
        int localIndex = Recalculation.LocalIndexFromWorldPos(world, level.offsetsScaled, level.dimensionsScaled);
        var gridTile = level.renderGrid[localIndex];

        uint entranceCount = gridTile.otherData & 0x3;
        if (!add)
        {
            entranceCount--;
            gridTile.otherData--;
            level.renderGrid[localIndex] = gridTile;
        }

        if (gridTile.TType == TileType.Road)
        {
            if (entranceCount == 0)
            {
                int worldIndex = SceneRefs.World.GetWorldIndex(world);
                overlay.overlayGrid[worldIndex] = overlay.overlayGrid[worldIndex].G((byte)(add ? 255 : 0));
                
                
                Debug.Log($"Place Entrance on {world}");
            }
        }

        if (add)
        {
            gridTile.otherData++;
            level.renderGrid[localIndex] = gridTile;
        }
    }

    public static void CleanPosAndUpdate(int gridIndex, LevelComponent level, OverlayComponent overlay)
    {
        int worldPosition = Recalculation.WorldIndexFromLocalIndex(gridIndex, level.offsetsScaled, level.dimensionsScaled);// WorldIndexFromLocalIndex(gridIndex);
        overlay.overlayGrid[worldPosition] = overlay.overlayGrid[worldPosition].R(0);
        overlay.markedTilesGrid.Set(worldPosition, false);
    }
}