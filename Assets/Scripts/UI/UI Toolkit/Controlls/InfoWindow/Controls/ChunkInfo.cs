using InfoWindowElements;
using Newtonsoft.Json.Linq;
using System.Linq;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ChunkInfo : InfoWindowControl
{
    Label assigned;
    ResourceList contains;

    public override void Open(object data)
    {
        // Assigned Binding
        DataBinding binding = BindingUtil.CreateBinding(nameof(Chunk.LocalRes));
        binding.sourceToUiConverters.AddConverter((ref StorageResource res) => $"{(res.carriers.Count > 0 ? res.carriers.First().objectName : "None")}");
        SceneRefs.infoWindow.RegisterTempBinding(new(assigned, "text"), binding, dataSource);


        contains.Open(data);
    }

    public ChunkInfo()
    {
        // Assigned
        VisualElement element = new() { name = "Group" };

        VisualElement secElement = new() { name = "Line-Container" }; ;
        secElement.Add(new Label("Assigned"));
        secElement.Add(assigned = new Label("Unknown"));
        element.Add(secElement);
        Add(element);

        //inventory
        Add(new Label("Contains") { name = "Resource-Header" });
        Add(contains = new ResourceList());
        contains.verticalPadding = 2;
    }
}
