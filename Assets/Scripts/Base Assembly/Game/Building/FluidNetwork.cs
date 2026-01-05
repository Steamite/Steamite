using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Groups all connected pipes and buildings to enable moving fluids between them.</summary>
[Serializable]
public class FluidNetwork
{
    #region Variables
    /// <summary>All connected pipes in the network.</summary>
    public List<Pipe> pipes = new();

    /// <summary>All connected buildings in the network.</summary>
    public List<IFluidWork> buildings;

    public List<FluidResProductionBuilding> consumptionBuildings;

    public List<IFluidWork> storageBuildings;
    /// <summary>Network ID for identification and display.</summary>
    public int networkID = -1;
    /// <summary>Network name for display.</summary>
    public string networkName;
    #endregion

    #region Constructors
    /// <summary>Constructor, used when creating pipes or spliting networks.</summary>
    public FluidNetwork()
    {
        CreateID();
    }
    public override string ToString()
    {
        return $"Network: {networkID}, with {buildings.Count} connected buildings";
    }

    /// <summary>
    /// Constructor, used to add a new pipe to Global network list.
    /// </summary>
    /// <param name="_pipe">Pipe to add.</param>
    public FluidNetwork(Pipe _pipe)
    {
        pipes.Add(_pipe);
        CreateID();
    }

    /// <summary>Creates a unique id for the network.</summary>
    void CreateID()
    {
        if (MyGrid.fluidNetworks.Count > 0)
            networkID = MyGrid.fluidNetworks.Last().networkID + 1;
        else
            networkID = 0;
        storageBuildings = new();
        consumptionBuildings = new();
        buildings = new();
    }
    #endregion

    #region Modifying
    /// <summary>
    /// Takes other network and merges it with this one.
    /// </summary>
    /// <param name="_mergeWith">network to merge with</param>
    public void Merge(FluidNetwork _mergeWith)
    {
        if (_mergeWith == this)
            Debug.LogError("Merging the network into itself");
        for (int i = _mergeWith.pipes.Count - 1; i > -1; i--)
        {
            Pipe pipe = _mergeWith.pipes[i];
            pipes.Add(pipe);
            pipe.network = this;
            pipe.UIUpdate(nameof(pipe.network));
        }

        buildings = buildings.Union(_mergeWith.buildings).ToList();
        storageBuildings = storageBuildings.Union(_mergeWith.storageBuildings).ToList();
        consumptionBuildings = consumptionBuildings.Union(_mergeWith.consumptionBuildings).ToList();

        MyGrid.fluidNetworks.Remove(_mergeWith);
    }

    /// <summary>
    /// Splits the network at the place of the splitter.
    /// </summary>
    /// <param name="spliter">The destroyed pipe, which is missing now.</param>
    public void Split(Pipe spliter)
    {
        IEnumerable<Pipe> pipes = spliter.connectedPipes.Where(q => q != null).Select(q => q.connectedPipe);

        int count = pipes.Count();
        if (count != 1)
        {
            foreach (var pipe in pipes)
            {
                if (pipe.network.networkID == networkID)
                {
                    FluidNetwork network = new();
                    pipe.ConnectToNetwork(network);
                    MyGrid.fluidNetworks.Add(network);
                }
            }

            MyGrid.fluidNetworks.Remove(this);
        }

    }

    public bool HasSpace(Resource fluid)
    {
        for (int i = 0; i < consumptionBuildings.Count; i++)
        {
            if (consumptionBuildings[i].InputFluid.HasSpace(fluid, true))
                return true;
        }
        for (int i = 0; i < storageBuildings.Count; i++)
        {
            if (storageBuildings[i].StoredFluids.HasSpace(fluid, true))
                return true;
        }
        return false;
    }
    #endregion
}