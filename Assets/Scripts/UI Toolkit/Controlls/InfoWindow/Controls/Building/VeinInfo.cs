using InfoWindowElements;
using UnityEngine.UIElements;

[UxmlElement]
public partial class VeinInfo : InfoWindowControl
{
    ResourceList stored;
    public override void Open(object data)
    {
        stored.Open(data);
    }

    public VeinInfo()
    {
        VisualElement element = new() { name = "Group" };
        element.Add(stored = new ResourceList());
        Add(element);
    }
}