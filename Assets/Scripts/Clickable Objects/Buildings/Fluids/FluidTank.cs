using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class FluidTank : Building, IFluidWork
{
    public List<BuildPipe> AttachedPipes { get; set; }

    [SerializeField] Fluid storedFluid;
    [CreateProperty] public Fluid StoredFluids { get => storedFluid; set => storedFluid = value; }

    public override void FinishBuild()
    {
        StoredFluids = new(
            new List<FluidType> { FluidType.Water },
            new List<int> { 0 },
            new List<int> { localRes.capacity.currentValue });
        base.FinishBuild();
    }

    public override void DestoyBuilding()
    {
        AttachedPipes.ForEach(q=> q.DestoyBuilding());
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
        (clickable as TankBSave).fluidSave = storedFluid;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        StoredFluids = (save as TankBSave).fluidSave;
        base.Load(save);
    }
    #endregion
}
