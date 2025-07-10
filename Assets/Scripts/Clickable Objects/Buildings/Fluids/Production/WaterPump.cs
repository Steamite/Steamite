using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class WaterPump : Building, IFluidWork, IProduction, IAssign
{
    public Water waterSource;
    public List<BuildPipe> AttachedPipes { get; set; } = new();

    [SerializeField] float prodTime;
    [CreateProperty] public float ProdTime { get => prodTime; set => prodTime = value; }

    [SerializeField] ModifiableInteger assignLimit = new();
    [CreateProperty] public ModifiableInteger AssignLimit { get => assignLimit; set => assignLimit = value; }

    [CreateProperty] public Fluid StoredFluids { get; set; } = new();
    [CreateProperty] public ModifiableFloat ProdSpeed { get; set; } = new();
    [CreateProperty] public List<Human> Assigned { get; set; } = new();
    [CreateProperty] public float CurrentTime { get; set; }
    [CreateProperty] public bool Stoped { get; set; }



    public override void FinishBuild()
    {
        StoredFluids = new(
            new List<FluidType> { FluidType.Water },
            new List<int> { 0 },
            new List<int> { localRes.capacity.currentValue });
        base.FinishBuild();
    }

    public void ProgressProduction(float progress)
    {
        if (Stoped 
            || waterSource.hasResources == false
            || !AttachedPipes[0].network.HasSpace(FluidType.Water, 2))
            return;

        CurrentTime += +ProdSpeed;
        UIUpdate(nameof(CurrentTime));
        if (CurrentTime >= ProdTime)
            Product();
    }

    public void Product()
    {
        ((IFluidWork)this).StoreInNetwork(new(new List<FluidType> { FluidType.Water }, new List<int> { 2 }, null));

        CurrentTime -= +ProdTime;
        waterSource.Ammount--;
    }

    
    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("Production", new List<string> { "Production Info", "Assign Info" });
        toEnable.Add("Fluids", new List<string> { "Fluid Info" });
        base.ToggleInfoComponents(info, toEnable);
    }


    public override void DestoyBuilding()
    {
        AttachedPipes.ForEach(q => q.DestoyBuilding());
        base.DestoyBuilding();
    }


    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new FluidProdBSave();
        (clickable as FluidProdBSave).fluidSave = StoredFluids;
        return base.Save(clickable);
    }

    public override void Load(ClickableObjectSave save)
    {
        StoredFluids = (save as FluidProdBSave).fluidSave;
        base.Load(save);
    }
    #endregion
}