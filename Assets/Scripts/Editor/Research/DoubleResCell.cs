using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

[UxmlElement]
public partial class DoubleResCell : VisualElement
{
    readonly ResourceCell resourceCell;
    readonly ResourceCell fluidCell;
    public DoubleResCell() 
    {
        Add(resourceCell = new ResourceCell() { allowedCategories = new() { 1,2,3,4,5,6,7 } });   
        Add(fluidCell = new ResourceCell() { allowedCategories = new() { 8 } });
    }
    public void Open(object data, UnityEngine.Object holder, bool cost)
    {
        resourceCell.style.display = DisplayStyle.Flex;
        fluidCell.style.display = DisplayStyle.Flex;
        switch (data)
        {
            case FluidProductionRecipe fluid:
                if (cost)
                {
                    resourceCell.Open(fluid.resourceCost, holder, true);
                    fluidCell.Open(fluid.fluidCost, holder, true);
                }
                else
                {
                    resourceCell.Open(fluid.resourceYield, holder, false);
                    fluidCell.Open(fluid.fluidYield, holder, false);
                }
                break;
            case ProductionRecipe recipe:
                if (cost)
                {
                    resourceCell.Open(recipe.resourceCost, holder, true);
                }
                else
                {
                    resourceCell.Open(recipe.resourceYield, holder, false);
                }
                fluidCell.style.display = DisplayStyle.None;
                break;
            
        }
    }
}

