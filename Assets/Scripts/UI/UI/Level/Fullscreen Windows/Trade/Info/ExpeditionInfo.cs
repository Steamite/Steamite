using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ExpeditionInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    TradeExpedition expedition;
    bool mouseOver = false;
    bool mouseDown = false;
    bool follow = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (expedition != null)
        {
            mouseOver = true;
            CanvasManager.miscellaneous.GetChild(1).GetComponent<LocalInfoWindow>().DisplayInfo(expedition, UIRefs.trade.transform.InverseTransformPoint(transform.GetComponent<RectTransform>().position));
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (expedition != null)
        {
            mouseOver = false;
            CanvasManager.miscellaneous.GetChild(1).GetComponent<LocalInfoWindow>().HideInfo();
            follow = false;
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        mouseDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
        if (mouseDown)
        {
            follow = !follow;
            mouseDown = false;
        }
    }

    public void SetExpedition(TradeExpedition _expedition)
    {
        expedition = _expedition;
    }

    public void MoveInfo()
    {
        if (mouseOver)
        {
            CanvasManager.miscellaneous.GetChild(1).GetComponent<LocalInfoWindow>().DisplayInfo(
                expedition, UIRefs.trade.transform.InverseTransformPoint(transform.GetComponent<RectTransform>().position));
            if (follow)
            {
                Mouse.current.WarpCursorPosition(transform.position);
            }
        }
    }
}
