using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

[UxmlElement]
public partial class OverlayElem : VisualElement, IInitiableUI
{
    OverlayButtons buttons;
    OverlayGradient gradient;

    public OverlayElem()
    {
        buttons = new();
        Add(buttons);

        gradient = new();
        Add(gradient);

    }


    public void Init()
    {
        buttons.Init();

        SceneRefs.Overlays.overlay.ResetListeners();
        SceneRefs.Overlays.overlay.AddOverlayChanged(OverlayChange);
    }



    private void OverlayChange(int newIndex, BaseOverlay overlay)
    {
        buttons.OutsideTrigger(newIndex);
        gradient.Change(overlay);
    }
}

