using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
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
        image.RegisterCallback<PointerEnterEvent>(Enter);
        image.RegisterCallback<PointerMoveEvent>(Move);

        image.RegisterCallback<PointerLeaveEvent>(EndMove);

        labels = new();

        CreateLabel();
        CreateLabel();

        style.display = DisplayStyle.None;
    }

    private void Enter(PointerEnterEvent evt)
    {
        LocalMenuUtility.LocalMenuController.OpenUI(
                new GradientMouseData(0),
                this);
    }

    private void EndMove(PointerLeaveEvent evt)
    {
        LocalMenuUtility.LocalMenuController.Close(this);
    }

    private void Move(PointerMoveEvent evt)
    {
        var x = evt.localPosition.x / resolvedStyle.width;
        
        Debug.Log($"{resolvedStyle.width}; ({evt.localPosition.x}); ({x})");

        if (SceneRefs.Overlays.overlay.ActiveOverlay is StabilityOverlay stability)
        {
            var val = Mathf.RoundToInt(Mathf.Lerp(
                0, 
                stability.MaxIntegrity, 
                x));

            LocalMenuUtility.LocalMenuController.OpenUI(
                new GradientMouseData(val), 
                this,
                true);
        }
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
