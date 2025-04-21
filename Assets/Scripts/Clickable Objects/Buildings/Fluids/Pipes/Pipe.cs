using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class Pipe : Building
{
    public FluidNetwork network = new();
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
    public override bool CanPlace()
    {
        if (MyRes.CanAfford(cost))
        {
            return MyGrid.CanPlace(this);
        }
        return false;
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
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
    }
    public override void PlaceBuilding(GridTiles gT)
    {
        gameObject.layer = 7;
        GetComponent<SortingGroup>().sortingLayerName = "Pipes";
        maximalProgress = cost.ammount.Sum() * 2;
        gT.HighLight(new(), gameObject);

        HumanUtil humans = GameObject.FindWithTag("Humans").GetComponent<HumanUtil>();
        humans.GetComponent<JobQueue>().AddJob(JobState.Constructing, this); // creates a new job with the data above
        MyRes.UpdateResource(cost, -1);
    }
    public void ConnectPipe(int _case, Pipe connectedPipe, bool canNext)
    {
        Transform pipePrefab;
        if ((pipePrefab = transform.GetComponentsInChildren<Transform>().FirstOrDefault(q => q.name == _case.ToString())) == null || !canNext)
        {
            pipePrefab = Instantiate(SceneRefs.objectFactory.specialPrefabs.GetPrefab("Pipe connection"), transform).transform;
            pipePrefab.GetComponent<MeshRenderer>().sharedMaterial = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
            pipePrefab.gameObject.name = _case.ToString();
        }
        pipePrefab.GetComponent<PipePart>().connectedPipe = connectedPipe;
        switch (_case)
        {
            case 0:
                pipePrefab.rotation = Quaternion.Euler(90, 0, 90);
                pipePrefab.localPosition = new(1.03f, 0, 0);
                if (canNext)
                    connectedPipe.ConnectPipe(1, this, false);
                break;
            case 1:
                pipePrefab.rotation = Quaternion.Euler(90, 0, 90);
                pipePrefab.localPosition = new(-1.03f, 0, 0);
                if (canNext)
                    connectedPipe.ConnectPipe(0, this, false);
                break;
            case 2:
                pipePrefab.rotation = Quaternion.Euler(90, 0, 0);
                pipePrefab.localPosition = new(0, 0, 1.03f);
                if (canNext)
                    connectedPipe.ConnectPipe(3, this, false);
                break;
            case 3:
                pipePrefab.rotation = Quaternion.Euler(90, 0, 0);
                pipePrefab.localPosition = new(0, 0, -1.03f);
                if (canNext)
                    connectedPipe.ConnectPipe(2, this, false);
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
        Pipe pipe = (Pipe)MyGrid.GetGridItem(gridPos, true);
        if (pipe?.id == id)
        {
            MyGrid.SetGridItem(gridPos, null, true);
            if (network.networkID != -1)
                network.Split(this);
        }
        base.DestoyBuilding();
    }
    public void DisconnectPipe(int _case, bool canNext)
    {
        try
        {
            PipePart connection = null;
            GridTiles gT = GameObject.FindWithTag("Grid")?.GetComponent<GridTiles>();
            for (int i = 0; i < transform.childCount; i++)
            {
                connection = transform.GetChild(i).GetComponent<PipePart>();
                if (connection.gameObject.name == _case.ToString())
                {
                    Destroy(connection.gameObject);
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
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("FUCK" + e);
        }
    }

    #region Window

    /*protected override void SetupWindow(InfoWindow info, List<String> toEnable)
    {
        base.SetupWindow(info, toEnable);
        *//*info.SwitchMods(InfoMode.Water, $"network {network.networkID}");
        info.transform.GetChild(1).GetChild(4).GetChild(0).GetComponent<TMP_Text>().text =
            $"Network: {network.networkID} \n";*//*
    }

    protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
        *//*info.transform.GetChild(1).GetChild(4).GetChild(0).GetComponent<TMP_Text>().text +=
                $"{PrintBuildings()}" +
                $"{PrintStoredFluids()}"; *//*
    }*/
    #endregion

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
            /*int i = b.localRes.stored.type.IndexOf(resourceType.water);
            if (i > -1)
                water += b.localRes.stored.ammount[i];
            i = b.localRes.stored.type.IndexOf(resourceType.steam);*/

        }
        if (water > 0)
            s += $"Water: {water}";
        if (steam > 0)
            s += $"Steam: {steam}";
        return s;
    }
    public virtual void PlacePipe()
    {
        if (id == -1)
            UniqueID();
        GridPos pos = new(transform.position);
        MyGrid.SetGridItem(pos, this, true);
        MyGrid.CanPlace(this);
        if (constructed)
            FinishBuild();
    }
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
            constructed = true;
            id = save.id;
            network.networkID = (save as LightWeightPipeBSave).networkID;
        }
        PlacePipe();
    }
}
