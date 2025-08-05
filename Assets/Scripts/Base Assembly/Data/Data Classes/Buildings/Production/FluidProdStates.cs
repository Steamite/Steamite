using System;

[Serializable]
public class FluidProdStates : ProductionStates
{
    public bool fluidSupplied = false;
    public bool fluidSpace = false;

    public FluidProdStates()
    {
    }

    public FluidProdStates(bool fluidSupplied, bool fluidSpace, bool requestedSupply, bool supplied, bool needsResources, bool requestedPickup, bool space, bool running) : base(requestedSupply, supplied, needsResources, requestedPickup, space, running)
    {
        this.fluidSupplied = fluidSupplied;
        this.fluidSpace = fluidSpace;
    }
}