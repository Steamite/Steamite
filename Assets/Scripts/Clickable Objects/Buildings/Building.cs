using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Rendering;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements;

public class Building : StorageObject
{
    protected List<Color> myColor;
    [Header("Base")]
    public Build build = new();

    ///////////////////////////////////////////////////
    ///////////////////Overrides///////////////////////
    ///////////////////////////////////////////////////
    public override void UniqueID()
    {
        CreateNewId(MyGrid.buildings.Select(q => q.id).ToList());
    }
    public override void GetID(JobSave jobSave)
    {
        jobSave.objectId = id;
        jobSave.objectType = typeof(Building);
    }

    /// <summary>
    /// Sets up/Updates info window
    /// </summary>
    /// <param name="setUp">false = when only updating</param>
    /// <returns></returns>
    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info;
        // if selected
        if ((info = base.OpenWindow(setUp)) != null)
        {
            // sets window mod to buildings and toggles constructed/unconstructed
            if (setUp)
            {
                info.SwitchMods(InfoMode.Building, name);
                info.cTransform.parent.GetChild(0).gameObject.SetActive(!build.constructed); // unconstructed
                info.cTransform.gameObject.SetActive(build.constructed); // constructed
                for (int i = 0; i < info.cTransform.childCount; i++)
                {
                    info.cTransform.GetChild(i).gameObject.SetActive(false); // deactivate all build windows
                }
            }
            if (build.constructed)
                return info;
            else
                DefText(info.cTransform);
        }
        return null;
    }

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

    #region Storing
    public override void Store(Human human, int transferPerTick)
    {
        int index = localRes.carriers.IndexOf(human);
        MyRes.MoveRes(localRes.stored, human.inventory, localRes.requests[index], transferPerTick);
        if (localRes.requests[index].ammount.Sum() == 0)
        {
            if (!build.constructed && localRes.stored.Equals(build.cost))
            {
                human.jData.job = JobState.Constructing;
                human.ChangeAction(HumanActions.Build);
                localRes.mods[index] = 0;
                return;
            }
            localRes.RemoveRequest(human);
            HumanActions.LookForNew(human);
        }
        OpenWindow();
    }
    public override void Take(Human h, int transferPerTick)
    {
        base.Take(h, transferPerTick);
        OpenWindow();
    }
    #endregion Storing

    public override GridPos GetPos()
    {
        GridPos pos = MyGrid.Rotate(build.blueprint.moveBy, transform.rotation.eulerAngles.y);
        return new(
            transform.position.x - pos.x,
            (transform.position.y - 1) / 2,
            transform.position.z - pos.z);
    }
    ///////////////////////////////////////////////////
    ///////////////////Methods/////////////////////////
    ///////////////////////////////////////////////////
    protected virtual void Awake()
    {
        GetColors();
    }

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
        OpenWindow(true); // if selected, update the info window
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
                        localRes.carriers[0].jData.job = JobState.Constructing;
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
                    carrier.ChangeAction(HumanActions.Move);
                    carrier.jData.job = JobState.Supply;
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
        MyGrid.RemoveBuilding(this);
        DestoyBuilding(); // destroy self
        return c; 
    }
    public virtual void DestoyBuilding()
    {
        SceneRefs.gridTiles.DeselectObjects();
        Destroy(gameObject);
        if (id > -1)
        {
            MyGrid.RemoveBuilding(this);
        }
        else
        {
            MyGrid.GetOverlay().DeleteBuildGrid();
        }
    }
    public virtual void ChangeRenderMode(bool transparent)
    {
        foreach(Material material in transform.GetComponentsInChildren<MeshRenderer>().Select(q=> q.material))
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
        //bool can = MyRes.DiffRes(build.cost, MyRes.resources, new()).ammount.Sum() == 0;
        bool tmp = MyGrid.CanPlace(this);
        if (tmp)
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

        Humans humans = GameObject.FindWithTag("Humans").GetComponent<Humans>();
        humans.GetComponent<JobQueue>().AddJob(JobState.Constructing, this); // creates a new job with the data above
        MyRes.UpdateResource(build.cost, -1);
        MyGrid.PlaceBuild(this);
        print(MyGrid.PrintGrid());
    }

    public void GetColors()
    {
        myColor = transform.GetComponentsInChildren<MeshRenderer>().Select(q => q.material.color).ToList(); // saves the original color
    }
    public virtual Fluid GetFluid()
    {
        return null;
    }


    private void DefText(Transform cTransform)
    {
        // deactivate constructed part and activate unconstructed
        cTransform.gameObject.SetActive(false);
        cTransform.parent.GetChild(0).gameObject.SetActive(true);
        // create a string and set it
        string infoText =
            $"{MyRes.GetDisplayText(localRes.stored, build.cost)}\n" +
            $"Constructed: {build.constructionProgress}/{build.maximalProgress}";
        cTransform.parent.GetChild(0).GetChild(0).GetComponent<TMP_Text>()
            .text = infoText;
    }
}