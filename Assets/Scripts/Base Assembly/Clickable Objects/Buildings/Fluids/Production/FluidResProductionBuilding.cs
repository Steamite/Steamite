using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;

public class FluidResProductionBuilding : ResourceProductionBuilding, IInputFluidWork, IResourceProduction
{
    public List<BuildPipe> AttachedPipes { get; set; } = new();

    [CreateProperty] public ModifiableResource FluidCost { get => fluidCost; set => fluidCost = value; }
    [SerializeField] ModifiableResource fluidCost = new();
    [CreateProperty] public ModifiableResource FluidYeild { get => fluidYeild; set => fluidYeild = value; }
    [SerializeField] ModifiableResource fluidYeild = new();
    [CreateProperty] public CapacityResource StoredFluids { get => storedFluids; set => storedFluids = value; }

    [CreateProperty] public CapacityResource InputFluid { get; set; }


    [SerializeField] CapacityResource storedFluids;

    public override void FinishBuild()
    {
        ProdStates = new FluidProdStates();
        StoredFluids.InitCapacity();// = new(storedFluids.capacity.getBaseVal);
        InputFluid = new(fluidCost.ammounts.Sum()*2);
        base.FinishBuild();
    }

    public override void DestoyBuilding()
    {
        ((IFluidWork)this).OnDestroy();
        base.DestoyBuilding();
    }

    #region Window
    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("General", new List<string> { "Production Info", "Assign Info" });
        toEnable.Add("Fluids", new List<string> { "Fluid Info" });
        base.ToggleInfoComponents(info, toEnable);
    }
    #endregion

    #region Production
    public virtual bool ManageInputRes()
    {
        bool res = true;
        if (ProdStates.needsResources && !ProdStates.supplied)
        {
            ((IResourceProduction)this).RequestRestock();
            res = false;
        }
        if (!ProdStates.space)
        {
            ((IResourceProduction)this).RequestPickup();
            res = false;
        }

        FluidProdStates fluidProd = ProdStates as FluidProdStates;
        if (!fluidProd.fluidSupplied)
        {
            fluidProd.fluidSupplied = ((IInputFluidWork)this).TakeFromNetwork(FluidCost, false);
            if (fluidProd.fluidSupplied == false)
                res = false;
        }

        if (!fluidProd.fluidSpace)
        {
            fluidProd.fluidSpace = ((IFluidWork)this).HasSpace(FluidYeild);
            if (fluidProd.fluidSpace == false)
                res = false;
        }

        ProdStates.running = res;
        if (res)
        {
            InputResource.Manage(ResourceCost, false);
            UIUpdate(nameof(InputResource));
            InputFluid.Manage(fluidCost, false);
            UIUpdate(nameof(InputFluid));
        }
        return res;
    }
    void IProduction.Product()
    {
        // Resource Prod
        FluidProdStates states = ProdStates as FluidProdStates;
        states.fluidSpace = ((IFluidWork)this).StoreInNetwork(FluidYeild, out bool success);
        // only if the fluid was succesfully stored, finish the production
        if (success)
        {
            CurrentTime -= ProdTime;
            states.fluidSupplied = InputFluid.Diff(FluidCost).Sum() == 0;

            // Resource Prod
            LocalRes.Manage(ResourceYield, true);
            MyRes.UpdateResource(ResourceYield, true);
            UIUpdate(nameof(LocalRes));
            if (ProdStates.needsResources)
                ProdStates.supplied = InputResource.Diff(ResourceCost).Sum() == 0;
            ProdStates.space = ResourceYield.Sum() <= LocalResource.FreeSpace;

            ManageInputRes();
        }
    }
    #endregion

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new FluidResProductionSave();
        (clickable as FluidResProductionSave).inputFluid = new ResourceSave(InputFluid);
        (clickable as FluidResProductionSave).storedFluid = new ResourceSave(StoredFluids);
        return base.Save(clickable);
    }

    public override void Load(ClickableObjectSave save)
    {
        InputFluid = new(fluidCost.ammounts.Sum() * 2);
        InputFluid.Manage(new(((FluidResProductionSave)save).inputFluid), true);

        StoredFluids.InitCapacity();// = new(storedFluids.capacity.currentValue);
        StoredFluids.Manage(new(((FluidResProductionSave)save).storedFluid), true);
        base.Load(save);
    }
    #endregion
}