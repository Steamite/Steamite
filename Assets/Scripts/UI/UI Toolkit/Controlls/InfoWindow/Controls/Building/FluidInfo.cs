using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

struct FluidContainerElem
{
    public VisualElement container;
    public VisualElement filledMask;
    public Label emptyLabel;
    public Label filledLabel;
}

[UxmlElement]
public partial class FluidInfo : InfoWindowControl
{
    List<FluidContainerElem> containers;
    int size = 100;
    [UxmlAttribute]
    int Size
    {
        get => size;
        set
        {
            size = value;
            containers.ForEach(
                q =>
                {
                    q.container.style.width = size;
                    q.container.style.height = size;
                    q.filledMask.style.width = size;
                    q.filledMask.style.height = size / 2;
                    q.emptyLabel.style.width = size;
                    q.emptyLabel.style.height = size;
                    q.filledLabel.style.width = size;
                    q.filledLabel.style.height = size;
                });
        }
    }
    public override void Open(object data)
    {
        Clear();
        containers = new();
        switch (data)
        {
            case IFluidWork:
                Fluid fluid = (data as IFluidWork).StoredFluids;
                dataSource = data;
                for (int i = 0; i < fluid.types.Count; i++)
                {
                    FluidType type = fluid.types[i];
                    containers.Add(CreateFluidIcon());
                    containers[i].filledMask.style.color
                        = ToolkitUtils.resSkins.GetResourceColor(type);

                    var t = type;
                    var container = containers[i];
                    var x = i;
                    DataBinding binding = BindingUtil.CreateBinding(nameof(IFluidWork.StoredFluids));
                    binding.sourceToUiConverters.AddConverter((ref Fluid flu) =>
                    {
                        Debug.Log((float)flu[t] / flu.capacities[x] * size);
                        return new StyleLength(flu[t] / (float)flu.capacities[x] * size);
                    });
                    SceneRefs.infoWindow.RegisterTempBinding(new BindingContext(container.filledMask, "style." + nameof(VisualElement.style.height)), binding, data);

                    binding = BindingUtil.CreateBinding(nameof(IFluidWork.StoredFluids));
                    binding.sourceToUiConverters.AddConverter((ref Fluid flu) => flu[t].ToString());
                    SceneRefs.infoWindow.RegisterTempBinding(new BindingContext(container.filledLabel, "text"), binding, data);

                    binding = BindingUtil.CreateBinding(nameof(IFluidWork.StoredFluids));
                    binding.sourceToUiConverters.AddConverter((ref Fluid flu) => flu[t].ToString());
                    SceneRefs.infoWindow.RegisterTempBinding(new BindingContext(container.emptyLabel, "text"), binding, data);
                }
                break;
        }
    }

    public FluidInfo()
    {
        containers = new() { CreateFluidIcon(), CreateFluidIcon() };
    }

    FluidContainerElem CreateFluidIcon()
    {
        Label emptyLabel, filledLabel;
        VisualElement container = new()
        {
            name = "container",
            style =
            {
                width = size,
                height = size
            }
        };

        VisualElement mask = new() { name = "empty-mask" };
        mask.Add(emptyLabel = new Label("##")
        {
            name = "empty-label",
            style =
            {
                width = size,
                height = size
            }
        });
        container.Add(mask);

        mask = new() { name = "filled-mask" };
        mask.Add(filledLabel = new Label("##")
        {
            name = "filled-label",
            style =
            {
                width = size,
                height = size
            }
        });
        container.Add(mask);
        Add(container);
        return new FluidContainerElem() { container = container, filledMask = mask, emptyLabel = emptyLabel, filledLabel = filledLabel };
    }
    // void Create
}
