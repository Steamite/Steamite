using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public enum BuildingCategType
{
    Population,
    Production,
    Research,
    Fluid
}

/// <summary>
/// Class for buildings, which can be constructed on free tiles. 
/// Each building needs atleast one free access point.
/// </summary>
public class Building : StorageObject
{
    #region Variables
    /// <summary>Used for remembering color.</summary>
    [SerializeField] public List<Renderer> meshRenderers;

    /// <summary>
    /// Mask saying which categories this building belongs to.
    /// </summary>
    [SerializeField] int buildingCategories;
    /// <inheritdoc cref="buildingCategories"/>
    public int BuildingCateg { get => buildingCategories; set => buildingCategories = value; }

    /// <summary>Building layout(entry points, anchor, ...).</summary>
    public BuildingGrid blueprint;
    /// <summary>Construction cost in resources.</summary>
    [SerializeField] protected MoneyResource cost = new();
    [CreateProperty] public MoneyResource Cost { get => cost; set => cost = value; }
    /// <summary>Is constructed.</summary>
    public bool constructed;
    /// <summary>Is being deconstructed.</summary>
    public bool deconstructing;
    /// <summary>Progress of construction/deconstruction.</summary>
    [CreateProperty] public float constructionProgress;
    /// <summary>.</summary>
    public int maximalProgress;

    [Header("Prefab info")]
    public byte categoryID;
    public int wrapperID;

    bool isTransparent = false;
    #endregion

    #region Basic Operations

