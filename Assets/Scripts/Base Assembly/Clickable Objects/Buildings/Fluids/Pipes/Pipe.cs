using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Pipe : Building
{
    public FluidNetwork network = null;
    public PipePart[] connectedPipes = new PipePart[4];
    readonly int[] connectionOrder = { 1, 0, 3, 2 };

    public override bool Equals(object obj)
    {
        bool b = base.Equals(obj);

        if (b && id == -1)
        {
            return (obj as Pipe).GetPos().Equals(GetPos());
        }
        return false;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), objectName, id);
    }

    public override void FinishBuild()
    {
        List<FluidNetwork> connectedNetworks = connectedPipes
            .Where(q => q != null && q.connectedPipe.network.networkID != -1)
            .Select(q => q.connectedPipe.network).ToList();

        if (connectedNetworks.Count > 0)
        {
            connectedNetworks = connectedNetworks.OrderBy(q => q.networkID).ToList();
            network = connectedNetworks[0];
            network.pipes.Add(this);
            int z = network.networkID;
            for (int i = 1; i < connectedNetworks.Count; i++)
            {
                if (z != connectedNetworks[i].networkID)
                {
                    z = connectedNetworks[i].networkID;
                    network.Merge(connectedNetworks[i]);
                }
            }
        }
        else
        {
            if (network.networkID == -1)
                network = new(this);
            network.pipes.Add(this);
            MyGrid.fluidNetworks.Add(network);
        }
        base.FinishBuild();
        ConnectToNetwork(network);
    }
    public virtual void ConnectToNetwork(FluidNetwork _network)
    {
        if (_network == network)
        {
            return;
        }
        else
        {
            network.pipes.Remove(this);

            network = _network;
            network.pipes.Add(this);

            foreach (Pipe connected in connectedPipes.Where(q => q != null).Select(q => q.connectedPipe))
            {
                connected.ConnectToNetwork(network);
            }
        }
    }

    public override void PlaceBuilding()
    {
        UniqueID();
        gameObject.layer = 7;
        for (int i = 0; i < 4; i++)
        {
            PipePart part = connectedPipes[i];
            if (part != null)
            {
                part.gameObject.layer = 7;
                part.connectedPipe.connectedPipes[connectionOrder[i]].gameObject.layer = 7;
            }
        }
        GetComponent<SortingGroup>().sortingLayerName = "Pipes";
        Highlight(new());

        SceneRefs.JobQueue.AddJob(JobState.Constructing, this); // creates a new job with the data above
        MyRes.PayCostGlobal(cost);
    }

    public void ConnectPipe(int _case, Pipe connectedPipe, bool canNext)
    {
        float pipeLenghtScale = (1 - transform.lossyScale.z) / 4 / transform.lossyScale.z;
        Transform pipePart;
        if ((pipePart = transform.GetComponentsInChildren<Transform>().FirstOrDefault(q => q.name == _case.ToString())) == null || !canNext)
        {
            if (connectedPipes[_case] != null)
            {
                Debug.LogError("Pipe is already present");
                return;
            }
            connectedPipes[_case] = Instantiate(SceneRefs.ObjectFactory.PipeConnectionPrefab, transform);
            pipePart = connectedPipes[_case].transform;
            pipePart.localScale = new(
                0.1f / transform.lossyScale.x,
                pipeLenghtScale,
                0.1f / transform.lossyScale.y);
            AddRenderer(pipePart.GetComponent<Renderer>());
        }
        connectedPipes[_case].connectedPipe = connectedPipe;
        switch (_case)
        {
            case 0:
                pipePart.SetLocalPositionAndRotation(
                    new((0.5f / transform.lossyScale.z) - pipeLenghtScale, 0, 0),
                    Quaternion.Euler(90, 0, 90));
                if (canNext)
                    connectedPipe.ConnectPipe(connectionOrder[_case], this, false);
                break;
            case 1:
                pipePart.SetLocalPositionAndRotation(
                    new(-((0.5f / transform.lossyScale.z) - pipeLenghtScale), 0, 0),
                    Quaternion.Euler(90, 0, 90));
                if (canNext)
                    connectedPipe.ConnectPipe(connectionOrder[_case], this, false);
                break;
            case 2:
                pipePart.SetLocalPositionAndRotation(
                    new(0, 0, (0.5f / transform.lossyScale.z) - pipeLenghtScale),
                    Quaternion.Euler(90, 0, 0));
                if (canNext)
                    connectedPipe.ConnectPipe(connectionOrder[_case], this, false);
                break;
            case 3:
                pipePart.SetLocalPositionAndRotation(
                    new(0, 0, -((0.5f / transform.lossyScale.z) - pipeLenghtScale)),
                    Quaternion.Euler(90, 0, 0));
                if (canNext)
                    connectedPipe.ConnectPipe(connectionOrder[_case], this, false);
                break;
            default:
                Debug.LogError("WTF");
                break;
        }
    }

    public override void DestoyBuilding()
    {
        GridPos gridPos = GetPos();
        for (int x = 0; x < 4; x++)
        {
            DisconnectPipe(x, true, false);
        }
        MyGrid.SetGridItem(gridPos, null, true);
        if (network.networkID != -1)
            network.Split(this);
        base.DestoyBuilding();
    }

    public void DisconnectPipe(int _case, bool canNext, bool destroyConnection = true)
    {
        try
        {
            PipePart connection = connectedPipes[_case];
            if (connection != null)
            {
                if (destroyConnection)
                {
                    Destroy(connection.gameObject);
                    connectedPipes[_case] = null;
                    RemoveRenderer(connection.GetComponent<Renderer>());
                }

                if (canNext)
                {
                    Pipe p = connection.connectedPipe;
                    if (p != null)
                    {
                        p.DisconnectPipe(_case % 2 == 0 ? _case + 1 : _case - 1, false);
                    }
                }
                else
                    connection.connectedPipe = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("FUCK" + e);
        }
    }

    protected virtual void AddRenderer(Renderer _renderer)
    {
        meshRenderers.Add(_renderer);
        UpdateRenderMode(meshRenderers.Count - 1, meshRenderers[0].material);
    }

    protected virtual void RemoveRenderer(Renderer _renderer)
    {
        meshRenderers.Remove(_renderer);
    }
    #region Window

    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("Pipes", new List<string> { "Pipe Info" });
        base.ToggleInfoComponents(info, toEnable);
    }
    #endregion

    #region Print
    string PrintBuildings()
    {
        string s = "";
        foreach (Building b in network.buildings)
        {
            s += $"{b.objectName}: {b.id}\n";
        }
        return s;
    }
    string PrintStoredFluids()
    {
        string s = "";
        int water = 0;
        int steam = 0;
        foreach (Building b in network.buildings)
        {
            /*int i = b.localRes.type.IndexOf(resourceType.water);
            if (i > -1)
                water += b.localRes.ammount[i];
            i = b.localRes.type.IndexOf(resourceType.steam);*/

        }
        if (water > 0)
            s += $"Water: {water}";
        if (steam > 0)
            s += $"Steam: {steam}";
        return s;
    }
    #endregion

    public virtual void PlacePipe()
    {
        MyGrid.SetBuilding(this);
        MyGrid.SetGridItem(GetPos(), this, true);
        FindConnections();
        if (constructed)
            FinishBuild();
    }

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (id == -1)
            return null;
        // if constructed or ordered to be deconstructed save all data
        if (clickable == null)
            clickable = new PipeBSave();
        (clickable as PipeBSave).networkID = network.networkID;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
        network.networkID = -1;// = (save as PipeBSave).networkID;
        PlacePipe();
    }
    #endregion

    public override GridPos GetPos()
    {
        return new(
            transform.position.x,
            (transform.position.y - ClickableObjectFactory.PIPE_OFFSET) / 2,
            transform.position.z);
    }

    public void FindConnections(bool canPlace = true)
    {
        GridPos pos = GetPos();
        for (int i = 0; i < 4; i++) // checks in every direction
        {
            GridPos checkVec = new();
            switch (i)
            {
                case 0:
                    checkVec = new(pos.x + 1, pos.z);
                    if (checkVec.x == MyGrid.gridSize(pos.y))
                        continue;
                    break;
                case 1:
                    checkVec = new(pos.x - 1, pos.z);
                    if (checkVec.x < 0)
                        continue;
                    break;
                case 2:
                    checkVec = new(pos.x, pos.z + 1);
                    if (checkVec.z == MyGrid.gridSize(pos.y))
                        continue;
                    break;
                case 3:
                    checkVec = new(pos.x, pos.z - 1);
                    if (checkVec.z < 0)
                        continue;
                    break;
            }
            DisconnectPipe(i, true);
            Pipe connectedPipe = MyGrid.GetGridItem(checkVec, true) as Pipe;
            if (connectedPipe && canPlace)
            {
                ConnectPipe(i, connectedPipe, true);
            }
        }
    }

}
