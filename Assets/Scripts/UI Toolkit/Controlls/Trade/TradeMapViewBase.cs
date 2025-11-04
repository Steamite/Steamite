using UnityEngine.UIElements;

public class TradeMapViewBase : VisualElement
{
    public virtual void Hide()
    {
        parent.style.display = DisplayStyle.None;
    }

    public virtual object Open(int i = 0)
    {
        parent.style.display = DisplayStyle.Flex;
        return null;
    }
}
