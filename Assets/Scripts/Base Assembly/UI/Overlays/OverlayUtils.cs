using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class OverlayUtils
{
    [SerializeField] public static Color pathColor;
    public static Image CreateTile(
        GridPos posInParent, 
        RectTransform parent, 
        string name = "tile")
    {
        RectTransform trans = new GameObject(name, typeof(RectTransform), typeof(Image))
            .GetComponent<RectTransform>();
        trans.SetParent(parent);
        trans.anchoredPosition3D = new(posInParent.x, -posInParent.z, 0);
        trans.sizeDelta = new(1, 1);
        trans.localRotation = Quaternion.Euler(0,0,0);

        return trans.GetComponent<Image>();
    }

    public static RectTransform CreateGroup(RectTransform parent, GridPos gridPos, string name = "group")
    {
        RectTransform trans = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        trans.SetParent(parent, false);
        trans.anchoredPosition = gridPos.ToVecUI();
        return trans.GetComponent<RectTransform>();
    }
}

