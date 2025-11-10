using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

/// <summary>Adds Production Handling.</summary>
public class ResourceProductionBuilding : Building, IAssign, IResourceProduction
{
    #region Variables
    [SerializeField] ModifiableInteger assignLimit;
    [SerializeField][Header("Production")] float productionTime;
    [SerializeField] ModifiableFloat prodSpeed;

    [SerializeField] ModifiableResource resourceCost = new();
    [SerializeField] ModifiableResource resourceYield = new();
    [SerializeField] List<DataAssign> recipesAssigmnet = new();

    #endregion

    #region Properties

    #region Time
    [CreateProperty] public float CurrentTime { get; set; } = 0;
    [CreateProperty] public bool Stoped { get; set; } = false;
    public float ProdTime { get => productionTime; set => productionTime = value; }
    [CreateProperty] public ModifiableFloat ProdSpeed { get => prodSpeed; set => prodSpeed = value; }
    #endregion

    #region Assign
    [CreateProperty] public List<Human> Assigned { get; set; } = new();

    [CreateProperty] public ModifiableInteger AssignLimit { get => assignLimit; set => assignLimit = value; }
    #endregion

    #region Resources
    [CreateProperty] public ProductionStates ProdStates { get; set; } = new();
    [CreateProperty] public StorageResource LocalResource { get => LocalRes; }
    [CreateProperty] public StorageResource InputResource { get; set; } = new();
    [CreateProperty] public ModifiableResource ResourceCost { get => resourceCost; set => resourceCost = value; }
    [CreateProperty] public ModifiableResource ResourceYield { get => resourceYield; set => resourceYield = value; }
    [CreateProperty] public List<ProductionRecipe> Recipes { get; set; }
    public List<DataAssign> RecipeAsssigment { get => recipesAssigmnet; set => recipesAssigmnet = value; }
    [CreateProperty] public int SelectedRecipe { get; set; }
    #endregion

    #endregion

