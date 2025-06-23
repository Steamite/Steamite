using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;

public class WaterPump : Building, IFluidWork, IProduction, IAssign
{
    public Water waterSource;
    public List<Pipe> AttachedPipes { get; set; } = new();
    
    [SerializeField] float prodTime;
    [CreateProperty] public float ProdTime { get => prodTime; set => prodTime = value; }

    [SerializeField] ModifiableInteger assignLimit = new();
    [CreateProperty] public ModifiableInteger AssignLimit { get => assignLimit; set => assignLimit = value; }

    [CreateProperty] public Fluid StoredFluids { get; set; } = new();
    [CreateProperty] public ModifiableFloat ProdSpeed { get; set; } = new();
    [CreateProperty] public List<Human> Assigned { get; set; } = new();
    [CreateProperty] public float CurrentTime { get; set; }
    [CreateProperty] public bool Stoped { get; set; }


    public override void InitModifiers()
    {
        base.InitModifiers();
        ProdSpeed = new(1);
        ((IModifiable)AssignLimit).Init();
    }
    public override void PlaceBuilding()
    {
        base.PlaceBuilding();
        ((IFluidWork)this).PlacePipes();
    }

    public override bool CanPlace()
    {
        bool canPlace = base.CanPlace();
        AttachedPipes.ForEach(q => q.FindConnections(canPlace));
        return canPlace;
    }

    public override void FinishBuild()
    {
        StoredFluids = new(
            new List<FluidType> { FluidType.Water },
            new List<int> { 0 },
            new List<int> { localRes.capacity.currentValue });
        base.FinishBuild();
        ((IFluidWork)this).ConnectToNetwork();
        InitModifiers();
    }

    public void ProgressProduction(float progress)
    {
        if (waterSource.hasResources == false 
            || !AttachedPipes[0].network.HasSpace(FluidType.Water, 2))
            return;

        CurrentTime += +ProdSpeed;
        ((IUpdatable)this).UIUpdate(nameof(CurrentTime));
        if (CurrentTime >= ProdTime)
            Product();
    }

    public void Product()
    {
        if (StoredFluids.HasSpace(FluidType.Water, 2))
        {
            StoredFluids.AddFluid(FluidType.Water, 2);
            UIUpdate(nameof(StoredFluids));
        }
        else
        {
            AttachedPipes[0].network.StoreFluid(FluidType.Water, 2);
        }
        
        CurrentTime -= +ProdTime;
        waterSource.Ammount--;
    }

    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("Production", new List<string> { "Production Info", "Assign Info" });
        toEnable.Add("Fluids", new List<string> { "Fluid Info" });
        base.ToggleInfoComponents(info, toEnable);
    }


    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {

        if (clickable == null)
            clickable = new WaterPumpSave();
        (clickable as WaterPumpSave).fluidSave = StoredFluids;
        return base.Save(clickable);

    }

    public override void Load(ClickableObjectSave save)
    {
        StoredFluids = (save as WaterPumpSave).fluidSave; 
        ((IFluidWork)this).CreatePipes(true);
        base.Load(save);
    }
}