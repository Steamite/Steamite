using AbstractControls;
using UnityEngine.UIElements;

public partial class OutpostButton : CustomRadioButton
{
    public OutpostButton()
    {
        AddToClassList("location-button");
        rotator = new();
        style.width = 100;
        style.height = 100;
        style.position = Position.Relative;
        style.marginLeft = 10;
        style.marginRight = 10;
        Add(rotator);
    }

    public OutpostButton(CustomRadioButtonGroup group, int i)
        : base("location-button", i, group)
    {
        rotator = new();
        style.width = 100;
        style.height = 100;
        style.position = Position.Relative;
        style.marginLeft = 10;
        style.marginRight = 10;
        Add(rotator);
    }

    protected override bool SelectChange(bool UpdateGroup)
    {
        base.SelectChange(UpdateGroup);
        return true;
    }

    public override void Deselect(bool triggerTransition = true)
    {
        base.Deselect(triggerTransition);
    }
}