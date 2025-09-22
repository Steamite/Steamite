using InfoWindowElements;
using UnityEngine.UIElements;

public partial class DoubleFluidResList : VisualElement
{
    DoubleResList resList;
    DoubleFluidList fluidList;

    public DoubleFluidResList() { }

    public DoubleFluidResList(bool _cost, string _name, bool _useBindings = false, int _leftPadding = 0)
    {
        style.width = new Length(100, LengthUnit.Percent);
        style.flexGrow = 1;
        resList = new(_cost, _name, _useBindings, true) { leftPadding = _leftPadding, style = { flexGrow = 1 } };
        Add(resList);
        fluidList = new(_cost, _name, _useBindings, true) { leftPadding = _leftPadding, style = { flexGrow = 1 } };
        Add(fluidList);
        style.flexDirection = FlexDirection.Column;
    }

    public void Open(object data)
    {
        resList.Open(data);
        fluidList.Open(data);
    }
}