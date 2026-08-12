using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

public partial class OverlayGradient : VisualElement
{
    VisualElement image;
    VisualElement labelContainer;
    List<Label> labels;


    public OverlayGradient()
    {
        style.flexGrow = 0;
        Add(labelContainer = new()
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween
            }
        });

        Add(image = new()
        {

        });
        image.AddToClassList("overlay-gradient-image");

        labels = new();

        CreateLabel();
        CreateLabel();

        style.display = DisplayStyle.None;
    }

    void CreateLabel()
    {
        Label l;
        labelContainer.Add(l = new());
        
        labels.Add(l);
    }


    public void Change(BaseOverlay overlay)
    {
        if(overlay is StabilityOverlay stability)
        {
            style.display = DisplayStyle.Flex;

            image.style.backgroundImage = Background.FromTexture2D(SceneRefs.Overlays.overlay.GradientTexture);
            labels[0].text = "0";
            labels[1].text = stability.MaxIntegrity.ToString();
        }
        else
        {
            style.display = DisplayStyle.None;
        }
    }
}
