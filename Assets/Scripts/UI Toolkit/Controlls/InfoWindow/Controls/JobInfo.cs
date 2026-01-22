using UnityEngine.UIElements;

[UxmlElement]
public partial class JobInfo : InfoWindowControl
{
    Label jobType, jobPosition, jobObject;

    public override void Open(object data)
    {
        // Job Binding
        DataBinding binding = BindingUtil.CreateBinding(nameof(Human.Job));
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

    public JobInfo()
    {
        // Job
        VisualElement element = new() { name = "Group" };
        element.Add(new Label("Job") { name = "Header" });

        VisualElement secElement = new() { name = "Line-Container", style = { marginLeft = new Length(5, LengthUnit.Percent) } };
        secElement.Add(new Label("Type"));
        secElement.Add(jobType = new Label("Unknown") { name = "Type" });
        element.Add(secElement);

        secElement = new() { name = "Line-Container", style = { marginLeft = new Length(5, LengthUnit.Percent) } };
        secElement.Add(new Label("Position"));
        secElement.Add(jobPosition = new Label("Unknown") { name = "Position" });
        element.Add(secElement);

        secElement = new() { name = "Line-Container", style = { marginLeft = new Length(5, LengthUnit.Percent) } };
        secElement.Add(new Label("Object"));
        secElement.Add(jobObject = new Label("Unknown") { name = "Object" });
        element.Add(secElement);
        Add(element);
    }
}

