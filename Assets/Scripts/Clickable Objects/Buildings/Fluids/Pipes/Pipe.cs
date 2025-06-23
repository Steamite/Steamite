using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class Pipe : Building
{
    public FluidNetwork network = new();
    PipePart[] connectedPipes = new PipePart[4];
    readonly int[] connectionOrder = { 1, 0, 3, 2};
    public override void OrderDeconstruct()
    {
        base.OrderDeconstruct();
        if (Input.GetButton("Shift"))
        {
            foreach (Pipe pipe in transform.GetComponentsInChildren<PipePart>().Select(q => q.connectedPipe).Where(q => q.deconstructing == false))
            {
                pipe.OrderDeconstruct();
            }
        }
    }
    public override void FinishBuild()
    {
        base.FinishBuild();
        List<FluidNetwork> connectedNetworks = transform.GetComponentsInChildren<PipePart>().Select(q => q.connectedPipe.network).Where(q => q.networkID != -1).ToList();
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
                    network.Merge(MyGrid.fluidNetworks.First(q => q.networkID == z));
                }
            }
        }
        else
        {
            MyGrid.fluidNetworks.Add(new(this));
            network = MyGrid.fluidNetworks.Last();
        }
    }
    public override void ChangeRenderMode(bool transparent)
    {
        foreach (MeshRenderer mesh in transform.GetComponentsInChildren<MeshRenderer>())
        {
            Material material = mesh.material;
            if (transparent)
            {
                // transparent
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                material.color = new(1, 1, 1, 0.5f);
            }
            else
            {
                // opaque
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                material.color = new(material.color.r, material.color.g, material.color.b, 0.5f);
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
        maximalProgress = cost.Sum() * 2;
        SceneRefs.gridTiles.HighLight(new(), gameObject);

        SceneRefs.jobQueue.AddJob(JobState.Constructing, this); // creates a new job with the data above
        MyRes.UpdateResource(cost, -1);
    }
    public void ConnectPipe(int _case, Pipe connectedPipe, bool canNext)
    {
        float pipeLenghtScale = (1 - transform.lossyScale.z) / 4 / transform.lossyScale.z;
        Transform pipePart;
        if ((pipePart = transform.GetComponentsInChildren<Transform>().FirstOrDefault(q => q.name == _case.ToString())) == null || !canNext)
        {
            if (connectedPipes[_case] != null)
                Debug.LogError("Pipe is already present");
            connectedPipes[_case] = Instantiate(SceneRefs.objectFactory.PipeConnectionPrefab, transform);
            pipePart = connectedPipes[_case].transform;
            pipePart.localScale = new(
                0.1f / transform.lossyScale.x,
                pipeLenghtScale,
                0.1f / transform.lossyScale.y);
            _meshRenderers.Add(pipePart.GetComponent<Renderer>());

            pipePart.GetComponent<MeshRenderer>().sharedMaterial = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
        }
        connectedPipes[_case].connectedPipe = connectedPipe;
        switch (_case)
        {
            case 0:
                pipePart.rotation = Quaternion.Euler(90, 0, 90);
                pipePart.localPosition = new((0.5f/transform.lossyScale.z) - pipeLenghtScale, 0, 0);
                if (canNext)
                    connectedPipe.ConnectPipe(connectionOrder[_case], this, false);
                break;
            case 1:
                pipePart.rotation = Quaternion.Euler(90, 0, 90);
                pipePart.localPosition = new(-((0.5f / transform.lossyScale.z) - pipeLenghtScale), 0, 0);
                if (canNext)
                    connectedPipe.ConnectPipe(connectionOrder[_case], this, false);
                break;
            case 2:
                pipePart.rotation = Quaternion.Euler(90, 0, 0);
                pipePart.localPosition = new(0, 0, (0.5f / transform.lossyScale.z) - pipeLenghtScale);
                if (canNext)
                    connectedPipe.ConnectPipe(connectionOrder[_case], this, false);
                break;
            case 3:
                pipePart.rotation = Quaternion.Euler(90, 0, 0);
                pipePart.localPosition = new(0, 0, -((0.5f / transform.lossyScale.z) - pipeLenghtScale));
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
            DisconnectPipe(x, true);
        }
        if (id == -1)
            Destroy(gameObject);
        else
        {
            MyGrid.SetGridItem(gridPos, null, true);
            if (network.networkID != -1)
                network.Split(this);
            base.DestoyBuilding();
        }
    }

    public void DisconnectPipe(int _case, bool canNext)
    {
        try
        {
            PipePart connection = connectedPipes[_case];
            if (connection != null)
            {
                Destroy(connection.gameObject);
                _meshRenderers.Remove(connection.GetComponent<Renderer>());
                connectedPipes[_case] = null;
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

    protected override void UpdateConstructionProgressAlpha()
    {
        for (int i = 0; i < _meshRenderers.Count; i++)
        {
            _meshRenderers[i].material.color
                = new(_materialColors[0].r, _materialColors[0].g, _materialColors[0].b, 0.1f + (constructionProgress / maximalProgress) * 0.9f);
        }
    }

    #region Window

    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.Add("Pipes", new List<string> { "Pipe Info"});
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
        // if constructed or ordered to be deconstructed save all data
        if (!constructed || deconstructing)
        {
            if (clickable == null)
                clickable = new PipeBSave();
            (clickable as PipeBSave).networkID = network.networkID;
            return base.Save(clickable);
        }
        else
        {
            // else save only id of pipe and id of network
            if (clickable == null)
                clickable = new LightWeightPipeBSave();
            (clickable as LightWeightPipeBSave).networkID = network.networkID;
            clickable.id = id;
            clickable.objectName = objectName;
            return clickable;
        }
    }
    public override void Load(ClickableObjectSave save)
    {
        if (save is PipeBSave)
        {
            network.networkID = (save as PipeBSave).networkID;
            base.Load(save);
        }
        else
        {
            gameObject.layer = 6;
            constructed = true;
            id = save.id;
            network.networkID = (save as LightWeightPipeBSave).networkID;
        }
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
