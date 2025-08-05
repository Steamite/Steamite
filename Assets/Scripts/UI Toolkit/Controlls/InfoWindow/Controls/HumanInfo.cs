using InfoWindowElements;
using UnityEngine.UIElements;

[UxmlElement]
public partial class HumanInfo : InfoWindowControl
{
    Label specialization, efficiency, jobType, jobPosition, jobObject;
    ResList inventory;

    public override void Open(object data)
    {
        ((IUIElement)inventory).Open(dataSource);
        specialization.text = ((Human)dataSource).specialization.ToString();

        // Efficiency Binding
        DataBinding binding = BindingUtil.CreateBinding(nameof(Human.Efficiency));
        binding.sourceToUiConverters.AddConverter((ref Efficiency efficiency) => $"{efficiency.efficiency * 100:0.#}%");
        SceneRefs.InfoWindow.RegisterTempBinding(new(efficiency, "text"), binding, dataSource);

        // Job Binding
        binding = BindingUtil.CreateBinding(nameof(Human.Job));
        binding.sourceToUiConverters.AddConverter((ref JobData jobData) => $"{jobData.job}");
        SceneRefs.InfoWindow.RegisterTempBinding(new(jobType, "text"), binding, dataSource);

        // Pos Binding
        binding = BindingUtil.CreateBinding(nameof(Human.Job));
        binding.sourceToUiConverters.AddConverter((ref JobData jobData) => $"{(jobData.interest ? jobData.interest.GetPos() : "None")}");
        SceneRefs.InfoWindow.RegisterTempBinding(new(jobPosition, "text"), binding, dataSource);

        // Object Binding
        binding = BindingUtil.CreateBinding(nameof(Human.Job));
        binding.sourceToUiConverters.AddConverter((ref JobData jobData) => $"{(jobData.interest ? jobData.interest.objectName : "None")}");
        SceneRefs.InfoWindow.RegisterTempBinding(new(jobObject, "text"), binding, dataSource);
    }

    public HumanInfo()
    {
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
        Add(element);

        // Job
        element = new() { name = "Group" };
        element.Add(new Label("Job") { name = "Header" });

        secElement = new() { name = "Line-Container", style = { marginLeft = new Length(5, LengthUnit.Percent) } }; ;
        secElement.Add(new Label("Type"));
        secElement.Add(jobType = new Label("Unknown") { name = "Type" });
        element.Add(secElement);

        secElement = new() { name = "Line-Container", style = { marginLeft = new Length(5, LengthUnit.Percent) } }; ;
        secElement.Add(new Label("Position"));
        secElement.Add(jobPosition = new Label("Unknown") { name = "Position" });
        element.Add(secElement);

        secElement = new() { name = "Line-Container", style = { marginLeft = new Length(5, LengthUnit.Percent) } }; ;
        secElement.Add(new Label("Object"));
        secElement.Add(jobObject = new Label("Unknown") { name = "Object" });
        element.Add(secElement);
        Add(element);

        //inventory
        Add(new Label("Inventory") { name = "Resource-Header" });
        Add(inventory = new ResList());
        inventory.verticalPadding = 2;
    }
}