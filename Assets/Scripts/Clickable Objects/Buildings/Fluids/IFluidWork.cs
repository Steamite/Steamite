using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

public interface IFluidWork
{
    List<BuildPipe> AttachedPipes { get; set; }
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

    public void OnDestroy()
    {
        Empty();
        AttachedPipes.ForEach(q => q.DestoyBuilding());
    }
    void Empty()
    {
        StoreInNetwork(StoredFluids, false);
    }
    /// <summary>
    /// Finds buildings that can contain the needed fluids and then tries to take it from them.
    /// </summary>
    /// <param name="fluid">Fluid to take</param>
    bool TakeFromNetwork(Fluid fluid, bool immediatelyUse = true)
    {
        if (StoredFluids.Contains(fluid))
        {
            if(immediatelyUse)
                StoredFluids.Manage(fluid, false);
            return true;
        }

        IEnumerable<FluidNetwork> fluidNetworks =
            AttachedPipes.Select(q => q.network).Distinct();

        IEnumerable<IFluidWork> bestSources = fluidNetworks
            .SelectMany(q => q.storageBuildings
                .Where(q => q.StoredFluids.types.Union(fluid.types).Count() > 0))
                .Where(q => q != this);

        Fluid toTransfer = new(fluid);
        foreach (var storage in bestSources)
        {
            storage.StoredFluids.RemoveAndCheck(toTransfer);
            ((IUpdatable)storage).UIUpdate(nameof(StoredFluids));
            if (toTransfer.types.Count == 0)
            {
                if(immediatelyUse == false)
                {
                    StoredFluids.Manage(fluid, true);
                    ((IUpdatable)this).UIUpdate(nameof(StoredFluids));
                }
                return true;
            }
        }

        
        for (int i = 0; i < fluid.types.Count; i++)
        {
            int j = toTransfer.types.IndexOf(fluid.types[i]);
            int x = j == -1 ? 0 : toTransfer.ammounts[j];

            StoredFluids.ManageSimple(
                fluid.types[i],
                fluid.ammounts[i] - x,
                true);
        }
        ((IUpdatable)this).UIUpdate(nameof(StoredFluids));
        return false;
    }

    public bool StoreInNetwork(Fluid fluid, bool storeInSelf = true)
    {
        if (storeInSelf && StoredFluids.HasSpace(fluid))
        {
            StoredFluids.Manage(fluid, true);
            ((IUpdatable)this).UIUpdate(nameof(StoredFluids));
            if(StoredFluids.HasSpace(fluid))
                return true;
        }

        IEnumerable<IFluidWork> buildings = AttachedPipes.Select(q => q.network).Distinct().SelectMany(q => q.storageBuildings);
        IFluidWork build = buildings.FirstOrDefault(q => q.StoredFluids.HasSpace(fluid));
        if (build != null)
        {
            build.StoredFluids.Manage(fluid, true);
            ((IUpdatable)build).UIUpdate(nameof(StoredFluids));
            if (build.StoredFluids.HasSpace(fluid) 
                || buildings.FirstOrDefault(q => q.StoredFluids.HasSpace(fluid)) != null)
                return true;
        }

        StoredFluids.Manage(fluid, true);
        ((IUpdatable)this).UIUpdate(nameof(StoredFluids));
        return false;
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
                    SceneRefs.ObjectFactory.CreateBuildingPipe(itemPos, this));

            }
        }
        if (loading)
        {
            PlacePipes();
            ConnectToNetwork();
        }
    }

    public bool HasSpace(Fluid fluid)
    {
        return AttachedPipes.Select(q => q.network).Distinct().FirstOrDefault(q=> q.HasSpace(fluid)) != null;
    }
}
