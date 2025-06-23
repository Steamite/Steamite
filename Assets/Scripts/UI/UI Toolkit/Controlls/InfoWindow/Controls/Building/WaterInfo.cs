using UnityEngine.UIElements;

[UxmlElement]
public partial class WaterInfo : InfoWindowControl
{
    Label waterAmmount;
    public override void Open(object data)
    {
        dataSource = data;
        DataBinding binding = BindingUtil.CreateBinding(nameof(Water.Ammount));
        binding.sourceToUiConverters.AddConverter((ref int ammount) => $"{ammount}");
        SceneRefs.infoWindow.RegisterTempBinding(new(waterAmmount, "text"), binding, data);
    }

    public WaterInfo()
    {
        VisualElement element = new() { name = "Group" };

        VisualElement secElement = new() { name = "Line-Container", style = { marginLeft = new Length(5, LengthUnit.Percent) } }; ;
        secElement.Add(new Label("Water level:"));
        secElement.Add(waterAmmount = new Label("##"));
        element.Add(secElement);
        Add(element);
    }
}
