using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class ProductionRecipe : DataObject
{
    public MoneyResource resourceCost;
    public MoneyResource resourceYield;
    public int timeInTicks;

    public ProductionRecipe(int _id) : base(_id) { }
    public ProductionRecipe() { }
}

[Serializable]
public class ProductionRecipeCategory : DataCategory<ProductionRecipe>
{
    
}