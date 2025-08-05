using AbstractControls;
using TradeWindowElements;
using UnityEngine;
using UnityEngine.UIElements;

public partial class OutpostButton : CustomRadioButton
{
    VisualElement backgroundElem;
    public OutpostButton()
    {
        AddToClassList("location-button");
        backgroundElem = new();
        style.width = 100;
        style.height = 100;
        style.position = Position.Relative;
        style.marginLeft = 10;
        style.marginRight = 10;
        Add(backgroundElem);
    }

    public OutpostButton(CustomRadioButtonGroup group, int i) 
        : base("location-button", i, group)
    {
        backgroundElem = new();
        style.width = 100;
        style.height = 100;
        style.position = Position.Relative;
        style.marginLeft = 10;
        style.marginRight = 10;
        Add(backgroundElem);
    }

    protected override bool SelectChange(bool UpdateGroup)
    {
        base.SelectChange(UpdateGroup);
        backgroundElem.RegisterCallback<TransitionEndEvent>(Rotate);
        AddToClassList("rotate");
        return true;
    }

    public override void Deselect(bool triggerTransition = true)
    {
        base.Deselect(triggerTransition);
        Debug.LogWarning("dasdsadasdsa");
        backgroundElem.UnregisterCallback<TransitionEndEvent>(Rotate);
        RemoveFromClassList("rotate");
    }
}