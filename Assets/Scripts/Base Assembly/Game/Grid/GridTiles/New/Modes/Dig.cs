using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class DigNew : GridTilesModeNew
{
    [SerializeField] Color RemoveColor;
    [SerializeField] Color toBeDugColor;
    [SerializeField] Color toBeDugColorRoad;

    bool drag = false;


    public override void MouseTrack(float distance, float lowHeight, Plane low, Ray _ray)
    {
        int2 lastPos = controller.hoverWorldPosition;

        base.MouseTrack(distance, lowHeight, low, _ray);

        if(drag == true && !lastPos.Equals(controller.hoverWorldPosition))
        {
            DownObject();
        }
    }

    protected override bool CheckTopObstruction(int2 pos, int index, LevelGridTile tile)
    {
        TileType tileType = tile.TType;

        if (tileType == TileType.Wall)
            controller.SetMaterial(pos, 0, toBeDugColor, tileType, index, 0.0001f);
        else
            return false;


        return true;
    }

    protected override bool CheckMidObstruction(int2 pos, int index, LevelGridTile tileType)
    {
        return false;
    }

    protected override void FallbackColor(LevelGridTile tile, int index, int2 pos)
    {
        TileType type = tile.TType;
        if (type == TileType.Road || type == TileType.Empty)
            controller.SetMaterial(pos, 0, toBeDugColor, type, index, -0.0001f);

        else
            controller.ClearHoverMaterial();
    }



    public override void DownObject()
    {
        var tile = controller.world.GetActiveTile(
            controller.hoverWorldPosition.x,
            controller.hoverWorldPosition.y,
            out int index);
        HandleSelectionTypes(tile, index, controller.hoverWorldPosition);
        
        drag = true;
    }

    public override void UpObject()
    {
        drag = false;
    }

    void HandleSelectionTypes(LevelGridTile tile, int index, int2 worldPos)
    {
        TileType type = tile.TType;
        switch (type)
        {
            case TileType.Wall:
                controller.DamageClickedRock(type, index);
                break;
            case TileType.Road:
            case TileType.Empty:
                controller.DamageClickedRock(type, index);
                break;
        }
    }
}
