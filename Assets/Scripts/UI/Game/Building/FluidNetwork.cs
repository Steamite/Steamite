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
    public List<Building> buildings = new();
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
    }
    #endregion

    #region Modifying
    /// <summary>
    /// Takes other network and merges it with this one.
    /// </summary>
    /// <param name="_mergeWith">network to merge with</param>
    public void Merge(FluidNetwork _mergeWith)
    {
        foreach (Pipe pipe in _mergeWith.pipes)
        {
            pipes.Add(pipe);
            pipe.network = this;
        }
        foreach (Building building in _mergeWith.buildings)
        {
            buildings.Add(building);
            // TODO: call to building
        }
        MyGrid.fluidNetworks.Remove(_mergeWith);
    }

    /// <summary>
    /// Splits the network at the place of the splitter.
    /// </summary>
    /// <param name="spliter">The destroyed pipe, which is missing now.</param>
    public void Split(Pipe spliter)
    {
        if (spliter.transform.childCount == 0)
        {
            MyGrid.fluidNetworks.Remove(spliter.network);
            return;
        }
        else if (spliter.transform.childCount > 1)
        {
            pipes.Remove(spliter);
            DoSplit(0, 1, spliter.transform);
        }
    }

    /// <summary>
    /// Recursion that checks all connections, to find if they're connected somewhere else or not.
    /// </summary>
    /// <param name="childA">The pipe I'm looking at.</param>
    /// <param name="childB">The pipe I want to compare with.</param>
    /// <param name="pipeTransform">Transform of the splitting pipe.</param>
    void DoSplit(int childA, int childB, Transform pipeTransform)
    {
        if (childA == pipeTransform.childCount || childB == pipeTransform.childCount)
            return;
        Pipe pipeA = pipeTransform.transform.GetChild(childA).GetComponent<PipePart>().connectedPipe;
        Pipe pipeB = pipeTransform.transform.GetChild(childB).GetComponent<PipePart>().connectedPipe;
        if (PathFinder.FindPath(pipeA.GetPos(), pipeB.GetPos(), typeof(Pipe)).Count == 0)
        {
            if (childA == 0)
            {
                FluidNetwork fluidNetwork = new();
                MyGrid.fluidNetworks.Add(fluidNetwork);
                fluidNetwork.ChangeNetwork(pipeB);
                DoSplit(childB, childB + 1, pipeTransform);
            }
            else
            {
                DoSplit(0, childB, pipeTransform);
            }
        }
        else
        {
            DoSplit(childA, childB + 1, pipeTransform);
        }
    }

    /// <summary>
    /// Handles transfer of pipes across networks.
    /// </summary>
    /// <param name="pipe"></param>
    void ChangeNetwork(Pipe pipe)
    {
        if (pipe.network.networkID == -1)
            return;
        pipe.network.pipes.Remove(pipe);
        pipes.Add(pipe);
        if (pipe.GetComponent<BuildPipe>())
            buildings.Add(pipe.GetComponent<BuildPipe>().connectedBuilding);
        pipe.network = this;

        foreach (Pipe connected in pipe.GetComponentsInChildren<PipePart>().Select(q => q.connectedPipe).Where(q => q != null))
        {
            if (!connected)
                continue;
            if (connected.network.networkID != networkID)
            {
                ChangeNetwork(connected);
            }
        }
    }
    #endregion
}