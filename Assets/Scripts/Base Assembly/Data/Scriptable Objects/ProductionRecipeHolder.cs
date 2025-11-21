using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductionRecipeHolder", menuName = "Resources/ProductionRecipeHolder", order = 0)]
public class ProductionRecipeHolder : InitializableHolder<ProductionRecipeCategory, ProductionRecipe>
{
    public new const string PATH = "ProductionRecipeHolder";

#if UNITY_EDITOR
    
    public new const string EDITOR_PATH = "Assets/Game Data/ProductionRecipeHolder.asset";

#endif

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

