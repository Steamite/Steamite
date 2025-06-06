using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public enum BuildingCategType
{
    Population,
    Production,
    Research
}

/// <summary>
/// Class for buildings, which can be constructed on free tiles. 
/// Each building needs atleast one free access point.
/// </summary>
public class Building : StorageObject
{
    #region Variables
    /// <summary>Used for remembering color.</summary>
    [SerializeField] protected List<Color> _materialColors;
    [SerializeField] protected List<Renderer> _meshRenderers;

    /// <summary>
    /// Mask saying which categories this building belongs to.
    /// </summary>
    [SerializeField] int buildingCategories;
    /// <inheritdoc cref="buildingCategories"/>
    public int BuildingCateg { get => buildingCategories; set => buildingCategories = value; }

    /// <summary>Building layout(entry points, anchor, ...).</summary>
    public BuildingGrid blueprint;
    /// <summary>Construction cost in resources.</summary>
    [SerializeField] public ModifiableResource cost = new();
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
    #endregion

    #region Basic Operations
    /// <summary>Fills<see cref="myColor"/>.</summary>
    protected virtual void Awake() => GetColors();

    /// <summary>Creates a list from <see cref="MyGrid.buildings"/></summary>
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
    /// Opens and fills needed components in Building Visual Element.
    /// </summary>
    /// <param name="info"><see cref="InfoWindow"/> supplied from <see cref="OpenWindow"/>.</param>
    /// <param name="toEnable">List of components to enable in the Visual Element.</param>
    protected virtual void ToggleInfoComponents(InfoWindow info, List<string> toEnable)
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
        BSave save = (clickable as BSave);
        save.prefabName = objectName;
        save.rotationY = transform.rotation.eulerAngles.y;

        save.blueprint = blueprint;
        save.constructed = constructed;
        save.deconstructing = deconstructing;
        save.constructionProgress = constructionProgress;
        save.maximalProgress = maximalProgress;
        save.categoryID = categoryID;
        save.wrapperID = wrapperID;

        return base.Save(save);
    }
    
    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        objectName = (save as BSave).prefabName;
        blueprint = (save as BSave).blueprint;
        cost.Init();
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
            PlaceBuilding(true);
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
        // DEBUG_Binding Problem entrypoint
        // This happens when a building construction is finished.
        // If the building is selected(and InfoWindow is open), the information should refresh and show a different view.
        // The view is switched however, the bindings do not work.
        if (!constructed && this is not IStorage)
        {
            localRes.stored = new();
        }
        constructed = true;
        ChangeRenderMode(false);
        if(selected)
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
                Human human = null;
                queue.CancelJob(JobState.Constructing, this);
                queue.AddJob(JobState.Deconstructing, this);

                human = localRes.ReassignCarriers();
            }
            else
            {
                queue.CancelJob(JobState.Deconstructing, this);
                queue.AddJob(JobState.Constructing, this);
                if (localRes.carriers.Count > 0)
                {
                    localRes.carriers[0].SetJob(JobState.Constructing);
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
                    if(carrier.Job.interest != null && carrier.Job.interest != this)
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
    /// Creates a <see cref="Chunk"/> containing half of construction cost and <see cref="localRes.stored"/>.
    /// </summary>
    /// <param name="instantPos">Where to create the <see cref="Chunk"/>.</param>
    /// <returns>Created <see cref="Chunk"/>.</returns>
    public virtual Chunk Deconstruct(GridPos instantPos)
    {
        Resource r = new();
        MyRes.ManageRes(r, cost, 1);
        if (constructed)
        {
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
    #endregion

    #region Material change
    /// <summary>
    /// Changes building materials to transparent or opague.
    /// </summary>
    /// <param name="transparent">Requested render mode.</param>
    public virtual void ChangeRenderMode(bool transparent)
    {
        Material newMat = transparent 
            ? MaterialChanger.Transparent 
            : MaterialChanger.Opaque;
        for (int i = 0; i < _meshRenderers.Count; i++)
        {
            Color c = new(-1,-1,-1);
            if (selected || SceneRefs.gridTiles.activeObject == this) { 
                c = _meshRenderers[i].material.GetColor("_EmissionColor");
                
                _meshRenderers[i].material = newMat;
                _meshRenderers[i].material.EnableKeyword("_EMISSION");
                _meshRenderers[i].material.SetColor("_EmissionColor", c);
            }
            else
                _meshRenderers[i].material = newMat;

            if (transparent)
                _meshRenderers[i].material.color
                    = new(_materialColors[i].r, _materialColors[i].g, _materialColors[i].b, 0.1f + (constructionProgress / maximalProgress) * 0.9f);
            else
                _meshRenderers[i].material.color = _materialColors[i];
        }
    }

    public void UpdateConstructionProgressAlpha()
    {
        for (int i = 0; i < _meshRenderers.Count; i++)
        {
            _meshRenderers[i].material.color 
                = new(_materialColors[i].r, _materialColors[i].g, _materialColors[i].b, 0.1f + (constructionProgress / maximalProgress)*0.9f);
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
    /// Entry point to call for creating building that are stationary.
    /// Do not call for moving blueprints.
    /// </summary>
    public virtual void PlaceBuilding(bool loading = false)
    {
        foreach (Transform t in transform.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = 6;
        }
        GetComponent<SortingGroup>().sortingLayerName = "Buildings";
        SceneRefs.gridTiles.HighLight(new(), gameObject);

        MyRes.UpdateResource(cost, -1);
        if (loading)
        {
            maximalProgress = CalculateMaxProgress();
            ChangeRenderMode(true);
        }
        else
        {
            SceneRefs.jobQueue.AddJob(JobState.Constructing, this); // creates a new job with the data above
            UniqueID();
            MyGrid.SetBuilding(this, loading);
        }

    }

    /// <summary>
    /// Fills <see cref="myColor"/>.
    /// </summary>
    public void GetColors()
    {
        _meshRenderers = transform.GetComponentsInChildren<Renderer>().ToList();
        _materialColors = _meshRenderers.Select(q => q.material.color).ToList();

    } // saves the original color


    public virtual int CalculateMaxProgress()
    {
        int result = cost.ammount.Sum() * 2;
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