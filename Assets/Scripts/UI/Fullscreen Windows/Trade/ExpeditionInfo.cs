using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
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
            MyGrid.canvasManager.miscellaneous.GetChild(1).GetComponent<LocalInfoWindow>().DisplayInfo(expedition, MyGrid.canvasManager.tradeWindow.transform.InverseTransformPoint(transform.GetComponent<RectTransform>().position));
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (expedition != null)
        {
            mouseOver = false;
            MyGrid.canvasManager.miscellaneous.GetChild(1).GetComponent<LocalInfoWindow>().HideInfo();
            mouseDown = false;
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
            mouseDown = false;
            follow = !follow;
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
            Vector2 vec = transform.position;
            MyGrid.canvasManager.miscellaneous.GetChild(1).GetComponent<LocalInfoWindow>().DisplayInfo(
                expedition, vec);
            if (mouseDown)
            {
                Mouse.current.WarpCursorPosition(transform.position);
            }
        }
    }
}
