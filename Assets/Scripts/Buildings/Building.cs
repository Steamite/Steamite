using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Rendering;
using System.Collections.Generic;
using TMPro;

public class Building : StorageObject
{
    protected Color myColor;
    [Header("Base")]
    public Build build = new();

    protected virtual void Awake()
    {
        myColor = gameObject.GetComponent<MeshRenderer>().material.color; // saves the original color
    }

    public override void UniqueID()
    {
        CreateNewId(MyGrid.buildings.Select(q => q.id).ToList());
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
                info.SwitchMods(0, name);
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
        ChangeColor(new Color()); // builds
        ChangeRenderMode(false);
        OpenWindow(true); // if selected, update the info window
    }
    public virtual void OrderDeconstruct()
    {
        JobQueue queue = GameObject.Find("Humans").GetComponent<JobQueue>();
        if (build.constructed) // if contructed
        {
            // if there isn't a deconstruction order yet
            if (!build.deconstructing)
            {
                // create a new order for deconstruction
                localRes.ReassignCarriers();
                build.deconstructing = true;
                queue.AddJob(JobState.Deconstructing, this);
                Material m = GetComponent<MeshRenderer>().material;
                m.SetColor("_EmissionColor", m.GetColor("_EmissionColor") + Color.red);
                GetComponent<MeshRenderer>().material.EnableKeyword("_Emission");
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
                queue.constructions.RemoveAll(q => q.id == id);
                queue.deconstructions.Add(this);
                localRes.ReassignCarriers();
            }
            else
            {
                foreach (Human carrier in localRes.carriers)
                {
                    MyRes.FindStorage(carrier);
                    carrier.ChangeAction(HumanActions.Move);
                    carrier.jData.job = JobState.Supply;
                }
                Deconstruct(transform.position);
            }
        }
    }
    public virtual Chunk Deconstruct(Vector3 instantPos)
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
        if (r.ammount.Sum() > 0) // if there's anything to salvage
        {
            // create a chunk containing salvagable resources
            c = Instantiate(MyGrid.specialPrefabs.GetPrefab("Chunk"), instantPos, Quaternion.identity, GameObject.Find("Chunks").transform).GetComponent<Chunk>();
            c.Create(r, true);
        }

        MyGrid.RemoveBuilding(this);
        transform.parent.parent.parent.parent.GetComponent<GridTiles>().Remove(this);
        DestoyBuilding(); // destroy self
        return c;
    }

    public virtual void ChangeColor(Color color)
    {
        if (color.Equals(new Color()) || build.constructed) // signal for restart of color
        {
            gameObject.GetComponent<MeshRenderer>().material.color = myColor;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material.color = color;
        }
    }
    public virtual void ChangeRenderMode(bool transparent)
    {
        Material material = gameObject.GetComponent<MeshRenderer>().material;
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
    public virtual Resource GetDiff(Resource inventory)
    {
        Resource r = new();
        r = MyRes.DiffRes(build.cost, localRes.Future(false), inventory);
        return r;
    }

    ///////////////////////////////////////////////////
    //---------------Saving & Loading----------------//
    ///////////////////////////////////////////////////
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
        if (build.constructed)
        {
            gameObject.layer = 6;
            GetComponent<SortingGroup>().sortingLayerName = "Buildings";
            if (build.deconstructing)
            {
                GameObject.Find("Humans").GetComponent<JobQueue>().AddJob(JobState.Deconstructing, this);
            }

        }
        else
        {
            PlaceBuilding(MyGrid.gridTiles);
        }
        GetComponent<Building>().ChangeColor(new Color());
        base.Load(save);
    }
    public override void GetID(JobSave jobSave)
    {
        jobSave.objectId = id;
        jobSave.objectType = typeof(Building);
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
    public virtual List<string> GetInfoText()
    {
        return new() { $"<u>Costs</u>:\n{MyRes.GetDisplayText(build.cost)}" };
    }

    public virtual bool CanPlace()
    {
        if (MyRes.DiffRes(build.cost, MyRes.resources, new()).ammount.Sum() == 0)
        {
            return MyGrid.CanPlace(this);
        }
        return false;
    }
    public virtual void PlaceBuilding(GridTiles gT)
    {
        gameObject.layer = 6;
        GetComponent<SortingGroup>().sortingLayerName = "Buildings";
        build.maximalProgress = build.cost.ammount.Sum() * 2;
        gT.HighLight(new(), gameObject);

        Humans humans = GameObject.FindWithTag("Humans").GetComponent<Humans>();
        humans.GetComponent<JobQueue>().AddJob(JobState.Constructing, this); // creates a new job with the data above
        MyRes.UpdateResource(build.cost, -1);
        MyGrid.PlaceBuild(this);
        print(MyGrid.PrintGrid());
    }
    public virtual void DestoyBuilding()
    {
        Destroy(gameObject);
        if(transform.childCount > 0 && transform.GetChild(0).childCount > 0)
        {
            foreach (GridPos gp in transform.GetChild(0).GetComponentsInChildren<Transform>().Skip(1).Select(q=> new GridPos(q.transform.position)))
            {
                MyGrid.grid[(int)gp.x, (int)gp.z].GetComponent<Road>()?.entryPoints.Remove(id);
            }
        }
    }
    public virtual Fluid GetFluid()
    {
        return null;
    }
}