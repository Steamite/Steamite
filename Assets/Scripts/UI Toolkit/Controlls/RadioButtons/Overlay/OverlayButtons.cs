using AbstractControls;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

[UxmlElement]
public partial class OverlayButtons : ShortcutRadioButtonGroup, IInitiableUI
{
    public OverlayButtons() : base()
    {
        
    }

    public void Init()
    {
        SetChangeCallback((i) => SceneRefs.Overlays.overlay.ChangeOverlay(i));

        var overlayTypes = SceneRefs.Overlays.overlay.GetButtonOverlayTypes();
        for (int i = 0; i < overlayTypes.Count; i++)
        {
            CustomRadioButton button = new("overlay-button", i, this, true);
            button.AddToClassList("");
            button.iconImage = Background.FromSprite(overlayTypes[i].sprite);

            Add(button);
        }
    }
}