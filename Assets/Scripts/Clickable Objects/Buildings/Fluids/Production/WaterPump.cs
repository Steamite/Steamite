using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class WaterPump : FluidResProductionBuilding, IResourceProduction
{
    public Water waterSource;

    public override void FinishBuild()
    {
        StoredFluids = new(
            new List<FluidType> { FluidType.Water },
            new List<int> { 0 },
            new List<int> { localRes.capacity.currentValue });
        base.FinishBuild();
    }

    bool IResourceProduction.ManageInputRes()
    {
        if (waterSource.Ammount == 0)
        {
            ProdStates.running = false;
            return false;
        }
        else
        {
            waterSource.Ammount -= 1;
            waterSource.UIUpdate(nameof(Water.Ammount));
            ProdStates.running = true;
            return true;
        }
    }

    public override void DestoyBuilding()
    {
        AttachedPipes.ForEach(q => q.DestoyBuilding());
        base.DestoyBuilding();
    }
}