using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using Unity.Properties;
using UnityEngine.UIElements;

/// <summary>
/// Class for buildings, which can be constructed on free tiles. 
/// Each building needs atleast one free access point.
/// </summary>
public class Building : StorageObject
{
    #region Variables
    /// <summary>Used for determening buildings.</summary>
    [SerializeField] protected List<Color> myColor;
    
    /// <summary>Building layout(entry points, anchor, ...).</summary>
    public BuildingGrid blueprint;
    /// <summary>Construction cost in resources.</summary>
    public Resource cost;
    /// <summary>Is constructed.</summary>
    public bool constructed;
    /// <summary>Is being deconstructed.</summary>
    public bool deconstructing;
    /// <summary>Progress of construction/deconstruction.</summary>
    [CreateProperty] public float constructionProgress;
    /// <summary>.</summary>
    public int maximalProgress;


    #endregion

    #region Basic Operations
    /// <summary>Fills<see cref="myColor"/>.</summary>
    protected virtual void Awake() => GetColors();

    /// <summary>Creates a list from <see cref="MyGrid.buildings"/></summary>
    public override void UniqueID() => CreateNewId(MyGrid.buildings.Select(q => q.id).ToList());
    /// <summary>
    /// Calculates positio using the anchor from <see cref="blueprint"/>.
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override GridPos GetPos()
    {
        GridPos pos = MyGrid.Rotate(blueprint.moveBy, transform.rotation.eulerAngles.y);
        return new(
            transform.position.x - pos.x,
            (transform.position.y - 1) / 2,
            transform.position.z - pos.z);
    }
    #endregion

