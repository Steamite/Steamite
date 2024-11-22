using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviour
{
    InputActionMap cameraMap => asset?.inputAsset.actionMaps[0];
    InputAction move => cameraMap.FindAction("Move");
    InputAction rotate => cameraMap.FindAction("Rotate");
    InputAction zoom => cameraMap.FindAction("Zoom");
    InputAction reset => cameraMap.FindAction("Reset");

    InputAction panButton => cameraMap.FindAction("Drag - Down");
    InputAction panMove => cameraMap.FindAction("Drag - Move");

    [SerializeField] MainShortcuts asset;

    [Header("General")]
    /// <summary> multiplier in GetSpeed </summary>
    [SerializeField] float generalSpeed = 3;

    [Header("Movement")]
    [SerializeField] float addMovement = 1f;
    [SerializeField] float removeMovement = 2f;
    [SerializeField] float currentMovementX = 0;
    [SerializeField] float currentMovementY = 0;
    [SerializeField] int maxMovement = 50;

    [Header("Edge Thresholds")]
    [SerializeField] float mouseThreshold = 0.495f;

    [Header("Rotation")]
    [SerializeField] float addRotationY = 2;
    [SerializeField] float removeRotationY = 4;
    [SerializeField] float currentRotationY = 0;
    [SerializeField] int maxRotationY = 50;

    [Header("Zoom")]
    [SerializeField] float minY = 3;
    [SerializeField] float maxY = 20;

    [Header("Pan")]
    [SerializeField] float maxPan = 0.1f;
    
    float mod;

    private void OnEnable()
    {
        currentMovementX = 0;
        currentMovementY = 0;
        currentRotationY = 0;
        transform.GetChild(0).LookAt(transform);
        cameraMap.Enable();
    }
    private void OnDisable()
    {
        cameraMap.Disable();
    }


    void Update()
    {
        mod = Input.GetKey(KeyCode.LeftShift) ? 2 : 1;
        mod *= Time.timeScale;

        Move();
        RotZoom();
        Reset();
    }

    void Reset()
    {
        if (reset.triggered)
        {
            transform.position = new(10, 1, 10);
            transform.rotation = Quaternion.Euler(0,0,0);

            transform.GetChild(0).localPosition = new(0, 20, -15);
            OnEnable();
        }
    }

    void Move()
    {
        Vector2 vec = move.ReadValue<Vector2>();
        Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mouse.x -= 0.5f;
        mouse.y -= 0.5f;
        transform.Translate(
            GetSpeed(ref currentMovementX, addMovement, removeMovement, maxMovement, MergeMove(Edge(mouse.x, mouseThreshold), vec.x)),
            0,
            GetSpeed(ref currentMovementY, addMovement, removeMovement, maxMovement, MergeMove(Edge(mouse.y, mouseThreshold), vec.y)));
        //EdgeMove();
    }
    float MergeMove(float mouse, float key)
    {
        if(Mathf.Abs(mouse) > 0)
        {
            if(mouse > 0)
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
    float Edge(float mouse, float threshold)
    {
        if(Mathf.Abs(mouse) < 0.5f)
        {
            if (mouse > 0)
            {
                mouse -= threshold;
                if (mouse < 0)
                    return 0;
            }
            else if (mouse < 0)
            {
                mouse += threshold;
                if (mouse > 0)
                    return 0;
            }
            return mouse;
        }
        return 0;
    }

    /// <summary>
    /// Handles rotation and zoom
    /// </summary>
    void RotZoom()
    {
        float rot = rotate.ReadValue<float>();
        float zoomVal = zoom.ReadValue<Vector2>().y;
        if (Mathf.Abs(rot) < Mathf.Abs(zoomVal))
            rot = 0;
        else
            zoomVal = 0;
        Rot(rot);
        Zoom(zoomVal);
        Pan();
    }

    void Pan()
    {
        if (panButton.inProgress)
        {
            float delta = panMove.ReadValue<Vector2>().y;
            if (delta > 0)
                delta = maxPan;
            else if (delta < 0)
                delta = -maxPan;
            if (transform.GetChild(0).localRotation.eulerAngles.x < 10)
                transform.GetChild(0).localRotation = Quaternion.Euler(10, 0, 0);
            else if(transform.GetChild(0).localRotation.eulerAngles.x > 85)
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
        transform.RotateAround(
            transform.position,
            Vector3.up,
            GetSpeed(ref currentRotationY, addRotationY, removeRotationY, maxRotationY, rot));
    }

    /// <summary>
    /// Zoom in and out.
    /// </summary>
    /// <param name="zoom">Input zoom value.</param>
    void Zoom(float zoom)
    {
        //print(zoom);
        zoom = zoom * 200 * Time.deltaTime / mod;
        Transform cam = transform.GetChild(0);
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
        }
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
        float f = Time.deltaTime / mod * generalSpeed;
        switch (input)
        {
            case > 0:
                if (currentMovement + add * f < max)
                    currentMovement += add * f;
                else
                    currentMovement = max;
                break;
            case < 0:
                if (currentMovement - add * f > -max)
                    currentMovement -= add * f;
                else
                    currentMovement = -max;
                    
                break;
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
