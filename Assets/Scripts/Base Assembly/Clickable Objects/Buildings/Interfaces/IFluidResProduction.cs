using System.Collections.Generic;
using Unity.Properties;
/*
public interface IFluidResProduction : IResourceProduction
{
    /// <summary>Cost of one production cycle.</summary>
    [CreateProperty] Fluid FluidCost { get; set; }

    /// <summary>Production cycle yeild.</summary>
    [CreateProperty] Fluid FluidYeild { get; set; }

    bool IResourceProduction.ManageInputRes()
    {
        IFluidWork fluidWork = ((IFluidWork)this);
        if (!ProdStates.supplied)
        {
            ProdStates.running = false;
            RequestRestock();
            return false;
        }
        else if (!((FluidProdStates)ProdStates).fluidSupplied)
        {
            ProdStates.running = false;
            ((FluidProdStates)ProdStates).fluidSupplied = 
                fluidWork.TakeFromNetwork(
                    fluidWork.StoredFluids.Diff(FluidCost));
            return false;
        }
        else
        {
            InputResource.Manage(ResourceCost, false);
            ((ClickableObject)this).UIUpdate("InputResource");
            ProdStates.running = true;
            return true;
        }
    }

    void IProduction.Product()
    {
        while (CurrentTime >= ProdTime &&
            (LocalResource.capacity.currentValue == -1 || LocalResource.Sum() < LocalResource.capacity.currentValue))
        {
            CurrentTime -= ProdTime;
            LocalResource.Manage(ResourceYield, true);
            MyRes.UpdateResource(ResourceYield, true);
            ProdStates.supplied = InputResource.Diff(ResourceCost).Sum() == 0;

            IFluidWork fluidWork = (IFluidWork)this;
            fluidWork.StoreInNetwork(new(new List<FluidType> { FluidType.Water }, new List<int> { 2 }, null));
            ((FluidProdStates)ProdStates).fluidSupplied =
                fluidWork.TakeFromNetwork(
                    fluidWork.StoredFluids.Diff(FluidCost));

            if (!ManageInputRes())
                return;
        }
    }
}*/