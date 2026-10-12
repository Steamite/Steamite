using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

public class DigReal : GridTilesModeNew
{
    [SerializeField] Color RemoveColor;
    [SerializeField] Color toBeDugColor;
    [SerializeField] Color toBeDugColorRoad;

    [SerializeField] Texture2D toDigTexture;
    
    bool drag = false;

    public override void InitMode(MouseController _mouseController)
    {
        base.InitMode(_mouseController);
        toDigTexture = new(SceneRefs.World.LargestLevelDimensions, SceneRefs.World.LargestLevelDimensions);
    }


    public override void MouseTrack(float distance, float lowHeight, Plane low, Ray _ray)
    {
        int2 lastPos = controller.hoverWorldPosition;

        base.MouseTrack(distance, lowHeight, low, _ray);

        if (drag == true && !lastPos.Equals(controller.hoverWorldPosition))
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
        controller.ClearHoverMaterial();
    }



    public override void DownObject()
    {
        var tile = controller.world.GetActiveTile(
            controller.hoverWorldPosition.x,
            controller.hoverWorldPosition.y,
            out int index);
        
        if (drag == false)
            InitDrag();

        HandleSelectionTypes(tile, index, controller.hoverWorldPosition);
    }

    void InitDrag()
    {
        drag = true;
    }

    public override void UpObject()
    {
        drag = false;
        // EndCalc
    }

    void CalculateChanges()
    {

    }

    void HandleSelectionTypes(LevelGridTile tile, int index, int2 worldPos)
    {
        TileType type = tile.TType;
        switch (type)
        {
            case TileType.Wall:
                
                break;
        }
    }
}
