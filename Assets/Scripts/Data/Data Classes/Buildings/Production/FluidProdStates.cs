using System;

[Serializable]
public class FluidProdStates : ProductionStates
{
    public bool fluidSupplied = false;
    public FluidProdStates(bool _supplied, bool _space, bool _running, bool _fluidSupplied) : base(_supplied, _space, _running)
    {
        fluidSupplied = _fluidSupplied;
    }

    public FluidProdStates()
    {

    }
}