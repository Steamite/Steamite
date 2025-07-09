using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IFluidWork
{
    List<Pipe> AttachedPipes { get; set; }
    Fluid StoredFluids { get; set; }

    /// <summary>
    /// Call in PlaceBuilding(), places pipes.
    /// </summary>
    /// <param name="t">Building reference</param>
    void PlacePipes()
    {
        foreach (BuildPipe buildPipe in AttachedPipes)
        {
            buildPipe.PlacePipe();
        }
    }

    /// <summary>
    /// Call in FinishBuild(), adds the build and pipes into the network.
    /// </summary>
    /// <param name="building"></param>
    void ConnectToNetwork()
    {
        foreach (BuildPipe buildPipe in AttachedPipes)
        {
            buildPipe.FinishBuild();
        }
    }

    /// <summary>
    /// Call on DestoyBuilding().
    /// </summary>
    /// <param name="t">pipes</param>
    void DisconnectFromNetwork()
    {
        List<int> networks = new();
        foreach (BuildPipe buildPipe in AttachedPipes)
        {
            for (int i = 0; i < 4; i++)
            {
                buildPipe.DisconnectPipe(i, true);
            }
        }
    }

    /// <summary>
    /// Finds buildings that can contain the needed fluids and then tries to take it from them.
    /// </summary>
    /// <param name="fluid">Fluid to take</param>
    bool TakeFromNetwork(Fluid fluid, bool immediatelyUse = true)
    {
        if (immediatelyUse && StoredFluids.Contains(fluid))
        {
            StoredFluids.Remove(fluid);
            return true;
        }

        IEnumerable<FluidNetwork> fluidNetworks =
            AttachedPipes.Select(q => q.network).Distinct();
        
        IEnumerable<IFluidWork> bestSources = fluidNetworks
            .SelectMany(q => q.storageBuildings
                .Where(q => q.StoredFluids.types.Union(fluid.types).Count() > 0));

        Fluid remainingTransfer = new(fluid);
        foreach (var storage in bestSources)
        {
            storage.StoredFluids.Remove(ref remainingTransfer);
            if (remainingTransfer.types.Count == 0)
            {
                if(immediatelyUse == false)
                    StoredFluids.Add(fluid);
                return true;
            }
        }

        for (int i = 0; i < fluid.types.Count; i++)
        {
            int j = remainingTransfer.types.IndexOf(fluid.types[i]);
            int x = j == -1 ? 0 : remainingTransfer.ammounts[j];
            
            StoredFluids.Add(
                fluid.types[i], 
                fluid.ammounts[i] - x);
        }
        return false;
    }

    public void StoreInNetwork(Fluid fluid)
    {
        if (StoredFluids.HasSpace(fluid))
        {
            StoredFluids.Add(fluid);
            ((IUpdatable)this).UIUpdate(nameof(StoredFluids));
            return;
        }

        IEnumerable<FluidNetwork> fluidNetworks =
            AttachedPipes.Select(q => q.network).Distinct();

        IFluidWork build = fluidNetworks.SelectMany(q => q.storageBuildings).FirstOrDefault(q => q.StoredFluids.HasSpace(fluid));
        if (build != null)
        {
            build.StoredFluids.Add(fluid);
            ((IUpdatable)build).UIUpdate(nameof(StoredFluids));
        }
    }

    void CreatePipes(bool loading = false)
    {
        AttachedPipes = new();
        Building building = (Building)this;
        GridPos buildPos = building.GetPos();
        for (int i = building.blueprint.itemList.Count - 1; i > -1; i--)
        {

            NeededGridItem item = building.blueprint.itemList[i];
            if (item.itemType == GridItemType.Pipe)
            {
                GridPos itemPos = MyGrid.Rotate(item.pos, building.transform.rotation.eulerAngles.y, true);
                itemPos.x += buildPos.x;
                itemPos.z = buildPos.z - itemPos.z;
                AttachedPipes.Add(
                    SceneRefs.objectFactory.CreateBuildingPipe(itemPos, this));

            }
        }
        if (loading)
        {
            PlacePipes();
            ConnectToNetwork();
        }
    }
}
