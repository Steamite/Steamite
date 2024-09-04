using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[Serializable]
public class NetworkAccesBuilding
{
    public Fluid fluid = new();
    /// <summary>
    /// Call in UniqueID(), ids all pipes.
    /// </summary>
    /// <param name="t">Building transform</param>
    public void ID(Transform t)
    {
        foreach (Pipe pipe in t.GetComponentsInChildren<Pipe>())
        {
            pipe.UniqueID();
        }
    }
    /// <summary>
    /// Call in CanPlace(), connect or disconnects pipes, and check if the place to build the pipes is free.
    /// </summary>
    /// <param name="t">Building transform</param>
    public bool ConnectPipes(Transform t)
    {
        bool canPlace = true;
        foreach (Pipe pipe in t.GetComponentsInChildren<Pipe>())
        {
            pipe.transform.rotation = Quaternion.Euler(0, 0, 0);
            if (!pipe.CanPlace())
            {
                canPlace = false;
            }
        }
        return canPlace;
    }

    /// <summary>
    /// Call in PlaceBuilding(), replaces or preps pipes.
    /// </summary>
    /// <param name="t">Building reference</param>
    public void PlacePipes(Transform t)
    {
        foreach (BuildPipe buildPipe in t.GetComponentsInChildren<BuildPipe>())
        {
            buildPipe.PlacePipe();
        }
        /*
        foreach (BuildPipe p in building.transform.GetChild(3).GetComponentsInChildren<BuildPipe>())
        {
            GridPos grid = new(p.gameObject);
            Pipe prevPipe = MyGrid.pipeGrid[(int)grid.x, (int)grid.z];
            if (prevPipe && prevPipe.id != p.id)
            {
                BuildPipe newPipe = prevPipe.gameObject.AddComponent<BuildPipe>();
                newPipe.FillData(prevPipe, building);
                prevPipe.transform.parent = building.transform.GetChild(3);
                if (prevPipe.associatedNetwork != -1)
                {
                    FluidNetwork fluidNet = MyGrid.fluidNetworks.First(q => q.networkID == prevPipe.associatedNetwork);
                    fluidNet.pipes[fluidNet.pipes.FindIndex(q => q.id == prevPipe.id)] = newPipe;
                    fluidNet.buildings.Add(building);
                }
                Object.Destroy(prevPipe);
                Object.Destroy(p.gameObject);
            }
            MyGrid.pipeGrid[(int)grid.x, (int)grid.z] = p;
        }*/
    }
    /// <summary>
    /// Call in FinishBuild(), adds the build and pipes into the network.
    /// </summary>
    /// <param name="building"></param>
    public void ConnectToNetwork(Transform t)
    {
        foreach (BuildPipe buildPipe in t.GetComponentsInChildren<BuildPipe>())
        {
            buildPipe.FinishBuild();
        }
    }
    /// <summary>
    /// Call on DestoyBuilding().
    /// </summary>
    /// <param name="t">pipes</param>
    public void DisconnectFromNetwork(Transform t)
    {
        List<int> networks = new();
        foreach (BuildPipe p in t.GetComponentsInChildren<BuildPipe>())
        {
            if (networks.IndexOf(p.network.networkID) == -1)
            {
                networks.Add(p.network.networkID);
                p.network.buildings.Remove(p.connectedBuilding);
                p.DestoyBuilding();
            }
        }
    }
    /// <summary>
    /// Call when there's no space to store the resource
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="t"></param>
    /// <param name="deposit">If deposit find place to store, else find place to take from </param>
    /// <returns></returns>
    public Building FindStore(FluidType fluidType, Transform t, bool deposit)
    {
        List<int> networks = new();
        foreach(FluidNetwork network in t.GetComponentsInChildren<Pipe>().Select(q=>q.network))
        {
            if (networks.IndexOf(network.networkID) == -1)
            {
                // add prio to production buildings
                networks.Add(network.networkID);
                foreach(Building building in network.buildings)
                {
                    int i = building.GetFluid().type.IndexOf(fluidType);
                    if (deposit)
                    {
                        if (i > -1 && building.GetFluid().ammount[i] < building.GetFluid().capacity[i])
                        {
                            return building;
                        }
                    }
                    else
                    {
                        if (i > -1 && building.GetFluid().ammount[i] > 0)
                        {
                            return building;
                        }
                    }
                }
            }       
        }
        return null;
    }

    public FluidWorkSave SaveFluidData(Transform pipeHolder)
    {
        FluidWorkSave workSave = new();
        workSave.fluid = fluid;
        workSave.pipeSaves = new();
        foreach (BuildPipe buildPipe in pipeHolder.GetComponentsInChildren<BuildPipe>())
        {
            workSave.pipeSaves.Add(buildPipe.Save());
        }
        return workSave;
    }

    internal void Load(Transform transform, List<ClickableObjectSave> pipeSaves)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<BuildPipe>().Load(pipeSaves[i]); 
        }
    }
}
