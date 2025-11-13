using Unity.Properties;

public class NeedSourceProduction : FluidResProductionBuilding, IResourceProduction
{
    [CreateProperty]
    public TileSource Source
    {
        get => source;
        set
        {
            source = value;
            UpdateYields(false);
        }
    }
    TileSource source;
    public int ammountPerTick = 3;

    public override void FinishBuild()
    {
        ResourceYield = new();
        FluidYeild = new();
        UpdateYields(false);
        base.FinishBuild();
    }
    public override bool ManageInputRes()
    {
        bool res = true;
        if (!source.HasResources)
        {
            ProdStates.running = false;
            res = false;
        }
        else
            res = base.ManageInputRes();

        if (res == true)
        {
            UpdateYields(true);
            ProdStates.running = true;
        }
        return res;
    }

    void UpdateYields(bool takeFromSource)
    {
        if (source is Vein)
        {
            ResourceYield = new(source.RemoveFromSource(ammountPerTick, takeFromSource) as Resource);
            UIUpdate(nameof(ResourceYield));
        }
        else
        {
            FluidYeild = new(source.RemoveFromSource(ammountPerTick, takeFromSource) as Resource);
            //FluidCost = FluidYeild;
            UIUpdate(nameof(FluidYeild));
            UIUpdate(nameof(Source) + "." + nameof(TileSource.Storing));
        }
    }
}