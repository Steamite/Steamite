using UnityEngine.UIElements;

public abstract class InfoWindowControl : VisualElement, IUIElement
{
    public abstract void Open(object data);

    public InfoWindowControl()
    {
        style.flexGrow = 1;
    }
}

