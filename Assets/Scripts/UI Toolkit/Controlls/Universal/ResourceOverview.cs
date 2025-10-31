using AbstractControls;
using Assets.Scripts.UI_Toolkit.Controlls.Universal;
using InfoWindowElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ResourceOverview : CustomRadioButtonGroup, IUIElement
{
    ResourceDisplay display;
    List<ResourceTypeCategory> categories;
    ResourceOverviewList resList;
    public ResourceOverview() : base()
    {
        
    }

    void OpenCategory(int index)
    {
        if (index == -1)
            resList.style.display = DisplayStyle.None;
        else
        {
            resList.style.display = DisplayStyle.Flex;
            IResolvedStyle resStyle = buttons[index].resolvedStyle;
            resList.style.left = (resStyle.left + resStyle.width / 2) - resList.width/2;
            resList.style.top = resolvedStyle.top + resolvedStyle.height + 10;
            resList.ChangeCategory(index, display);
        }
    }

    public void Open(object resDis)
    {
        display = resDis as ResourceDisplay;
        categories = ResFluidTypes.GetData().Categories.Skip(1).ToList();
        for (int i = 0; i < categories.ToList().Count; i++)
        {
            CustomRadioButton customRadioButton = new("resource-button", i, this, true) { style = { backgroundImage = categories[i].Icon } };
            customRadioButton.style.backgroundColor = categories[i].color;
        }
        SetChangeCallback(OpenCategory);
        resList = new ResourceOverviewList(categories, 0.5f);
        resList.Open(display);

        resList.AddToClassList("res-overview-group");
        resList.style.display = DisplayStyle.None;
        Add(resList);
    }
}
