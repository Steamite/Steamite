
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.InputSystem;
using static Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.LevelGridTile;
using static UnityEditor.PlayerSettings;

public class MouseListener : MonoBehaviour, IAfterLoad
{
    [SerializeField] float topHeight;
    [SerializeField] float lowHeight;

    /// <summary>
    /// Grid, Building, Human, ....
    /// </summary>
    [SerializeField] Material[] materials;

    Plane low;
    Plane top;

    GridWorld world;

    Vector3 topPoint;
    Vector3 endPoint;
    Ray ray;

    //[SerializeField] bool useJob = false;

    //NativeReference<int2> pos;
    float2 lastPos;


    public bool isForcedUpdate;

    private void Awake()
    {
        enabled = false;

        low = new(Vector3.up, new Vector3(0, lowHeight, 0));
        top = new(Vector3.up, new Vector3(0, topHeight, 0));
    }

    public void AfterLoad()
    {
        world = SceneRefs.World;
        enabled = true;
    }

    private void OnDrawGizmos()
    {
        Color c = Color.red;
        c.a = 0.5f;
        Gizmos.color = c;
        Gizmos.DrawCube(new(0, topHeight, 0), new(1000, 0.1f, 1000));
        Gizmos.DrawLine(ray.origin, topPoint);

        c = Color.blue;
        c.a = 0.5f;
        Gizmos.color = c;
        
        Gizmos.DrawCube(new(0, lowHeight, 0), new(1000, 0.1f, 1000));
        Gizmos.DrawLine(topPoint, endPoint);
    }
    

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.value;
        if (!isForcedUpdate && Mathf.Approximately(mousePos.x, lastPos.x) && Mathf.Approximately(mousePos.y, lastPos.y))
            return;

        lastPos = mousePos;
        isForcedUpdate = false;

        ray = Camera.main.ScreenPointToRay(mousePos);
        top.distance = -topHeight;

        if (!top.Raycast(ray, out float distance))
            return;

        
        Resolution r = Screen.currentResolution;
        BetterMouseTrack(distance);
    }

    void SetMaterial(int2 pos, int matIndex, Color c, int selectionType, float mod = 0)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (i == matIndex)
                continue;
            materials[i].SetFloat("_Range", 0);
        }
        materials[matIndex].SetFloat("_Range", 0.5f + mod);
        materials[matIndex].SetVector("_MousePos", new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f));
        materials[matIndex].SetColor("_HighlightColor", c);
        activeMousePosition = pos;
        hoverTile = selectionType;
    }

    public int2 activeMousePosition;
    int hoverTile;

    void BetterMouseTrack(float distance)
    {
        int2 pos = new();

        int b;
        int i = 0;
        topPoint = ray.GetPoint(distance);
        endPoint = topPoint;

        pos.x = Mathf.FloorToInt(endPoint.x);// ;
        pos.y = Mathf.FloorToInt(endPoint.z);// ;

        var tile = world.GetActiveTile(pos.x, pos.y);
        int tileType = tile.Type;
        if (CheckTopObstruction(pos, tileType))
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

            tile = world.GetActiveTile(pos.x, pos.y);
            tileType = tile.Type;

            if (CheckTopObstruction(pos, tileType))
                return;

            if (CheckMidObstruction(pos, tileType))
                return;
        }

        // mark road
        SetMaterial(prevPos, 0, Color.red * 2, tile.Type, - 0.0002f);
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

    bool CheckTopObstruction(int2 pos, int tileType)
    {
        if (tileType < (int)TileType.Road)
        {
            SetMaterial(pos, 0, Color.blue * 2, tileType, 0.0001f);
            return true;
        }
        else if (tileType == (int)TileType.Building)
        {
            SetMaterial(pos, 1, Color.blue * 2, tileType);
        }
        return false;
    }

    bool CheckMidObstruction(int2 pos, int tileType)
    {
        if(tileType == (int)TileType.Road || tileType == (int)TileType.Empty)
        {
            // querry a human/chunk spatial hash table

            //SetMaterial()
        }
        return false;
    }
}
