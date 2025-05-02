using AbstractControls;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public partial class ScrollableRadioList : CustomRadioButtonList
{
    float maxHeight;
    public override void Init(Action<int> onChange)
    {
        base.Init(onChange);
        //contentContainer.RegisterCallback<GeometryChangedEvent>(RecalculateSize);
    }
}