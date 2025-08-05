using ResearchUI;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ResearchInfo : InfoWindowControl
{
    Label title;
    VisualElement image;
    ProgressBar progress;
    bool wasOpened = false;

    public ResearchInfo()
    {
        style.maxHeight = new Length(37.5f, LengthUnit.Percent);
        title = new();
        title.AddToClassList("res-title");
        title.text = "random name";
        Add(title);


        image = new();
        image.AddToClassList("res-image");
        Add(image);

        progress = new();
        progress.AddToClassList("res-progress");
        progress.title = "000/###";
        progress.lowValue = 0;
        progress.value = 0;
        progress.highValue = 999;
        progress[0][0].style.borderBottomLeftRadius = 5;
        progress[0][0].style.borderBottomRightRadius = 5;
        progress[0][0].style.borderTopLeftRadius = 5;
        progress[0][0].style.borderTopRightRadius = 5;
        progress[0][0].style.borderBottomColor = Color.black;
        progress[0][0].style.borderRightColor = Color.black;
        progress[0][0].style.borderLeftColor = Color.black;
        progress[0][0].style.borderTopColor = Color.black;
        progress[0][0].style.backgroundColor = Color.grey;
        Add(progress);
    }

    public override void Open(object data)
    {
        if (wasOpened == false)
        {
            // Research completion can be assigned only once.
            UIRefs.ResearchWindow.researchCompletion += RefillData;
            wasOpened = true;
        }
        RefillData(UIRefs.ResearchWindow.currentResearch);
    }

    /// <summary>
    /// Refils the data to the view.
    /// </summary>
    /// <param name="node">Active node to get the data from.</param>
    void RefillData(ResearchNode node)
    {
        dataSource = node;
        if (node != null)
        {
            image.style.backgroundImage = new(node.preview);
            title.text = node.Name;
            progress.highValue = node.researchTime;

            DataBinding binding = BindingUtil.CreateBinding(nameof(ResearchNode.CurrentTime));
            binding.sourceToUiConverters.AddConverter((ref float dat) => $"{dat:0}/{node.researchTime}");
            SceneRefs.InfoWindow.RegisterTempBinding(new(progress, nameof(ProgressBar.title)), binding, dataSource);

            binding = BindingUtil.CreateBinding(nameof(ResearchNode.CurrentTime));
            SceneRefs.InfoWindow.RegisterTempBinding(new(progress, nameof(ProgressBar.value)), binding, dataSource);
        }
        else
        {
            image.style.backgroundImage = StyleKeyword.Null;
            title.text = "None set";
            progress.title = "";
            progress.value = 0;
        }
    }
}