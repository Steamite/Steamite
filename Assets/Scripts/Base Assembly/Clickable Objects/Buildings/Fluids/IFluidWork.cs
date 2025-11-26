using System.Collections.Generic;
using System.Linq;
public interface IFluidWork
{
    List<BuildPipe> AttachedPipes { get; set; }
    CapacityResource StoredFluids { get; set; }

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
        StoreInNetwork(StoredFluids, out _,false);
    }
    
    public static IFluidWork GetBuildingWith(IEnumerable<IFluidWork> buildings, Resource toStore)
    {
        return buildings.FirstOrDefault(q => (q is IInputFluidWork input) ? input.InputFluid.HasSpace(toStore) : q.StoredFluids.HasSpace(toStore));

    }

    public bool StoreInNetwork(Resource fluid, out bool succes, bool storeInSelf = true)
    {
        succes = true;
        if (storeInSelf && StoredFluids.HasSpace(fluid))
        {
            StoredFluids.Manage(fluid, true);
            ((IUpdatable)this).UIUpdate(nameof(StoredFluids));
            if (StoredFluids.HasSpace(fluid))
                return true;
            return false;
        }

        IEnumerable<IFluidWork> buildings = AttachedPipes.Select(q => q.network).Distinct().SelectMany(q => q.storageBuildings);
        IFluidWork build = GetBuildingWith(buildings, fluid);
        if (build != null)
        {
            if(build is IInputFluidWork input)
            {
                input.InputFluid.Manage(fluid, true);
                ((IUpdatable)input).UIUpdate(nameof(IInputFluidWork.InputFluid));

                if (input.InputFluid.HasSpace(fluid))
                    return true;
            }
            else
            {
                build.StoredFluids.Manage(fluid, true);
                ((IUpdatable)build).UIUpdate(nameof(StoredFluids));
                if (build.StoredFluids.HasSpace(fluid))
                    return true;
            }

            if(GetBuildingWith(buildings, fluid) != null)
                return true;
            return false;
        }
        /*
                StoredFluids.Manage(fluid, true);
                ((IUpdatable)this).UIUpdate(nameof(StoredFluids));*/
        succes = false;
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

    public bool HasSpace(Resource fluid)
    {
        return AttachedPipes.Select(q => q.network).Distinct().FirstOrDefault(q => q.HasSpace(fluid)) != null;
    }
}
