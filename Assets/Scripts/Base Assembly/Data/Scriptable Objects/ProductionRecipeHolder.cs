using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "ProductionRecipeHolder", menuName = "Resources/ProductionRecipeHolder", order = 0)]
public class ProductionRecipeHolder : InitializableHolder<ProductionRecipeCategory, ProductionRecipe>
{
    public new const string PATH = "Assets/Game Data/ProductionRecipeHolder.asset";

    public ProductionRecipe GetRecipeByName(string _name)
    {
        return Categories.SelectMany(q => q.Objects).FirstOrDefault(q => q.Name == _name);
    }

    public override void Init()
    {
        foreach (var category in Categories)
        {
            foreach (var obj in category.Objects)
            {
                obj.resourceCost?.Init();
                obj.resourceYield?.Init();
            }
        }
    }
}

