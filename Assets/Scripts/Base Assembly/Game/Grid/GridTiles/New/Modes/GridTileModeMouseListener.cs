using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

public abstract partial class GridTilesModeNew : MonoBehaviour
{
    [SerializeField] protected Color hoverColor = new(1, 1, 1);
    Vector3 topPoint;
    Vector3 endPoint;
    Ray ray;


    private void OnDrawGizmos()
    {
        Color c = Color.red;
        c.a = 0.5f;
        Gizmos.color = c;
        //Gizmos.DrawCube(new(0, low.distance, 0), new(1000, 0.1f, 1000));
        Gizmos.DrawLine(ray.origin, topPoint);

        c = Color.blue;
        c.a = 0.5f;
        Gizmos.color = c;

        //Gizmos.DrawCube(new(0, low.distance, 0), new(1000, 0.1f, 1000));
        Gizmos.DrawLine(topPoint, endPoint);
    }

    public virtual void MouseTrack(float distance, float lowHeight, Plane low, Ray _ray)
    {
        int2 pos = new();

        ray = _ray;
        topPoint = ray.GetPoint(distance);
        endPoint = topPoint;

        pos.x = Mathf.FloorToInt(endPoint.x);
        pos.y = Mathf.FloorToInt(endPoint.z);

        var tile = controller.world.GetActiveTile(pos.x, pos.y, out int index);
        if (CheckTopObstruction(pos, index, tile))
            return;

        float3 dir = ray.direction;

        int2 step = new(
            dir.x < 0 ? -1 : 1,
            dir.z < 0 ? -1 : 1);
        float2 modDir = new(
            dir.x < 0 ? 0 : 1,
            dir.z < 0 ? 0 : 1);

        //float step;
        int2 prevPos;
        while (true)
        {
            prevPos = pos;
            pos = MovePosition(pos, modDir, ref endPoint, dir, step);
            if (endPoint.y < lowHeight)
                break;

            tile = controller.world.GetActiveTile(pos.x, pos.y, out index);

            if (CheckTopObstruction(pos, index, tile))
                return;

            if (CheckMidObstruction(pos, index, tile))
                return;
        }

        // mark road
        // NewMethod(tile, index, prevPos);
        FallbackColor(tile, index, prevPos);
    }
    int2 MovePosition(int2 pos, float2 modDir, ref Vector3 endPoint, float3 dir, int2 step)
    {
        float2 diff;
        diff.x = pos.x + modDir.x - endPoint.x;
        diff.y = pos.y + modDir.y - endPoint.z;

        float2 min;
        min.x = diff.x / dir.x;
        min.y = diff.y / dir.z;

        if (min.x < min.y)
        {
            endPoint.x = pos.x + modDir.x;
            endPoint.y += min.x * dir.y;
            endPoint.z += min.x * dir.z;

            pos.x += step.x;
        }
        else
        {
            endPoint.x += min.y * dir.x;
            endPoint.y += min.y * dir.y;
            endPoint.z = pos.y + modDir.y;

            pos.y += step.y;
        }
        return pos;
    }

    protected virtual void FallbackColor(LevelGridTile tile, int index, int2 prevPos)
    {
        controller.SetMaterial(prevPos, 0, hoverColor, (TileType)tile.Type, index, -0.001f);
    }

    protected virtual bool CheckTopObstruction(int2 pos, int index, LevelGridTile tile)
    {
        TileType tileType = (TileType)tile.Type;
        if (tileType < TileType.Road)
        {
            controller.SetMaterial(pos, 0, hoverColor, tileType, index, 0.0001f);
            return true;
        }
        else if (tileType == TileType.Building)
        {
            controller.SetMaterial(pos, 1, hoverColor, tileType, tile.Amount, 0.0001f);
            return true;
        }
        return false;
    }

    protected virtual bool CheckMidObstruction(int2 pos, int index, LevelGridTile tileType)
    {
        TileType tile = (TileType)tileType.Type;
        if (tile == TileType.Road || tile == TileType.Empty)
        {
            // querry a human/chunk spatial hash table

            //SetMaterial()
        }
        return false;
    }
}
