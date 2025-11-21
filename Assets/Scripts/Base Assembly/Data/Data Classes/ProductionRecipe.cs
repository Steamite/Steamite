using System;

[Serializable]
public class ProductionRecipe : DataObject
{
    public MoneyResource resourceCost = new();
    public MoneyResource resourceYield = new();
    public int timeInTicks;

    public ProductionRecipe(ProductionRecipe recipe) : base(recipe)
    {
        resourceCost = recipe.resourceCost;
        resourceYield = recipe.resourceYield;
        timeInTicks = recipe.timeInTicks;
    }

    public ProductionRecipe(int _id) : base(_id) { }
    public ProductionRecipe() { }
}



[Serializable]
public class FluidProductionRecipe : ProductionRecipe
{
    public Resource fluidCost = new();
    public Resource fluidYield = new();

    public FluidProductionRecipe(FluidProductionRecipe recipe) : base(recipe)
    {
        fluidCost = recipe.fluidCost;
        fluidYield = recipe.fluidYield;
    }

    public FluidProductionRecipe(int _id) : base(_id) { }
    public FluidProductionRecipe() { }

}

[Serializable]
public class ProductionRecipeCategory : DataCategory<ProductionRecipe>
{

}