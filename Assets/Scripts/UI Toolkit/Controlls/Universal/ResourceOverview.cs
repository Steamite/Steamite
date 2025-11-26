using AbstractControls;
using Assets.Scripts.UI_Toolkit.Controlls.Universal;
using System.Collections.Generic;
using System.Linq;
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
            resList.Close();// style.display = DisplayStyle.None;
        else
        {
            IResolvedStyle resStyle = buttons[index].resolvedStyle;
            resList.ChangeCategory(index, display, resStyle);
        }
    }

    public void Open(object resDis)
    {
        display = resDis as ResourceDisplay;
        categories = ResFluidTypes.GetData().Categories.Skip(1).SkipLast(1).ToList();
        for (int i = 0; i < categories.ToList().Count; i++)
        {
            CustomRadioButton customRadioButton = new("resource-button", i, this, true) { style = { backgroundImage = categories[i].Icon } };
            customRadioButton.style.backgroundColor = categories[i].color;
        }
        SetChangeCallback(OpenCategory);
        resList = new ResourceOverviewList(categories, 0.5f);
        resList.Open(display);

        resList.AddToClassList("res-overview");
        resList.style.display = DisplayStyle.None;
        Add(resList);
    }
}
