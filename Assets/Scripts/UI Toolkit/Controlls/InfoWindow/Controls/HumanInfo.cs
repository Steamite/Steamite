using InfoWindowElements;
using UnityEngine.UIElements;

[UxmlElement]
public partial class HumanInfo : InfoWindowControl
{
    Label specialization, efficiency, jobType, jobPosition, jobObject;
    ResourceList inventory;

    public override void Open(object data)
    {
        ((IUIElement)inventory).Open(dataSource);
        specialization.text = ((Human)dataSource).specialization.ToString();

        // Efficiency Binding
        DataBinding binding = BindingUtil.CreateBinding(nameof(Human.Efficiency));
        binding.sourceToUiConverters.AddConverter((ref Efficiency efficiency) => $"{efficiency.efficiency * 100:0.#}%");
        SceneRefs.InfoWindow.RegisterTempBinding(new(efficiency, "text"), binding, dataSource);
    }

    public HumanInfo()
    {
        VisualElement view = new();
        Add(view);
        // Stats
        VisualElement element = new() { name = "Group" };
        element.Add(new Label("Stats") { name = "Header" });

        VisualElement secElement = new() { name = "Line-Container", style = { marginLeft = new Length(5, LengthUnit.Percent) } }; ;
        secElement.Add(new Label("Specialization"));
        secElement.Add(specialization = new Label("Unknown") { name = "Specialization" });
        element.Add(secElement);

        secElement = new() { name = "Line-Container", style = { marginLeft = new Length(5, LengthUnit.Percent) } };
        secElement.Add(new Label("Efficiency"));
        secElement.Add(efficiency = new Label("###%") { name = "Efficiency" });
        element.Add(secElement);
        view.Add(element);

        //inventory
        element = new() { name = "Group" };
        element.Add(new Label("Inventory") { name = "Resource-Header" });
        element.Add(inventory = new ResourceList());
        inventory.verticalPadding = 2;
        Add(element);
    }
}