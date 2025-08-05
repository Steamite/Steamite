using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class PipeNetworkControl : VisualElement, IUIElement
{
    TabView tabView;
    Label networkIdLabel;
    public PipeNetworkControl()
    {
        tabView = new() { name = "tabView" };
        Add(tabView);
        //tabView.Add()
        networkIdLabel = new() { name = "networkIdLabel", text = "Network ID: ##" };
        Add(networkIdLabel);
    }

    public void Open(object data)
    {
        switch (data)
        {
            case Pipe:

                break;
            case IFluidWork:

                break;
            default:
                Debug.LogWarning("Should not get here?");
                break;
        }
    }
}
