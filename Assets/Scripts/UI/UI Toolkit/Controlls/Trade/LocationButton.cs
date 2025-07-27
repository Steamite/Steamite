using AbstractControls;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class LocationButton : CustomRadioButton
{
    public const int elementSize = 150;
    protected Vector2 pos;
    VisualElement backgroundElem;

    #region Constructors
    public LocationButton()
    {

    }

    public LocationButton(Vector2 _pos, int i, CustomRadioButtonGroup group) : base("location-button", i, group)
    {
        pos = _pos;

        backgroundElem = new();
        Add(backgroundElem);
    }
    #endregion

    public virtual void RecalculateLayout(float zoom)
    {
        style.width = elementSize * zoom;
        style.height = elementSize * zoom;

        style.left = (pos.x * zoom) - (style.width.value.value / 2);
        style.top = (pos.y * zoom) - (style.height.value.value / 2);
    }

    protected override bool SelectChange(bool UpdateGroup)
    {
        base.SelectChange(UpdateGroup);
        backgroundElem.RegisterCallback<TransitionEndEvent>((_) => ToggleInClassList("rotate"));
        AddToClassList("rotate");
        return true;
    }

    public override void Deselect(bool triggerTransition = true)
    {
        base.Deselect(triggerTransition);
        backgroundElem.UnregisterCallback<TransitionEndEvent>((_) => ToggleInClassList("rotate"));
        RemoveFromClassList("rotate");
    }
}
