using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class DigReal : GridTilesModeNew
{
    [SerializeField] Color RemoveColor;
    [SerializeField] Color toBeDugColor;
    [SerializeField] Color toBeDugColorRoad;

    [SerializeField] LevelRenderer renderer;
    
    bool drag = false;


    public override void MouseTrack(float distance, float lowHeight, Plane low, Ray _ray)
    {
        int2 lastPos = controller.hoverWorldPosition;

        base.MouseTrack(distance, lowHeight, low, _ray);

        if (drag == true && !lastPos.Equals(controller.hoverWorldPosition))
        {
            CalculateTiles();
        }
    }

    protected override bool CheckTopObstruction(int2 pos, int index, LevelGridTile tile)
    {
        TileType tileType = tile.TType;

        if (tileType == TileType.Wall)
        {
            int worldIndex = SceneRefs.World.GetWorldIndex(pos);

            if (drag)
            {
                controller.SetMaterial(pos, 0, removing ? RemoveColor : toBeDugColor, tileType, index, 0.0001f);
            }
            else
            {
                if (renderer.markedTilesGrid.GetBits(worldIndex) == 1)
                    controller.SetMaterial(pos, 0, RemoveColor, tileType, index, 0.0001f);
                else
                    controller.SetMaterial(pos, 0, toBeDugColor, tileType, index, 0.0001f);
            }

        }
        else
            return false;

        return true;
    }

    protected override bool CheckMidObstruction(int2 pos, int index, LevelGridTile tileType)
    {
        return false;
    }

    protected override void FallbackColor(LevelGridTile tile, int index, int2 worldPosition)
    {
        controller.ClearHoverMaterial();
        controller.hoverWorldPosition = worldPosition;
    }


    public override void DownObject()
    {
        if (drag == false)
            InitDrag();

        CalculateTiles();
    }

    int2 startDragWorldPos;
    int2 lastDragWorldPos;
    bool removing;
    void InitDrag()
    {
        drag = true;
        startDragWorldPos = controller.hoverWorldPosition;
        lastDragWorldPos = startDragWorldPos;
        Debug.Log("startDrag");
        int worldIndex = SceneRefs.World.GetWorldIndex(startDragWorldPos);
        removing = renderer.markedTilesGrid.GetBits(worldIndex) == 1;
        if(removing)
            Debug.Log("removing");
    }

    public override void UpObject()
    {
        drag = false;

        int2 minPos = math.min(startDragWorldPos, lastDragWorldPos);
        int2 maxPos = math.max(startDragWorldPos, lastDragWorldPos);


        int2 position;
        int worldIndex;

        for (int y = minPos.y; y <= maxPos.y; y++)
        {
            position.y = y;
            position.x = minPos.x;

            worldIndex = SceneRefs.World.GetWorldIndex(position);
            for (int x = minPos.x; x <= maxPos.x; x++)
            {
                if (SceneRefs.World.GetActiveTile(x, y).TType == TileType.Wall)
                {
                    renderer.markedTilesGrid.Set(worldIndex, !removing);
                    renderer.levelOverlayGrid[worldIndex] = renderer.levelOverlayGrid[worldIndex].R((byte)(removing ? 0: 255));
                }
                else
                {
                    renderer.levelOverlayGrid[worldIndex] = renderer.levelOverlayGrid[worldIndex].R(0);
                }
                worldIndex++;
            }
        }
        renderer.UpdateOverlayTexture();
        controller.MouseListen();
    }

    void CalculateTiles()
    {
        int2 newPosition = controller.hoverWorldPosition;

        int2 minPos = math.min(startDragWorldPos, newPosition);
        int2 maxPos = math.max(startDragWorldPos, newPosition);

        int2 totMin = math.min(minPos, lastDragWorldPos);
        int2 totMax = math.max(maxPos, lastDragWorldPos);

        int2 position;
        int worldIndex;

        for (int y = totMin.y; y <= totMax.y; y++)
        {
            position.y = y;
            position.x = totMin.x;
            worldIndex = SceneRefs.World.GetWorldIndex(position);

            bool clear = y < minPos.y || y > maxPos.y;
            for (int x = totMin.x; x <= totMax.x; x++)
            {
                bool isUnSet = renderer.markedTilesGrid.GetBits(worldIndex) == 0;
                if (clear || x < minPos.x || x > maxPos.x)
                {
                    if (isUnSet)
                    {
                        renderer.levelOverlayGrid[worldIndex] = renderer.levelOverlayGrid[worldIndex].R(0);// = 0;
                    }
                    else if (removing)
                    {
                        renderer.levelOverlayGrid[worldIndex] = renderer.levelOverlayGrid[worldIndex].R(255);
                    }
                }
                else
                {
                    if (isUnSet)
                        renderer.levelOverlayGrid[worldIndex] = renderer.levelOverlayGrid[worldIndex].R((byte)(removing ? 90 : 200));
                    else if(removing)
                    {
                        renderer.levelOverlayGrid[worldIndex] = renderer.levelOverlayGrid[worldIndex].R(115);
                    }
                }
                worldIndex++;
            }
        }

        renderer.UpdateOverlayTexture();
        lastDragWorldPos = newPosition;
    }
}
