using InfoWindowElements;
using Newtonsoft.Json.Linq;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class RockInfo : InfoWindowControl
{
    Label assigned, integrity;
    ResourceList yeild;

    public override void Open(object data)
    {
        // Assigned Binding
        DataBinding binding = BindingUtil.CreateBinding(nameof(Rock.Assigned));
        binding.sourceToUiConverters.AddConverter((ref Human human) => $"{(human ? human.objectName : "None")}");
        SceneRefs.infoWindow.RegisterTempBinding(new(assigned, "text"), binding, dataSource);

        // Integrity Binding
        binding = BindingUtil.CreateBinding(nameof(Rock.Integrity));
        binding.sourceToUiConverters.AddConverter((ref float integrity) => $"{integrity:0.#}");
        SceneRefs.infoWindow.RegisterTempBinding(new(integrity, "text"), binding, dataSource);
        yeild.Open(data);
    }

    public RockInfo()
    {
        // Assigned
        VisualElement element = new() { name = "Group" };

        VisualElement secElement = new() { name = "Line-Container" }; ;
        secElement.Add(new Label("Assigned"));
        secElement.Add(assigned = new Label("Unknown"));
        element.Add(secElement);

        secElement = new() { name = "Line-Container" };
        secElement.Add(new Label("Integrity"));
        secElement.Add(integrity = new Label("#.#"));
        element.Add(secElement);
        Add(element);

        //inventory
        Add(new Label("Yeild") { name = "Resource-Header" });
        Add(yeild = new ResourceList());
        yeild.verticalPadding = 2;
    }
}
