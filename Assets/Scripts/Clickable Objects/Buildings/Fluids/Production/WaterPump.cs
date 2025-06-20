using System.Collections.Generic;

public class WaterPump : Building, IFluidWork
{
    public List<Pipe> AttachedPipes { get; set; }

    public override void PlaceBuilding()
    {
        base.PlaceBuilding();
        ((IFluidWork)this).PlacePipes();
    }
    public override void FinishBuild()
    {
        base.FinishBuild();
        ((IFluidWork)this).ConnectToNetwork();
    }
}