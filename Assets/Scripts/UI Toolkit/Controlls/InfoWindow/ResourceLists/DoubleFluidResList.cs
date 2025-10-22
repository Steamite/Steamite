using InfoWindowElements;
using System;
using System.Linq;
using UnityEngine.UIElements;

public partial class DoubleFluidResList : VisualElement
{
    public DoubleResList resList;
    public DoubleFluidList fluidList;

    public DoubleFluidResList() { }

    public DoubleFluidResList(bool _cost, string _name, bool _useBindings = false, int _leftPadding = 0, bool center = true, int iconSize = 60)
    {
        name = _name;
        style.width = new Length(100, LengthUnit.Percent);
        style.flexGrow = 1;
        resList = new(_cost, _name, _useBindings, center) { leftPadding = _leftPadding, style = { flexGrow = 1 }, iconSize = iconSize };
        Add(resList);
        fluidList = new(_cost, _name, _useBindings, center) { leftPadding = _leftPadding, style = { flexGrow = 1 }, iconSize = iconSize };
        Add(fluidList);
        style.flexDirection = FlexDirection.Column;

        VisualElement element = this;
        while (true)
        {
            element.pickingMode = PickingMode.Ignore;
            if (element.hierarchy.childCount > 0)
                element = element.hierarchy.Children().ToList()[0];
            else
                break;
        }
    }

    public void Open(object data)
    {
        resList.Open(data);
        fluidList.Open(data);
    }
}