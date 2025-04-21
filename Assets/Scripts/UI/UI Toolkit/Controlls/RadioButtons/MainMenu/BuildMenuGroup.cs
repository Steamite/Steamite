using System.Collections.Generic;
using AbstractControls;
using UnityEngine.UIElements;

public class BuildMenuGroup : CustomRadioButtonList
{
    List<BuildCategWrapper> buildCategWrappers;
    protected override void DefaultBindItem(VisualElement element, int index)
    {
        base.DefaultBindItem(element, index);
        // ((Button)element).iconImage = 
    }
}
