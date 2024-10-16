using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PreBuildInfo : UIBehaviour
{
    RectTransform rectTransform;
    Canvas c;
    GridPos gridPos = new();
    status s = status.notSet;
    enum status{
        notSet,
        set
    }
    public void SetUp()
    {
        rectTransform = (RectTransform)transform;
        c = transform.parent.parent.parent.GetComponent<Canvas>();
        s = status.set;
    }
    protected override void OnRectTransformDimensionsChange()
    {
        if(s == status.set)
        {
            float canvasHeight = c.renderingDisplaySize.y;// * c.scaleFactor;
            //print(canvasHeight);
            rectTransform.anchoredPosition = new(gridPos.x, (gridPos.z + ((canvasHeight / 13) + (rectTransform.rect.height / 2)))* c.scaleFactor);
            
            transform.gameObject.SetActive(true);
        }
    }
    public void DisplayInfo(Building prefab, Vector3 pos)
    {
        // triggers on button enter, works after setingUp
        if(s == status.set)
        {
            gridPos = new(pos.x, pos.y);
            print(prefab.name);
            transform.GetChild(0).GetComponent<TMP_Text>().text = prefab.name;
            transform.GetChild(1).GetComponent<TMP_Text>().text = string.Join('\n', prefab.GetInfoText());
            transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
        }
    }
    void OnApplicationPause()
    {
        HideInfo();
    }
    public void HideInfo()
    {
        gameObject.SetActive(false);
    }
}
