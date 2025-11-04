using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class FluidTank : Building, IFluidWork
{
    public List<BuildPipe> AttachedPipes { get; set; }

    [SerializeField] Fluid storedFluid;
    [CreateProperty] public Fluid StoredFluids { get => storedFluid; set => storedFluid = value; }
    public ulong TypesToStore;
    public override void FinishBuild()
    {
        storedFluid = new();

        ulong byt = TypesToStore;
        int i = 0;
        while (byt != 0)
        {
            if ((byt & 1) == 1)
            {
                storedFluid.types.Add(ResFluidTypes.GetFluidByIndex(i));
                storedFluid.ammounts.Add(0);
                storedFluid.capacities.Add(localRes.capacity.currentValue);
            }
            byt = byt >> 1;
            i++;
        }
        base.FinishBuild();
    }

    public override void DestoyBuilding()
    {
        AttachedPipes.ForEach(q => q.DestoyBuilding());
        base.DestoyBuilding();
    }

    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("Fluids", new List<string> { "Fluid Info" });
        base.ToggleInfoComponents(info, toEnable);
    }

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new TankBSave();
        (clickable as TankBSave).fluidSave = new(storedFluid);
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        StoredFluids = new((save as TankBSave).fluidSave);
        base.Load(save);
    }
    #endregion
}
