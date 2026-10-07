
using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class MouseListener : MonoBehaviour, IAfterLoad
{
    [SerializeField] float topHeight;
    [SerializeField] float lowHeight;

    [SerializeField] Material material;

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

    void SetMaterial(int2 pos, Color c, float mod)
    {
        material.SetFloat("_Range", 0.5f + mod); //(b == 0 ? -0.0001f : ));
        material.SetVector("_MousePos", new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f));
        material.SetColor("_HighlightColor", c);
        activeMousePosition = pos;
    }

    int2 activeMousePosition;
    void BetterMouseTrack(float distance)
    {
        int2 pos = new();

        int b;
        int i = 0;
        topPoint = ray.GetPoint(distance);
        endPoint = topPoint;

        pos.x = Mathf.FloorToInt(endPoint.x);// ;
        pos.y = Mathf.FloorToInt(endPoint.z);// ;

        b = world.GetActiveTile(pos.x, pos.y).Type;
        if (b < 2)
        {
            SetMaterial(pos, Color.blue * 2, 0.0001f);
            return;
        }

        float3 dir = ray.direction;
        float2 diff;
        float2 min;

        int2 step = new(
            dir.x < 0 ? -1 : 1,
            dir.z < 0 ? -1 : 1);
        float2 modDir = new(
            dir.x < 0 ? 0 : 1,
            dir.z < 0 ? 0 : 1);

        //float step;

        while (true) 
        {
            diff.x = pos.x + modDir.x - endPoint.x;
            diff.y = pos.y + modDir.y - endPoint.z;

            min.x = diff.x / dir.x;
            min.y = diff.y / dir.z;

            if (min.x < min.y)
            {
                endPoint.x = pos.x + modDir.x;
                endPoint.y += min.x * dir.y;
                endPoint.z += min.x * dir.z;
                
                if (endPoint.y < lowHeight)
                    break;

                pos.x += step.x;
            }
            else
            {
                endPoint.x += min.y * dir.x;
                endPoint.y += min.y * dir.y;
                endPoint.z = pos.y + modDir.y;

                if (endPoint.y < lowHeight)
                    break;

                pos.y += step.y;
            }
            

            b = world.GetActiveTile(pos.x, pos.y).Type;

            if (b < 2)
            {
                SetMaterial(pos, Color.blue * 2, 0.0001f);
                return;
            }

            i++;
            if (i > 10)
                return;
        }
        SetMaterial(pos, Color.red * 2, -0.0001f);
    }

    [SerializeField] InputAction clickAction;
    [SerializeField] InputAction breakAction;

    private void OnEnable()
    {
        clickAction.Enable();
        clickAction.performed += ClickAction_performed;

        breakAction.Enable();
        breakAction.performed += BreakAction_performed;
    }

    private void OnDisable()
    {
        clickAction.performed -= ClickAction_performed;
        clickAction.Disable();
        breakAction.performed -= BreakAction_performed;
        breakAction.Disable();
    }


    private void BreakAction_performed(InputAction.CallbackContext obj)
    {
        // TO deselect
        Debug.Log($"{activeMousePosition} - break");
        world.GetActiveTile(activeMousePosition.x, activeMousePosition.y);
    }

    private void ClickAction_performed(InputAction.CallbackContext obj)
    {
        // TO select
        Debug.Log($"{activeMousePosition} - click - {world.GetActiveTile(activeMousePosition.x, activeMousePosition.y)}");
    }

}
