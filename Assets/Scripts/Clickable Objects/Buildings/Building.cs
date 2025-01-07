using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Rendering;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements;
using NUnit.Framework;

public class Building : StorageObject
{
    [SerializeField]
    protected List<Color> myColor;
    [Header("Base")]
    public Build build = new();

    #region Basic Operations
    protected virtual void Awake()
    {
        GetColors();
    }
    public override void UniqueID()
    {
        CreateNewId(MyGrid.buildings.Select(q => q.id).ToList());
    }
    public override void GetID(JobSave jobSave)
    {
        jobSave.objectId = id;
        jobSave.objectType = typeof(Building);
    }
    public override GridPos GetPos()
    {
        GridPos pos = MyGrid.Rotate(build.blueprint.moveBy, transform.rotation.eulerAngles.y);
        return new(
            transform.position.x - pos.x,
            (transform.position.y - 1) / 2,
            transform.position.z - pos.z);
    }
    #endregion

    #region Window
    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Building);
        info.ToggleChildElems(info.buildingElement, new() { build.constructed ? "Constructed" : "Construction-View" });

        OpenWindowWithToggle(info, new());
        return info;
    }

    protected virtual void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        info.ToggleChildElems(info.constructedElement, toEnable);
    }

    /*protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info); 
        if (!build.constructed)
        {
            info.building.Q<Label>("Progress").text = $"Construction Progress: {(build.constructionProgress / (float)build.maximalProgress * 100f):0}%";
            info.FillResourceList(info.building.Q<VisualElement>("Resources"), localRes.stored, build.cost);
        }
    }*/
    #endregion

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new BSave();
        (clickable as BSave).build = build;
        (clickable as BSave).prefabName = name;
        (clickable as BSave).rotationY = transform.rotation.eulerAngles.y;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        name = (save as BSave).prefabName;
        build = (save as BSave).build;
        GetColors();
        if (build.constructed)
        {
            foreach (GameObject g in transform.GetComponentsInChildren<Transform>().Select(q => q.gameObject))
            {
                g.layer = 6;
            }
            GetComponent<SortingGroup>().sortingLayerName = "Buildings";
            if (build.deconstructing)
            {
                SceneRefs.humans.GetComponent<JobQueue>().AddJob(JobState.Deconstructing, this);
            }
        }
        else
        {
            PlaceBuilding(SceneRefs.gridTiles);
        }
        base.Load(save);
    }
    #endregion Saving

    #region Storage
    public override void Store(Human human, int transferPerTick)
    {
        int index = localRes.carriers.IndexOf(human);
        MyRes.MoveRes(localRes.stored, human.Inventory, localRes.requests[index], transferPerTick);
        UIUpdate(nameof(LocalRes));
        if (localRes.requests[index].ammount.Sum() == 0)
        {
            if (!build.constructed && localRes.stored.Equals(build.cost))
            {
                human.SetJob(JobState.Constructing);
                human.ChangeAction(HumanActions.Build);
                localRes.mods[index] = 0;
                return;
            }
            localRes.RemoveRequest(human);
            HumanActions.LookForNew(human);
        }
    }
    public override void Take(Human h, int transferPerTick)
    {
        base.Take(h, transferPerTick);
    }
    #endregion Storing

    #region Construction & Deconstruction
    /// <summary>
    /// sets constructed to true, clears resource for which it was built, and changes color to the original one
    /// </summary>
    public virtual void FinishBuild()
    {
        build.constructed = true;
        for (int i = 0; i < localRes.stored.ammount.Count; i++)
        {
            localRes.stored.ammount[i] = 0;
        }
        ChangeRenderMode(false);
    }

    public virtual void OrderDeconstruct()
    {
        JobQueue queue = SceneRefs.humans.GetComponent<JobQueue>();
        if (build.constructed) // if contructed
        {
            // if there isn't a deconstruction order yet
            if (!build.deconstructing)
            {
                // create a new order for deconstruction
                localRes.ReassignCarriers();
                build.deconstructing = true;
                queue.AddJob(JobState.Deconstructing, this);
                //Material m = GetComponent<MeshRenderer>().material;
                //m.SetColor("_EmissionColor", m.GetColor("_EmissionColor") + Color.red);
                //GetComponent<MeshRenderer>().material.EnableKeyword("_Emission");
            }
            else
            {
                // if there is cancel it
                queue.CancelJob(JobState.Deconstructing, this);
                build.deconstructing = false;
            }
        }
        else
        {
            
            // if there are any resources deposited(change to build progress when implemented)
            if (build.constructionProgress > 0)
            {
                if (!build.deconstructing)
                {
                    queue.CancelJob(JobState.Constructing, this);
                    queue.AddJob(JobState.Deconstructing, this);
                    localRes.ReassignCarriers();
                }
                else
                {
                    queue.AddJob(JobState.Constructing, this);
                    queue.CancelJob(JobState.Deconstructing, this);
                    if(localRes.carriers.Count > 0)
                    {
                        localRes.carriers[0].SetJob(JobState.Constructing);
                        localRes.carriers[0].ChangeAction(HumanActions.Build);
                    }
                }
                build.deconstructing = !build.deconstructing;
            }
            else
            {
                queue.CancelJob(JobState.Constructing, this);
                queue.AddJob(JobState.Deconstructing, this);
                foreach (Human carrier in localRes.carriers)
                {
                    MyRes.FindStorage(carrier);
                }
                Deconstruct(GetPos());
            }
        }
    }
    public virtual Chunk Deconstruct(GridPos instantPos)
    {
        Resource r = new();
        // if constructed
        if (build.constructed)
        {
            // get half of build cost
            MyRes.ManageRes(r, build.cost, 1);
            for (int i = 0; i < r.ammount.Count; i++)
            {
                r.ammount[i] /= 2;
            }
        }
        // get all stored resources
        MyRes.ManageRes(r, localRes.stored, 1);
        Chunk c = null;
        if (r.ammount.Sum() > 0)
            c = SceneRefs.objectFactory.CreateAChunk(instantPos, r);
        MyGrid.UnsetBuilding(this);
        DestoyBuilding(); // destroy self
        return c; 
    }
    public virtual void DestoyBuilding()
    {
        SceneRefs.gridTiles.DeselectObjects();
        Destroy(gameObject);
        if (id > -1)
        {
            MyGrid.UnsetBuilding(this);
        }
        else
        {
            MyGrid.GetOverlay().DeleteBuildGrid();
        }
    }

    public virtual void ChangeRenderMode(bool transparent)
    {
        foreach (Material material in transform.GetComponentsInChildren<MeshRenderer>().Select(q => q.material))
        {
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
    #endregion

    #region Placing
    public virtual Resource GetDiff(Resource inventory)
    {
        Resource r = null;
        r = MyRes.DiffRes(build.cost, localRes.Future(), inventory);
        return r;
    }
    public virtual List<string> GetInfoText()
    {
        return new() { $"<u>Costs</u>:\n{MyRes.GetDisplayText(build.cost)}" };
    }

    public virtual bool CanPlace()
    {
        if (MyRes.CanAfford(build.cost) && MyGrid.CanPlace(this))
            return true;
        else
            return false;
    }
    public virtual void PlaceBuilding(GridTiles gT)
    {
        foreach (Transform t in transform.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = 6;
        }
        GetComponent<SortingGroup>().sortingLayerName = "Buildings";
        build.maximalProgress = build.cost.ammount.Sum() * 2;
        gT.HighLight(new(), gameObject);

        SceneRefs.humans.GetComponent<JobQueue>().AddJob(JobState.Constructing, this); // creates a new job with the data above
        MyRes.UpdateResource(build.cost, -1);
        MyGrid.SetBuilding(this);
        print(MyGrid.PrintGrid());
    }

    public void GetColors() => myColor = transform.GetComponentsInChildren<MeshRenderer>().Select(q => q.material.color).ToList(); // saves the original color
    #endregion
}