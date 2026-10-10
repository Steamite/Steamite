using Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New;
using Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour, IAfterLoad
{
    InputActionMap cameraMap;
    InputAction move;
    InputAction rotate;
    InputAction zoom;
    InputAction reset;

    InputAction panButton;
    InputAction panMove;
    InputAction isMod;

    [SerializeField] MainShortcuts mainShortcuts;
    [SerializeField] public GameObject mainCamera;

    [SerializeField] MouseController mouseListener;
    [SerializeField] LevelRenderer levelRenderer;


    [Header("General")]
    [SerializeField] float generalSpeed = 3;

    [Header("Movement")]
    [SerializeField] float addMovement = 1f;
    [SerializeField] float removeMovement = 2f;
    [SerializeField] float currentMovementX = 0;
    [SerializeField] float currentMovementY = 0;
    [SerializeField] int maxMovement = 50;

    [Header("Edge Thresholds")]
    [SerializeField] float mouseThreshold = 0.05f;

    [Header("Rotation")]
    [SerializeField] float addRotationY = 2;
    [SerializeField] float removeRotationY = 4;
    [SerializeField] float currentRotationY = 0;
    [SerializeField] int maxRotationY = 50;

    [Header("Zoom")]
    [SerializeField] float minY = 3;
    [SerializeField] float maxY = 20;
    [SerializeField] float zoomStep = 1;
    [SerializeField] float zoomSpeed = 1;
    [SerializeField] float initZoom = 10;
    float currentZoom;
    float targetZoom = 10;

    [Header("Pan")]
    [SerializeField] float panAmmount = 0.1f;

    float mod;
    bool hasMoved = false;

    Transform childCamera;

    LevelRenderer levelRender;

    public void AfterLoad()
    {
        childCamera = transform.GetChild(0);
        childCamera.LookAt(transform);

        cameraMap = mainShortcuts?.inputAsset.actionMaps[0];
        move = cameraMap.FindAction("Move");
        rotate = cameraMap.FindAction("Rotate");
        zoom = cameraMap.FindAction("Zoom");
        reset = cameraMap.FindAction("Reset");

        panButton = cameraMap.FindAction("Drag - Down");
        panMove = cameraMap.FindAction("Drag - Move");
        isMod = mainShortcuts?.inputAsset.actionMaps[1].FindAction("Shift");


        //SceneRefs.CameraSceneMover.SetRaycastMask(SceneRefs.GridTiles.defaultMask);
        SceneRefs.CameraSceneMover.Toggle(true);

        levelRender = GameObject.FindAnyObjectByType<LevelRenderer>();
        mouseListener = GameObject.FindAnyObjectByType<MouseController>();
        enabled = true;
    }

    private void OnEnable()
    {
        cameraMap.Enable();
        Reset();
    }
    private void OnDisable()
    {
        cameraMap.Disable();
    }

    //bool hasChanged = false;
    void Update()
    {
        mod = isMod.IsInProgress() ? 2 : 1;
        //hasChanged = false;

        Move();
        RotZoom();
        if (reset.triggered)
            Reset();

        
        if (hasMoved && levelRender)
        {
            mouseListener.isForcedUpdate = true;
            levelRender.SetCameraPosition(new(transform.position.x, transform.position.z));
        }
        
        hasMoved = false;
    }

    void Reset()
    {        
        transform.SetPositionAndRotation(
            new(
                SceneRefs.World.CenterScaled,
                0,
                SceneRefs.World.CenterScaled
                ), 
            Quaternion.Euler(0, 0, 0)
        );
        currentMovementX = 0;
        currentMovementY = 0;
        currentRotationY = 0;
        targetZoom = initZoom;
        currentZoom = initZoom;

        childCamera.localPosition = new(0, 20, -15);
        childCamera.Translate(0, 0, initZoom);

        hasMoved = true;
        
    }

    void Move()
    {
        Vector2 vec = move.ReadValue<Vector2>();
        Vector2 mouse = Mouse.current.position.value;
        if (UIRefs.TradingWindow.IsOpen)
        {

        }
        else
        {
            /*transform.Translate(
                GetSpeed(
                    ref currentMovementX,
                    addMovement,
                    removeMovement,
                    maxMovement,
                    MergeMove(
                        Edge(
                            mouse.x,
                            Screen.width),
                        vec.x)),
                0,
                GetSpeed(
                    ref currentMovementY,
                    addMovement,
                    removeMovement,
                    maxMovement,
                    MergeMove(
                        Edge(
                            mouse.y,
                            Screen.height),
                        vec.y)));*/

            Vector3 toMove = new(
                GetSpeed(
                    ref currentMovementX,
                    addMovement,
                    removeMovement,
                    maxMovement,
                    MergeMove(
                        Edge(
                            mouse.x,
                            Screen.width),
                        vec.x)),
                0,
                GetSpeed(
                    ref currentMovementY,
                    addMovement,
                    removeMovement,
                    maxMovement,
                    MergeMove(
                        Edge(
                            mouse.y,
                            Screen.height),
                        vec.y)));

            if(toMove.magnitude > 0)
            {
                hasMoved = true;

                toMove = (transform.rotation * toMove) + transform.position;
                /*int gridSize = MyGrid.gridSize(MyGrid.currentLevel);
                toMove.x = Mathf.Clamp(toMove.x, 0, gridSize);
                toMove.z = Mathf.Clamp(toMove.z, 0, gridSize);*/
                transform.position = toMove;
            }
        }

    }

    float MergeMove(float mouse, float key)
    {
        if (Mathf.Abs(mouse) > 0)
        {
            if (mouse > 0)
            {
                if (key < 0)
                    return 0;
                else if (key == 0)
                    return mouse;
            }
            else
            {
                if (key > 0)
                    return 0;
                else if (key == 0)
                    return mouse;
            }
        }
        return key;

    }

    /// <summary>
    /// Handles movement on when mouse is on the edge of the screen.
    /// </summary>
    /// <param name="mouse">Mouse position</param>
    /// <param name="axisSize">Edge threshold.</param>
    /// <returns></returns>
    float Edge(float mouse, float axisSize)
    {
        float div = mouse / axisSize;
        //Debug.Log("Div:" + div);
        if (div < 0 || div > 1)
            return 0;
        else if (div < mouseThreshold)
            return -div;
        else if (div > 1 - mouseThreshold)
            return div;
        return 0;
    }

    /// <summary>
    /// Handles rotation and zoom
    /// </summary>
    void RotZoom()
    {
        float rot = rotate.ReadValue<float>();

        float zoomVal = zoom.ReadValue<Vector2>().y;


        Rot(rot);
        Zoom(zoomVal);
        Pan();
    }

    /// <summary>
    /// Checks pan direction and moves by <see cref="panAmmount"/>.
    /// </summary>
    void Pan()
    {
        if (panButton.inProgress)
        {
            float delta = panMove.ReadValue<Vector2>().y;
            if (delta > 0)
                delta = panAmmount;
            else if (delta < 0)
                delta = -panAmmount;
            if (transform.GetChild(0).localRotation.eulerAngles.x < 10)
                transform.GetChild(0).localRotation = Quaternion.Euler(10, 0, 0);
            else if (transform.GetChild(0).localRotation.eulerAngles.x > 85)
                transform.GetChild(0).localRotation = Quaternion.Euler(85, 0, 0);
            transform.GetChild(0).Rotate(delta, 0, 0);
        }
    }

    /// <summary>
    /// Rotates around the top camera object.
    /// </summary>
    /// <param name="rot">input rotation</param>
    void Rot(float rot)
    {
        var rotAmmount = GetSpeed(ref currentRotationY, addRotationY, removeRotationY, maxRotationY, rot);
        if (Mathf.Abs(rotAmmount) > 0)
        {
            hasMoved = true;
            transform.RotateAround(
                transform.position,
                Vector3.up, rotAmmount);
        }
    }

    /// <summary>
    /// Zoom in and out.
    /// </summary>
    /// <param name="zoom">Input zoom value.</param>
    void Zoom(float zoom)
    {
        zoom = zoom * zoomStep / mod;
        targetZoom = Mathf.Clamp(targetZoom + zoom, minY, maxY);

        if (Mathf.Approximately(targetZoom, currentZoom))
            return;

        float nextZoom = Mathf.Lerp(
            currentZoom, 
            targetZoom, 
            zoomSpeed * Time.deltaTime) - currentZoom;
        currentZoom += nextZoom;

        childCamera.Translate(0, 0, nextZoom);
        /*
        if (zoom > 0 && cam.localPosition.y > minY)
        {
            float x = minY - (cam.localPosition.y - zoom);
            if (x > 0)
                zoom -= x;
            cam.Translate(new(0, 0, zoom));
        }
        else if (zoom < 0 && cam.localPosition.y < maxY)
        {
            float x = (cam.localPosition.y - zoom) - maxY;
            if (x > 0)
                zoom += x;
            cam.Translate(new(0, 0, zoom));
        }*/
    }

    /// <summary>
    /// Handles smooth movement transitions.
    /// </summary>
    /// <param name="currentMovement">Current speed.</param>
    /// <param name="add">Speed to add.</param>
    /// <param name="max">Max speed.</param>
    /// <param name="input">Axis value from input (-1, 1).</param>
    /// <returns>Value to move by.</returns>
    float GetSpeed(ref float currentMovement, float add, float remove, int max, float input)
    {
        float f = Time.unscaledDeltaTime * generalSpeed;
        float fMod = f / mod;
        switch (input)
        {
            // input (speed up to a max)
            case > 0:
                currentMovement = Mathf.Clamp(currentMovement + add * fMod, -max, max);
                break;
            case < 0:
                currentMovement = Mathf.Clamp(currentMovement - add * fMod, -max, max);
                break;
            // no input (slowing down)
            case 0:
                switch (currentMovement)
                {
                    case < 0:
                        currentMovement += remove * f;
                        break;
                    case > 0:
                        currentMovement -= remove * f;
                        break;
                    default:
                        return 0;
                }
                break;
        }
        if (Mathf.Abs(currentMovement) > 0.1f)
        {
            return (f * currentMovement);
        }
        else if (input == 0)
            currentMovement = 0;
        return 0;
    }

}
