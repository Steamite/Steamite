using AbstractControls;
using System;

public partial class ScrollableRadioList : CustomRadioButtonList
{
    float maxHeight;
    public override void Init(Action<int> onChange)
    {
        base.Init(onChange);
        //contentContainer.RegisterCallback<GeometryChangedEvent>(RecalculateSize);
    }
}