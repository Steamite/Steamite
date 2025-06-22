using InfoWindowElements;
using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ConstructionInfo : InfoWindowControl
{
    Label progress, status;
    DoubleResourceListWithEvent resourceList;
    Button button;
    public override void Open(object data)
    {
        Building building = (Building)data;
        DataBinding binding = BindingUtil.CreateBinding(nameof(Building.constructionProgress));
        binding.sourceToUiConverters.AddConverter((ref float progress) => $"{(progress / building.maximalProgress) * 100:0}%");
        SceneRefs.infoWindow.RegisterTempBinding(new BindingContext(progress, "text"), binding, building);

        if (building.deconstructing)
        {
            resourceList.style.display = DisplayStyle.None;
            UpdateConstructionText(building);
        }
        else
        {
            resourceList.style.display = DisplayStyle.Flex;
            UpdateConstructionText(building);
            resourceList.Open(new Tuple<Building, Action<bool>>(building,
                    (_) =>
                    {
                        UpdateConstructionText(building);
                    }));
        }
    }

    public ConstructionInfo()
    {
        // Assigned
        VisualElement element = new() 
        { name = "Group", 
            style = 
            { 
                marginBottom=10
            } 
        };

        element.Add(status = new Label("No state") 
        { 
            style = 
            { 
                unityTextAlign = TextAnchor.MiddleCenter, 
                fontSize = 30, 
                marginBottom = 20 
            } 
        });
        Add(element);

        VisualElement secElement = new() { name = "Line-Container" }; ;
        secElement.Add(new Label("Construction Progress"));
        secElement.Add(progress = new Label("###%"));
        element.Add(secElement);


        //inventory
        Add(resourceList = new DoubleResourceListWithEvent() 
        { 
            cost = true, 
            useBindings = true, 
            showMoney = false, 
            verticalPadding = 2, 
            style = 
            { 
                maxWidth = StyleKeyword.Auto
            } 
        });
        Add(button = new Button()
        {
            text = "Empty"
        });
        button.AddToClassList("main-button");
        button.clicked +=
            () =>
            {
                Building b = (Building)dataSource;
                b.OrderDeconstruct();
                if (b != null)
                    UpdateConstructionText(b);
            };
    }


    void UpdateConstructionText(Building building)
    {
        if (building.constructed)
        {
            if (building.deconstructing)
            {
                status.text = "Deconstructing";
                button.text = "Cancel deconstruction";
            }
            else
            {
                status.text = "Reconstructing";
                button.text = "Deconstruct";
            }
        }
        else
        {
            if (building.deconstructing)
            {
                status.text = "Removing construction";
                button.text = "Continue construction";
            }
            else
            {
                if (MyRes.DiffRes(building.Cost, building.LocalRes).Sum() > 0)
                    status.text = "Waiting for resources";
                else
                    status.text = "Constructing";
                button.text = "Cancel construction";
            }
        }
    }
}
