using Assets.Scripts.Editor.Buildings.LevelList;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Rendering;

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
    public const int MAX_LEVEL = 3;
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
    [SerializeField, LevelData] protected List<MoneyResource> costs = new();
    public List<MoneyResource> Costs => costs;
    [CreateProperty] public MoneyResource Cost => costs[level];

    public bool IsWorking => !constructing && !Upgrading && !Deconstructing;

    /// <inheritdoc cref="constructing"/>
    public bool Constructing { get => constructing; set => constructing = value; }
    /// <summary>Is being constructed.</summary>
    [SerializeField] protected bool constructing;


    /// <summary>Is currently being upgraded(from level x => x + 1).</summary>
    public bool Upgrading { get; set; }


    /// <summary>Is being deconstructed.</summary>
    public bool Deconstructing { get; set; }

    /// <summary>Progress of construction/deconstruction.</summary>
    [CreateProperty] public float constructionProgress;
    /// <summary>.</summary>
    public int maximalProgress;

    /// <summary>Starts at 0(to avoid index offset for level Lists)</summary>
    public int level = 0;
    
    /// <summary>The number of levels for this building (minimum is 1, base)</summary>
    [Range(1, MAX_LEVEL)] 
    public int maxLevel = 1;

    [Header("Prefab info"), SerializeField] 
    DataAssign prefabConnection;
    public DataAssign PrefabConnection { get => prefabConnection; set => prefabConnection = value; }

    bool isTransparent = false;
    #endregion

    #region Basic Operations

    /// <summary>Creates a new unique id not present in <see cref="MyGrid.buildings"/></summary>
    public override void UniqueID() => CreateNewId(MyGrid.Buildings.Select(q => q.id).ToList());
    /// <summary>
    /// Calculates anchor position (<see cref="blueprint"/>).
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override GridPos GetPos()
    {
        GridPos pos = blueprint.moveBy.Rotate(transform.rotation.eulerAngles.y);
        return new(
            transform.position.x - pos.x,
            (transform.position.y - 1) / 2,
            transform.position.z - pos.z);
    }

    public virtual bool IsInside(GridPos pos)
    {
        GridPos buildAnchor = GetPos();
        pos = pos - buildAnchor;
        for (int i = 0; i < blueprint.itemList.Count; i++)
        {
            NeededGridItem item = blueprint.itemList[i];
            if (item.itemType == GridItemType.Road || item.itemType == GridItemType.Anchor)
            {
                GridPos tileOffset = item.pos.Rotate(transform.rotation.eulerAngles.y);
                if (tileOffset.x == pos.x && tileOffset.z == pos.z)
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Window
    /// <summary>
    /// <inheritdoc/>
    /// Also toggle contsructed view, and other child elements.
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public sealed override InfoWindow OpenWindow()
    {
        // DEBUG_Binding Common init
        // Opens the info window (if selected) and switches what is displayed.
        InfoWindow info = base.OpenWindow();
        if (info)
        {
            info.Open(this, InfoMode.Building);
            if (IsWorking)
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
        info.CreateTabbedView(toEnable, this);
    }


    #endregion

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new BuildingSave();
        BuildingSave save = (clickable as BuildingSave);
        save.Name = Name;
        save.rotationY = transform.rotation.eulerAngles.y;

        save.blueprint = blueprint;
        save.constructed = Constructing;
        save.deconstructing = Deconstructing;
        save.upgrading = Upgrading;
        save.constructionProgress = constructionProgress;
        save.level = level;


        save.prefabConnection = prefabConnection;

        return base.Save(save);
    }

    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        Name = (save as BuildingSave).Name;
        blueprint = (save as BuildingSave).blueprint;
        Constructing = (save as BuildingSave).constructed;
        Deconstructing = (save as BuildingSave).deconstructing;
        Upgrading = (save as BuildingSave).upgrading;
        constructionProgress = (save as BuildingSave).constructionProgress;
        maximalProgress = CalculateMaxProgress();
        level = (save as BuildingSave).level;
        
        localRes.Load((save as BuildingSave).resSave);
        GetRenderComponents();

        InitPrefabData();

        gameObject.layer = 6;
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.layer = 6;
        GetComponent<SortingGroup>().sortingLayerName = "Buildings";

        if (Constructing)
        {
            SceneRefs.JobQueue.AddJob(JobState.Constructing, this);
            ChangeRenderMode(true);
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
        StorageRequest request = localRes.GetRequestByHuman(human);

        MyRes.MoveRes(
            localRes,
            human.Inventory,
            request.resource,
            transferPerTick,
            Constructing);
        UIUpdate(nameof(LocalRes));
        if (request.resource.Sum() == 0)
        {
            if (Constructing && localRes.Same(Cost))
            {
                request.SetRequestToAction(JobState.Constructing);
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
        if (Constructing)
        {
            ChangeRenderMode(false);
            Constructing = false;
            if (this is not IStorage)
                localRes.Dump();

            SceneRefs.QuestController.BuildBuilding(this);
        }

        if (selected)
            OpenWindow();

        if (this is IFluidWork fluidWork)
            fluidWork.ConnectToNetwork();
    }
    #endregion

    #region Deconstruction
    /// <summary>Toogle deconstruction. <b>TODO: COLOR CHANGING</b></summary>
    public virtual void OrderDeconstruct()
    {
        JobQueue queue = SceneRefs.JobQueue;
        if (Constructing) // if not yet constructed
        {
            // if there are any resources deposited(change to build progress when implemented)
            if (constructionProgress > 0)
            {
                if (!Deconstructing)
                {
                    queue.CancelJob(JobState.Constructing, this);
                    queue.AddJob(JobState.Deconstructing, this);
                    localRes.ReassignCarriers(JobState.Deconstructing);
                }
                else
                {
                    queue.AddJob(JobState.Constructing, this);
                    queue.CancelJob(JobState.Deconstructing, this);
                    localRes.ReassignCarriers(JobState.Constructing);
                }
                Deconstructing = !Deconstructing;
            }
            else
            {
                queue.CancelJob(JobState.Constructing, this);
                foreach (StorageRequest request in localRes.Requests)
                {
                    Human carrier = request.carrier;
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
        else
        {
            // if there isn't a deconstruction order yet
            if (!Deconstructing)
            {
                queue.CancelJob(JobState.Constructing, this);
                queue.AddJob(JobState.Deconstructing, this);

                localRes.ReassignCarriers(JobState.Deconstructing);
            }
            else
            {
                queue.CancelJob(JobState.Deconstructing, this);
                if (constructionProgress == maximalProgress)
                {
                    Deconstructing = false;
                    OpenWindow();
                }
                else
                {
                    queue.AddJob(JobState.Constructing, this);
                    localRes.ReassignCarriers(JobState.Constructing);
                }
            }
            Deconstructing = !Deconstructing;
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
        r.Manage(localRes, true);
        if (Constructing)
        {
            Resource resource = Cost - localRes;
            MyRes.UpdateResource(resource, true);
        }
        else
        {
            r.Manage(Cost, true);
            for (int i = 0; i < r.ammounts.Count; i++)
            {
                r.ammounts[i] /= 2;
            }
        }
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
                if (i >= materials.Count)
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
        {
            Color highlight = _renderer.material.GetColor("_EmissionColor");
            Material mat = new(material);
            if(highlight != new Color())
            {
                mat.SetColor("_EmissionColor", highlight);
                mat.EnableKeyword("_Emmision");
            }
            _renderer.material = mat;
        }
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
        return inventory.Diff(localRes.Future(), Cost);
    }
    /// <summary>Short info for building buttons.</summary>
    public virtual List<string> GetInfoText()
    {
        return new() { $"<u>Costs</u>:\n{costs}" };
    }

    /// <summary>Checks if you can afford the building.</summary>
    public virtual bool CanPlace(bool checkCost = true)
    {
        bool canPlace = (checkCost ? MyRes.CanAfford(Cost) : true) && MyGrid.CanPlace(this);

        if (this is IFluidWork fluidWork)
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

        Highlight(new());
        MyRes.UpdateResource(Cost, false);
        MyRes.ManageMoneyGlobal(-Cost.Money);
        SceneRefs.JobQueue.AddJob(JobState.Constructing, this); // creates a new job with the data above
        UniqueID();
        MyGrid.SetBuilding(this);

        if (this is IFluidWork fluidWork)
        {
            fluidWork.PlacePipes();
        }
        foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
            collider.enabled = true;
        }
    }

    public virtual void InitPrefabData()
    {
        foreach (var cost in costs)
        {
            cost.Init();
        }
        ((IModifiable)LocalRes.capacity).Init();

        #region Interface modifiers
        if (this is IAssign assign)
        {
            ((IModifiable)assign.AssignLimit).Init();
        }

        if (this is IProduction prod)
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
        int result = Cost.Sum() * 2;
        if (result == 0)
            result = 1;
        return result;
    }

    #endregion

    #region Upgrade
    public bool CanUpgrade()
    {
        if (Constructing)
            return false;
        if (Upgrading)
            return false;
        if (level >= maxLevel-1)
            return false;
        if (!MyRes.CanAfford(costs[level+1]))
            return false;

        return true;
    }

    public virtual void StartUpgrade()
    {
        if (!CanUpgrade())
            return;
        level++;
        Upgrading = true;
        constructionProgress = 0;

        maximalProgress = CalculateMaxProgress();
        ChangeRenderMode(true);
        localRes.ReassignCarriers(JobState.Constructing);
    }
    #endregion

    #region Editor
#if UNITY_EDITOR
    public virtual void Clone(Building prev)
    {
        Name = prev.Name;
        blueprint = prev.blueprint;
        costs = prev.costs;
    }
#endif
    #endregion

}