    #region Window
    /// <summary>
    /// If there's no other component in <see href="toEnable"/> adds "Production", and fills it.
    /// </summary>
    /// <inheritdoc/>
    protected override void ToggleInfoComponents(InfoWindow info, Dictionary<string, List<string>> toEnable)
    {
        toEnable.TryAdd("General", new List<string> { "Production Info", "Assign Info" });
        base.ToggleInfoComponents(info, toEnable);
    }
    #endregion

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new ResProductionBSave();
        (clickable as ResProductionBSave).inputRes = new(InputResource);
        (clickable as ProductionBSave).currentTime = CurrentTime;
        (clickable as ProductionBSave).ProdStates = ProdStates;
        (clickable as ProductionBSave).selectedRecipe = SelectedRecipe;
        return base.Save(clickable);
    }

    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        InputResource.Load((save as ResProductionBSave).inputRes);

        CurrentTime = (save as ProductionBSave).currentTime;
        ProdStates = (save as ProductionBSave).ProdStates;
        SelectedRecipe = (save as ProductionBSave).selectedRecipe;
        base.Load(save);
    }
    #endregion

    #region Storage
    /// <summary>
    /// <inheritdoc/> <br/>
    /// If already constructed use stored resources for production(<see cref="inputResource"/>). 
    /// </summary>
    /// <param name="human"><inheritdoc/></param>
    /// <param name="transferPerTick"><inheritdoc/></param>
    public override void Store(Human human, int transferPerTick)
    {
        if (constructed)
        {
            ((IResourceProduction)this).StoreProdResources(human, transferPerTick);
        }
        else
        {
            base.Store(human, transferPerTick);
        }
    }
    public override void Take(Human h, int transferPerTick)
    {
        base.Take(h, transferPerTick);

        if (localRes.requests.Count == 0)
        {
            ProdStates.requestedPickup = false;
            ProdStates.space = ResourceYield.Sum() <= localRes.FreeSpace;
            SceneRefs.JobQueue.CancelJob(JobState.Pickup, this);
        }
    }

    /// <inheritdoc/>
    public override void RequestRes(Resource request, Human human, int mod)
    {
        StorageResource storage = null;
        if (constructed && mod == 1)
            storage = InputResource;
        else
            storage = localRes;
        storage.AddRequest(request, human, mod);
    }

    /// <inheritdoc/>
    public override void TryLink(Human h)
    {
        InputResource.LinkHuman(h);
        base.TryLink(h);
    }
    #endregion

    #region Construction & Deconstruction

    /// <summary>
    /// <inheritdoc/>
    /// And requests resources for production.
    /// </summary>
    public override void FinishBuild()
    {
        base.FinishBuild();
        if (ResourceCost.Sum() == 0)
        {
            ProdStates.needsResources = false;
            return;
        }
        ((IResourceProduction)this).Init(constructed, SceneRefs.ObjectFactory.recipeData);
    }


    /// <inheritdoc/>
    public override void OrderDeconstruct()
    {
        JobQueue queue = SceneRefs.JobQueue;
        if (!constructed)
        {
            base.OrderDeconstruct();
        }
        else
        {
            if (!deconstructing) // start deconstruction now!
            {
                Human human = null;
                queue.CancelJob(JobState.Pickup, this);
                queue.CancelJob(JobState.Supply, this);
                queue.CancelJob(JobState.Constructing, this);
                queue.AddJob(JobState.Deconstructing, this);
                deconstructing = true;

                human = localRes.ReassignCarriers();
                if (human)
                    InputResource.ReassignCarriers(false);
                else if (InputResource.carriers.Count > 0)
                {
                    human = InputResource.carriers[0];
                    localRes.AddRequest(new(), human, 0);
                    InputResource.RemoveRequest(human);
                    InputResource.ReassignCarriers(false);
                    JobData data = PathFinder.FindPath(new() { this }, human);
                    data.job = JobState.Deconstructing;
                    human.SetJob(data, true);
                }
            }
            else // has just been canceled
            {
                base.OrderDeconstruct();
                return;
            }
        }
    }

    /// <summary>
    /// <inheritdoc/> <br/>
    /// And full <see cref="inputResource"/>.
    /// </summary>
    /// <param name="instantPos"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
    public override Chunk Deconstruct(GridPos instantPos)
    {
        SceneRefs.JobQueue.CancelJob(JobState.Supply, this);
        Chunk c = base.Deconstruct(instantPos);
        if (InputResource.Sum() > 0)
        {
            if (!c)
            {
                c = SceneRefs.ObjectFactory.CreateChunk(instantPos, InputResource, true);
            }
            else
                c.LocalRes.Manage(InputResource, true);
        }
        return c;
    }

    #endregion

    #region Placing
    /// <summary>
    /// <inheritdoc/> <br/>
    /// If constructed returns missing resources for production.
    /// </summary>
    /// <param name="r"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
    public override Resource GetDiff(Resource r)
    {
        if (!constructed)
        {
            return base.GetDiff(r);
        }
        else
        {
            Resource cost = new();
            cost.Manage(ResourceCost, true, 2);
            return r.Diff(InputResource.Future(), cost);
        }
    }

    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can employ up to {AssignLimit} workers";
        strings.Insert(1, $"<u>Produces</u>: \n{ResourceYield}");
        if (ResourceCost.Sum() > 0)
            strings[1] += $", from: \n{ResourceCost}";
        return strings;
    }
    #endregion

    #region Assign
    public bool ManageAssigned(Human human, bool add)
    {
        if (add)
        {
            if (Assigned.Count == assignLimit.currentValue)
                return false;
            JobData job = PathFinder.FindPath(
                new List<ClickableObject>() { this },
                human);
            if (job.interest)
            {
                Assigned.Add(human);
                human.transform.SetParent(SceneRefs.Humans.transform.GetChild(1).transform);
                human.workplace = this;
                job.job = JobState.FullTime;

                SceneRefs.JobQueue.FreeHuman(human);
                if (!human.nightTime)
                    human.SetJob(job);
                else
                    human.SetJob(JobState.FullTime, job.interest);
                human.Decide();
                human.lookingForAJob = false;

            }
            else
            {
                Debug.LogError("cant find way here");
                return false;
            }
        }
        else
        {
            Assigned.Remove(human);
            human.workplace = null;
            human.transform.SetParent(SceneRefs.Humans.transform.GetChild(0).transform);
            human.SetJob(JobState.Free);
            human.Idle();
        }
        UIUpdate(nameof(Assigned));
        return true;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public List<Human> GetUnassigned()
    {
        return SceneRefs.Humans.GetPartTime();
    }
    #endregion

}