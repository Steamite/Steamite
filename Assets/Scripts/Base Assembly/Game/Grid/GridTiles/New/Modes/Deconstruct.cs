using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World;
using Grid.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
public class DeconstructNew : GridTilesModeNew
{
    protected override bool CheckTopObstruction(int2 pos, int index, LevelGridTile tile)
    {
        TileType tileType = (TileType)tile.Type;
        if (tileType >= TileType.Building)
        {
            controller.SetBuildingHighligh(pos, true, tile.Amount, hoverColor);
            return true;
        }
        return false;
    }


    public override void MouseTrack(float distance, float lowHeight, Plane low, Ray _ray)
    {
        int2 lastPos = controller.hoverWorldPosition;

        base.MouseTrack(distance, lowHeight, low, _ray);
        if (!lastPos.Equals(controller.hoverWorldPosition))
        {
        }
    }

    protected override void FallbackColor(LevelGridTile tile, int index, int2 prevPos)
    {
        controller.hoverWorldPosition = prevPos;
        controller.ClearHoverMaterial(ModesNew.MaterialType.Buildings);
    }

    public override void DownObject()
    {
        if (controller.hoverObjectType < TileType.Building)
            return;
        SceneRefs.World.WorldBuildings.RemoveBuilding(controller.hoverObjectIndex);
        controller.RefreshHover();
    }
}
