using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class PipeInfo : InfoWindowControl
{
    Label networkId;
    ListView connectedNetworks;
    public override void Open(object data)
    {
        FluidNetwork network = ((Pipe)data).network;
        networkId.text = network.networkID.ToString();

        connectedNetworks.itemsSource = network.buildings;
        /*networkName.value = network.networkName;
        networkName.RegisterValueChangedCallback(q => 
        {
            network.networkName = q.newValue;
        });*/
    }

    public PipeInfo()
    {
        VisualElement element = new() { name = "Group" };
        VisualElement secElement = new() { name = "Line-Container" }; ;
        secElement.Add(new Label("Network id"));
        secElement.Add(networkId = new Label("##"));
        element.Add(secElement);

        /*networkName = new TextField("Network name", 25, false, false, '|');
        element.Add(networkName);*/

        connectedNetworks = new()
        {
            makeItem =
                () =>
                {
                    return new Label()
                    {
                        style =
                        {
                            unityTextAlign = TextAnchor.MiddleCenter,
                            backgroundColor = new Color(0,0,0,0)

                        }
                    };
                },
            bindItem =
                (el, i) =>
                {
                    (el as Label).text = ((Building)connectedNetworks.itemsSource[i]).ToString();
                },
            selectionType = SelectionType.None
        };
        element.Add(connectedNetworks);

        Add(element);
    }
}
