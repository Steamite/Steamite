using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IInputFluidWork : IFluidWork
{
    public CapacityResource InputFluid { get; set; }

    new void Empty()
    {
        StoreInNetwork(InputFluid, out _, false);
        StoreInNetwork(StoredFluids, out _, false);
    }
    /// <summary>
    /// Finds buildings that can contain the needed fluids and then tries to take it from them.
    /// </summary>
    /// <param name="fluidCost">Fluid to take</param>
    bool TakeFromNetwork(Resource fluidCost, bool immediatelyUse = true)
    {
        if (InputFluid.Contains(fluidCost))
        {
            if (immediatelyUse)
                InputFluid.Manage(fluidCost, false);
            return true;
        }
        Resource remaining = InputFluid.Diff(fluidCost);
        Resource toTransfer = new(remaining);

        IEnumerable<FluidNetwork> fluidNetworks =
            AttachedPipes.Select(q => q.network).Distinct();

        IEnumerable<IFluidWork> bestSources = fluidNetworks
            .SelectMany(q => q.storageBuildings
                .Where(q => q.StoredFluids.types.Intersect(toTransfer.types).Count() > 0))
                .Where(q => q != this);

        foreach (var storage in bestSources)
        {
            storage.StoredFluids.TakeResource(toTransfer);
            ((IUpdatable)storage).UIUpdate(nameof(StoredFluids));
            if (toTransfer.types.Count == 0)
            {
                if (immediatelyUse == false)
                {
                    InputFluid.Manage(remaining, true);
                    ((IUpdatable)this).UIUpdate(nameof(InputFluid));
                }
                return true;
            }
        }


        for (int i = 0; i < remaining.types.Count; i++)
        {
            int j = toTransfer.types.IndexOf(remaining.types[i]);
            int x = j == -1 ? 0 : toTransfer.ammounts[j];

            InputFluid.ManageSimple(
                remaining.types[i],
                remaining.ammounts[i] - x,
                true);
        }
        ((IUpdatable)this).UIUpdate(nameof(InputFluid));
        return false;
    }
}
