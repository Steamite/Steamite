using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class FluidResProductionBuilding : ResourceProductionBuilding, IFluidWork
{
    public List<Pipe> AttachedPipes { get; set; } = new();

    [CreateProperty] public Fluid FluidCost { get => fluidCost; set => fluidCost = value; }
    [SerializeField] Fluid fluidCost = new();
    [CreateProperty] public Fluid FluidYeild { get => fluidYeild; set => fluidYeild = value; }
    [SerializeField] Fluid fluidYeild = new();

    [CreateProperty] public Fluid StoredFluids { get => storedFluids; set => storedFluids = value; }
    [SerializeField] Fluid storedFluids = new();

    public override void FinishBuild()
    {
        StoredFluids = new(
            new List<FluidType> { FluidType.Water, FluidType.Steam },
            new List<int> { 0, 0 },
            new List<int> { localRes.capacity.currentValue, localRes.capacity.currentValue });
        base.FinishBuild();
    }

    #region Window
    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("General", new List<string> { "Production Info", "Assign Info" });
        toEnable.Add("Fluids", new List<string> { "Fluid Info" });
        base.ToggleInfoComponents(info, toEnable);
    }
    #endregion

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new FluidResProductionSave();
        (clickable as FluidResProductionSave).fluidSave = StoredFluids;
        (clickable as ResProductionBSave).inputRes = new(InputResource);
        (clickable as ProductionBSave).currentTime = CurrentTime;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        InputResource.Load((save as ResProductionBSave).inputRes);
        StoredFluids = (save as FluidResProductionSave).fluidSave;
        CurrentTime = (save as ProductionBSave).currentTime;
        ProdStates = (save as ProductionBSave).ProdStates;

        if (!ProdStates.supplied)
        {
            SceneRefs.jobQueue.AddJob(JobState.Supply, this);
        }
        if (constructed && GetDiff(new()).Sum() > 0)
        {
            SceneRefs.jobQueue.AddJob(JobState.Pickup, this);
        }
        base.Load(save);
    }
    #endregion
}