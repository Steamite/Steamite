using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraSceneMovement : MonoBehaviour
{
    [SerializeField] PhysicsRaycaster raycaster;
    public void MoveToPosition(GridPos pos, bool setCursor = false)
    {
        transform.position = pos.ToVec();
        if (setCursor)
            SetCursor(pos);
    }
    public void SetCursor(GridPos pos)
    {
        Vector3 vector = Camera.main.WorldToScreenPoint(pos.ToVec());
        Mouse.current.WarpCursorPosition(vector);
    }
    public void SetRaycastMask(LayerMask mask)
    {
        raycaster.eventMask = mask;
    }
}