    #region Window
    /// <summary>
    /// <inheritdoc/>
    /// Also toggle contsructed view, and other child elements.
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Building);
        
        if(constructed)
            OpenWindowWithToggle(info, new());
        return info;
    }

    /// <summary>
    /// Opens and fills needed components in Building Visual Element.
    /// </summary>
    /// <param name="info"><see cref="InfoWindow"/> supplied from <see cref="OpenWindow"/>.</param>
    /// <param name="toEnable">List of components to enable in the Visual Element.</param>
    protected virtual void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        info.ToggleChildElems(info.constructedElement, toEnable, this);
    }


    #endregion

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new BSave();
        (clickable as BSave).prefabName = name;
        (clickable as BSave).rotationY = transform.rotation.eulerAngles.y;
        
        (clickable as BSave).blueprint = blueprint;
        (clickable as BSave).cost = cost;
        (clickable as BSave).constructed = constructed;
        (clickable as BSave).deconstructing = deconstructing;
        (clickable as BSave).constructionProgress = constructionProgress;
        (clickable as BSave).maximalProgress = maximalProgress;

        return base.Save(clickable);
    }
    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        name = (save as BSave).prefabName;
        blueprint = (save as BSave).blueprint;
        cost = (save as BSave).cost;
        constructed = (save as BSave).constructed;
        deconstructing = (save as BSave).deconstructing;
        constructionProgress = (save as BSave).constructionProgress;
        maximalProgress = (save as BSave).maximalProgress;
        GetColors();

        if (constructed)
        {
            foreach (GameObject g in transform.GetComponentsInChildren<Transform>().Select(q => q.gameObject))
            {
                g.layer = 6;
            }
            GetComponent<SortingGroup>().sortingLayerName = "Buildings";
            if (deconstructing)
            {
                SceneRefs.jobQueue.AddJob(JobState.Deconstructing, this);
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
    /// <summary>
    /// Stored resources are used for constructing.
    /// </summary>
    /// <param name="human"><inheritdoc/></param>
    /// <param name="transferPerTick"><inheritdoc/></param>
    public override void Store(Human human, int transferPerTick)
    {
        int index = localRes.carriers.IndexOf(human);
        MyRes.MoveRes(localRes.stored, human.Inventory, localRes.requests[index], transferPerTick);
        UIUpdate(nameof(LocalRes));
        if (localRes.requests[index].ammount.Sum() == 0)
        {
            if (!constructed && localRes.stored.Equals(cost))
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
    #endregion Storing

    #region Construction
    /// <summary>
    /// Adds <paramref name="progress"/> to construction progress. If it reaches max progress
    /// </summary>
    /// <param name="progress">Ammount to add.</param>
    /// <returns>If the construction is finnished.</returns>
    public bool ProgressConstruction(float progress)
    {
        constructionProgress += progress;
        if (constructionProgress >= maximalProgress)
        {
            FinishBuild();
            return true;
        }
        UIUpdate(nameof(constructionProgress));
        return false;
    }

    /// <summary>Sets constructed to true, clears resource for which it was built, and changes color to the original one.</summary>
    public virtual void FinishBuild()
    {
        constructed = true;
        for (int i = 0; i < localRes.stored.ammount.Count; i++)
        {
            localRes.stored.ammount[i] = 0;
        }
        ChangeRenderMode(false);
        OpenWindow();
    }
    #endregion

    #region Deconstruction
    /// <summary>If being constructed delete the building, else toogle deconstruction. <b>TODO: COLOR CHANGING</b></summary>
    public virtual void OrderDeconstruct()
    {
        JobQueue queue = SceneRefs.jobQueue;
        if (constructed) // if contructed
        {
            // if there isn't a deconstruction order yet
            if (!deconstructing)
            {
                // create a new order for deconstruction
                localRes.ReassignCarriers();
                deconstructing = true;
                queue.AddJob(JobState.Deconstructing, this);
                //Material m = GetComponent<MeshRenderer>().material;
                //m.SetColor("_EmissionColor", m.GetColor("_EmissionColor") + Color.red);
                //GetComponent<MeshRenderer>().material.EnableKeyword("_Emission");
            }
            else
            {
                // if there is cancel it
                queue.CancelJob(JobState.Deconstructing, this);
                deconstructing = false;
            }
        }
        else
        {
            // if there are any resources deposited(change to build progress when implemented)
            if (constructionProgress > 0)
            {
                if (!deconstructing)
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
                deconstructing = !deconstructing;
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

    /// <summary>
    /// Creates a <see cref="Chunk"/> containing half of construction cost and <see cref="localRes.stored"/>.
    /// </summary>
    /// <param name="instantPos">Where to create the <see cref="Chunk"/>.</param>
    /// <returns>Created <see cref="Chunk"/>.</returns>
    public virtual Chunk Deconstruct(GridPos instantPos)
    {
        Resource r = new();
        if (constructed)
        {
            MyRes.ManageRes(r, cost, 1);
            for (int i = 0; i < r.ammount.Count; i++)
            {
                r.ammount[i] /= 2;
            }
        }
        MyRes.ManageRes(r, localRes.stored, 1);
        DestoyBuilding(); // destroy self
        return SceneRefs.objectFactory.CreateAChunk(instantPos, r, true);
    }
    /// <summary>
    /// Removes the building from 
    /// </summary>
    public virtual void DestoyBuilding()
    {
        SceneRefs.gridTiles.DestroyUnselect(this);
        if (id > -1)
        {
            MyGrid.UnsetBuilding(this);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Changes building materials to transparent or opague.
    /// </summary>
    /// <param name="transparent">Requested render mode.</param>
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
                material.color = new(material.color.r, material.color.g, material.color.b, 1);
            }
        }
    }
    #endregion

    #region Placing
    /// <summary>
    /// Calculates missing resources needed for construction.
    /// </summary>
    /// <param name="inventory">Inventory of the carrier.</param>
    /// <returns>Missing resources.</returns>
    public virtual Resource GetDiff(Resource inventory)
    {
        Resource r = null;
        r = MyRes.DiffRes(cost, localRes.Future(), inventory);
        return r;
    }
    /// <summary>Short info for building buttons.</summary>
    public virtual List<string> GetInfoText()
    {
        return new() { $"<u>Costs</u>:\n{cost.GetDisplayText()}" };
    }

    /// <summary>Checks if you can afford the building.</summary>
    public virtual bool CanPlace()
    {
        if (MyRes.CanAfford(cost) && MyGrid.CanPlace(this))
            return true;
        else
            return false;
    }

    /// <summary>
    /// Placing building by player.
    /// </summary>
    /// <param name="gT"></param>
    public virtual void PlaceBuilding(GridTiles gT)
    {
        foreach (Transform t in transform.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = 6;
        }
        GetComponent<SortingGroup>().sortingLayerName = "Buildings";
        maximalProgress = cost.ammount.Sum() * 2;
        gT.HighLight(new(), gameObject);

        SceneRefs.jobQueue.AddJob(JobState.Constructing, this); // creates a new job with the data above
        MyRes.UpdateResource(cost, -1);
        UniqueID();

        MyGrid.SetBuilding(this);
    }

    /// <summary>
    /// Fills <see cref="myColor"/>.
    /// </summary>
    public void GetColors() => myColor = transform.GetComponentsInChildren<MeshRenderer>().Select(q => q.material.color).ToList(); // saves the original color
    #endregion
}