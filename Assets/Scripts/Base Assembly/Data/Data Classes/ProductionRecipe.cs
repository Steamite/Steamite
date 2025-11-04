using System;

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