    /// <summary>Creates a new unique id not present in <see cref="MyGrid.buildings"/></summary>
    public override void UniqueID() => CreateNewId(MyGrid.Buildings.Select(q => q.id).ToList());
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
        // DEBUG_Binding Common init
        // Opens the info window (if selected) and switches what is displayed.
        InfoWindow info = base.OpenWindow();
        if (info)
        {
            info.Open(this, InfoMode.Building);
            if (constructed && !deconstructing)
                ToggleInfoComponents(info, new());
        }
        return info;
    }

    /// <summary>
    /// Opens and fills needed components in "Building" VisualElement.
    /// </summary>
    /// <param name="info"><see cref="InfoWindow"/> supplied from <see cref="OpenWindow"/>.</param>
    /// <param name="toEnable">List of components to enable in the Visual Element.</param>
    protected virtual void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        info.CreateBuildingControls(toEnable, this);
    }


    #endregion

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new BuildingSave();
        BuildingSave save = (clickable as BuildingSave);
        save.prefabName = objectName;
        save.rotationY = transform.rotation.eulerAngles.y;

        save.blueprint = blueprint;
        save.constructed = constructed;
        save.deconstructing = deconstructing;
        save.constructionProgress = constructionProgress;
        save.categoryID = categoryID;
        save.wrapperID = wrapperID;

        return base.Save(save);
    }

    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        objectName = (save as BuildingSave).prefabName;
        blueprint = (save as BuildingSave).blueprint;
        constructed = (save as BuildingSave).constructed;
        deconstructing = (save as BuildingSave).deconstructing;
        constructionProgress = (save as BuildingSave).constructionProgress;
        maximalProgress = CalculateMaxProgress();
        localRes.Load((save as BuildingSave).resSave);
        GetRenderComponents();

        InitModifiers();

        gameObject.layer = 6;
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.layer = 6;
        GetComponent<SortingGroup>().sortingLayerName = "Buildings";

        if (!constructed)
        {
            SceneRefs.JobQueue.AddJob(JobState.Constructing, this);
            ChangeRenderMode(true);
        }

        base.Load(save);
        if (this is IFluidWork)
        {
            ((IFluidWork)this).CreatePipes(true);
        }
        if(this is IResourceProduction)
        {
            ((IResourceProduction)this).Init(constructed);
        }
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
        MyRes.MoveRes(
            localRes,
            human.Inventory,
            localRes.requests[index],
            transferPerTick,
            !constructed);
        UIUpdate(nameof(LocalRes));
        if (localRes.requests[index].Sum() == 0)
        {
            if (!constructed && localRes.Equals(cost))
            {
                human.SetJob(JobState.Constructing);
                localRes.mods[index] = 0;
                return;
            }
            localRes.RemoveRequest(human);
            human.SetJob(JobState.Free);
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
        UpdateConstructionProgressAlpha();
        UIUpdate(nameof(constructionProgress));
        return false;
    }

    /// <summary>
    /// Sets constructed to true, clears resource for which it was built, 
    /// and changes color to the original one.
    /// </summary>
    public virtual void FinishBuild()
    {
        if (!constructed)
        {
            ChangeRenderMode(false);
            constructed = true;
            if (this is not IStorage)
                localRes.Dump();
        }
        
        if (selected)
            OpenWindow();

        if (this is IFluidWork fluidWork)
            fluidWork.ConnectToNetwork();
    }
    #endregion

    #region Deconstruction
    /// <summary>If being constructed delete the building, else toogle deconstruction. <b>TODO: COLOR CHANGING</b></summary>
    public virtual void OrderDeconstruct()
    {
        JobQueue queue = SceneRefs.JobQueue;
        if (constructed) // if contructed
        {
            // if there isn't a deconstruction order yet
            if (!deconstructing)
            {
                Human human = null;
                queue.CancelJob(JobState.Constructing, this);
                queue.AddJob(JobState.Deconstructing, this);

                human = localRes.ReassignCarriers();
            }
            else
            {
                queue.CancelJob(JobState.Deconstructing, this);
                if(constructionProgress == maximalProgress)
                {
                    deconstructing = false;
                    OpenWindow();
                }
                else
                {
                    queue.AddJob(JobState.Constructing, this);
                    if (localRes.carriers.Count > 0)
                    {
                        localRes.carriers[0].SetJob(JobState.Constructing);
                    }
                }
            }
            deconstructing = !deconstructing;
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
                    if (localRes.carriers.Count > 0)
                    {
                        localRes.carriers[0].SetJob(JobState.Constructing);
                    }
                }
                deconstructing = !deconstructing;
            }
            else
            {
                queue.CancelJob(JobState.Constructing, this);
                foreach (Human carrier in localRes.carriers)
                {
                    if (carrier.Job.interest != null && carrier.Job.interest != this)
                    {
                        ((Building)carrier.Job.interest).LocalRes.RemoveRequest(carrier);
                    }
                    carrier.destination = null;
                    MyRes.FindStorage(carrier);
                }
                Deconstruct(GetPos());
            }
        }
    }

    public virtual bool ProgressDeconstruction(float v, Human h)
    {
        constructionProgress -= v;
        if (constructionProgress <= 0)
        {
            Deconstruct(h.GetPos());
            return true;
        }
        UpdateConstructionProgressAlpha();
        UIUpdate(nameof(constructionProgress));
        return false;
    }

    /// <summary>
    /// Creates a <see cref="Chunk"/> containing half of construction cost and <see cref="localRes"/>.
    /// </summary>
    /// <param name="instantPos">Where to create the <see cref="Chunk"/>.</param>
    /// <returns>Created <see cref="Chunk"/>.</returns>
    public virtual Chunk Deconstruct(GridPos instantPos)
    {
        Resource r = new();
        if (constructed)
        {
            r.Manage(cost, true);
            for (int i = 0; i < r.ammounts.Count; i++)
            {
                r.ammounts[i] /= 2;
            }
        }
        r.Manage(localRes, true);
        DestoyBuilding(); // destroy self
        return SceneRefs.ObjectFactory.CreateChunk(instantPos, r, true);
    }
    /// <summary>
    /// Removes the building from 
    /// </summary>
    public virtual void DestoyBuilding()
    {
        SceneRefs.GridTiles.DestroyUnselect(this);
        if (id > -1 || this is Pipe)
        {
            MyGrid.UnsetBuilding(this);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Material change
    /// <summary>
    /// Changes building materials to transparent or opague.
    /// </summary>
    /// <param name="transparent">Requested render mode.</param>
    public virtual void ChangeRenderMode(bool transparent)
    {
        isTransparent = transparent;
        Material mat;
        if (isTransparent)
        {
            mat = MaterialChanger.Transparent;
            for (int i = 0; i < meshRenderers.Count; i++)
            {
                UpdateRenderMode(i, mat);
            }
        }
        else
        {
            List<Material> materials = SceneRefs.ObjectFactory.GetModelMaterials(this);
            for (int i = 0; i < meshRenderers.Count; i++)
            {
                if(i >= materials.Count)
                    UpdateRenderMode(i, SceneRefs.ObjectFactory.GetPipeMaterial());
                else
                    UpdateRenderMode(i, materials[i]);
            }
        }
    }

    protected virtual void UpdateRenderMode(int i, Material material)
    {
        Renderer _renderer = meshRenderers[i];
        if (isTransparent)
        {
            Material newMat = new(material);
            Color c = _renderer.material.color;
            newMat.color = new(c.r, c.g, c.b);
            newMat.SetFloat("_Progress", 0.1f + (constructionProgress / maximalProgress) * 0.9f);
            _renderer.material = newMat;
        }
        else
            _renderer.material = new(material);
    }

    protected virtual void UpdateConstructionProgressAlpha()
    {
        for (int i = 0; i < meshRenderers.Count; i++)
        {
            meshRenderers[i].material.SetFloat("_Progress", 0.1f + (constructionProgress / maximalProgress) * 0.9f);
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
        return inventory.Diff(localRes.Future(), cost);
    }
    /// <summary>Short info for building buttons.</summary>
    public virtual List<string> GetInfoText()
    {
        return new() { $"<u>Costs</u>:\n{cost}" };
    }

    /// <summary>Checks if you can afford the building.</summary>
    public virtual bool CanPlace(bool checkCost = true)
    {
        bool canPlace = (checkCost ? MyRes.CanAfford(cost) : true) && MyGrid.CanPlace(this);
        
        if(this is IFluidWork fluidWork)
        {
            fluidWork.AttachedPipes.ForEach(q => q.FindConnections(canPlace));
        }
        return canPlace;
    }

    /// <summary>
    /// Sets the layer, updates global resources and creates a construction job.
    /// </summary>
    public virtual void PlaceBuilding()
    {
        gameObject.layer = 6;
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.layer = 6;
        GetComponent<SortingGroup>().sortingLayerName = "Buildings";

        SceneRefs.GridTiles.HighLight(new(), gameObject);
        MyRes.PayCostGlobal(cost);
        SceneRefs.JobQueue.AddJob(JobState.Constructing, this); // creates a new job with the data above
        UniqueID();
        MyGrid.SetBuilding(this);

        if(this is IFluidWork fluidWork)
        {
            fluidWork.PlacePipes();
        }
    }

    public virtual void InitModifiers()
    {
        cost.Init();
        ((IModifiable)LocalRes.capacity).Init();

        #region Interface modifiers
        if (this is IAssign assign)
        {
            ((IModifiable)assign.AssignLimit).Init();
        }

        if(this is IProduction prod)
        {
            prod.ProdSpeed = new(1);
            if (this is IResourceProduction resProd)
            {
                resProd.InputResource.capacity = new(-1);
                resProd.ResourceCost.Init();
                resProd.ResourceYield.Init();
            }
        }
        #endregion
    }


    /// <summary>
    /// Fills <see cref="myColor"/>.
    /// </summary>
    public virtual void GetRenderComponents()
    {
        meshRenderers = transform.GetComponentsInChildren<Renderer>().ToList();
    } // saves the original color


    public virtual int CalculateMaxProgress()
    {
        int result = cost.Sum() * 2;
        if (result == 0)
            result = 1;
        return result;
    }

    #endregion

    #region Editor
#if UNITY_EDITOR
    public void Clone(Building prev)
    {
        objectName = prev.objectName;
        blueprint = prev.blueprint;
        cost = prev.cost;
    }
#endif
    #endregion

}