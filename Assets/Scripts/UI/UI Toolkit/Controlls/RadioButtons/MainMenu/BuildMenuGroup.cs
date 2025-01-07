using AbstractControls;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildMenuGroup : CustomRadioButtonGroup
{
    List<BuildCategWrapper> buildCategWrappers;
    protected override void DefaultBindItem(VisualElement element, int index)
    {
        base.DefaultBindItem(element, index);
       // ((Button)element).iconImage = 
    }
}
