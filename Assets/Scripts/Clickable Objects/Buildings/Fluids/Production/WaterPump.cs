using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class WaterPump : FluidResProductionBuilding, IResourceProduction
{
    public Water waterSource;

    public override bool ManageInputRes()
    {
        bool res = true;
        if (waterSource.Ammount == 0)
        {
            ProdStates.running = false;
            res = false;
        }

        FluidProdStates fluidProd = ProdStates as FluidProdStates;
        if (!fluidProd.fluidSpace)
        {
            fluidProd.fluidSpace = ((IFluidWork)this).HasSpace(FluidYeild);
            if (fluidProd.fluidSpace == false)
                res = false;
        }

        if (res == true)
        {
            waterSource.Ammount -= 1;
            waterSource.UIUpdate(nameof(Water.Ammount));
            ProdStates.running = true;
        }
        return res;
    }
}