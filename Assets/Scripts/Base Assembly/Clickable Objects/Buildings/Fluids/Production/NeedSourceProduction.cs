using System.Linq;
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
        UpdateYields(false);
        StoredFluids.types = Source.Storing.types.ToList();
        StoredFluids.ammounts = new();
        for (int i = 0; i < StoredFluids.types.Count; i++)
            StoredFluids.ammounts.Add(0);
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
        {
            UpdateYields(false);
            res = base.ManageInputRes();
        }

        if (res == true)
        {
            UpdateYields(true);
            ProdStates.running = true;
        }
        return res;
    }

    void UpdateYields(bool takeFromSource)
    {
        // Current system doesn't support ResourceCost for VEIN
        // and FluidCost for WATER
        // but it can be changed later.
        if (source is Vein)
        {
            ResourceYield = new(source.RemoveFromSource(ammountPerTick, takeFromSource) as Resource);
            UIUpdate(nameof(ResourceYield));
        }
        else
        {
            FluidYeild = new(source.RemoveFromSource(ammountPerTick, takeFromSource) as Resource);
            UIUpdate(nameof(FluidYeild));
            UIUpdate(nameof(Source) + "." + nameof(TileSource.Storing));
        }
    }

    
}