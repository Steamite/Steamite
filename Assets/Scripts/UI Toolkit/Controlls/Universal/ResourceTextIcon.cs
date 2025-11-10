using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ResourceTextIcon : VisualElement
{
    Label value;
    VisualElement icon;

    public const int ICON_SIZE = 50;
    [UxmlAttribute] public float scale = 1;
    ResourceType resType;

    public ResourceTextIcon()
    {
        value = new("####");
        Add(value);
        icon = new();
        Add(icon);
    }

    public ResourceTextIcon(float _scale = 1)
    {
        scale = _scale;
        value = new("####");
        Add(value);
        icon = new();
        Add(icon);

        style.maxHeight = new Length(100, LengthUnit.Percent);
        style.minHeight = 60 * scale;
        style.maxHeight = 60 * scale;
        style.flexDirection = FlexDirection.Row;
        style.justifyContent = Justify.SpaceAround;
        style.alignItems = Align.Center;

        value.style.fontSize = 40 * scale;

        icon.style.backgroundImage = ResFluidTypes.None.image;
        icon.style.marginLeft = 10 * scale;
        icon.style.minWidth = ICON_SIZE * scale;
        icon.style.minHeight = ICON_SIZE * scale;
        icon.style.maxWidth = ICON_SIZE * scale;
        icon.style.maxHeight = ICON_SIZE * scale;
        icon.RegisterCallback<PointerEnterEvent>(OnHover);
        icon.RegisterCallback<PointerLeaveEvent>(OnLeave);
    }

    #region Mouse Events
    void OnHover(PointerEnterEvent evt)
    {
        ToolkitUtils.localMenu.UpdateContent(resType, icon);
    }

    void OnLeave(PointerLeaveEvent evt)
    {
        ToolkitUtils.localMenu.Close();
    }
    #endregion
    public void SetTextIcon(string newText, ResourceType type)
    {
        value.text = newText;
        icon.style.unityBackgroundImageTintColor = type.color;
        resType = type;
    }

    public void ColorText(Color color)
    {
        value.style.color = color;
    }

    public void SetText(string newText)
    {
        value.text = newText;
    }
}

