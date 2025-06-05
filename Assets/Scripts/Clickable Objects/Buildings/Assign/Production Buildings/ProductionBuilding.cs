using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;

/// <summary>Adds Production Handling.</summary>
public class ProductionBuilding : Building, IAssign, IResourceProduction
{
    #region Variables
    [SerializeField] int assignLimit;
    [SerializeField][Header("Production")] float productionTime;
    [SerializeField] float productionModifier = 1;

    [SerializeField] ModifiableResource productionCost = new();
    [SerializeField] ModifiableResource productionYield = new();
    #endregion

    #region Properties

    #region Time
    [CreateProperty] public float CurrentTime { get; set; } = 0;
    [CreateProperty] public bool Stoped { get; set; } = false;
    public float ProdTime { get => productionTime; set => productionTime = value; }
    public float Modifier { get => productionModifier; set => productionModifier = value; }
    #endregion

    #region Assign
    [CreateProperty] public List<Human> Assigned { get; set; } = new();

    [CreateProperty] public int AssignLimit { get => assignLimit; set => assignLimit = value; }
    #endregion

    #region Resources
    [CreateProperty] public ProductionStates ProdStates { get; set; } = new();
    [CreateProperty] public StorageResource LocalResource { get => LocalRes; }
    [CreateProperty] public StorageResource InputResource { get; set; } = new();
    public ModifiableResource ProductionCost { get => productionCost; }
    public ModifiableResource ProductionYield { get => productionYield; }
    #endregion

    #endregion

    #region Window
    /// <summary>
    /// If there's no other component in <see href="toEnable"/> adds "Production", and fills it.
    /// </summary>
    /// <inheritdoc/>
    protected override void ToggleInfoComponents(InfoWindow info, List<string> toEnable)
    {
        if (toEnable.Count == 0)
        {
            toEnable.Add("Production");
            toEnable.Add("Assign");
            base.ToggleInfoComponents(info, toEnable);
        }
        else
        {
            base.ToggleInfoComponents(info, toEnable);
        }
    }
    #endregion

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new ResProductionBSave();
        (clickable as ResProductionBSave).inputRes = new(InputResource);
        (clickable as ProductionBSave).prodTime = ProdTime;
        (clickable as ProductionBSave).currentTime = CurrentTime;
        (clickable as ProductionBSave).modifier = Modifier;
        (clickable as ProductionBSave).ProdStates = ProdStates;
        return base.Save(clickable);
    }
    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        InputResource = new((save as ResProductionBSave).inputRes);

        ProdTime = (save as ProductionBSave).prodTime;
        CurrentTime = (save as ProductionBSave).currentTime;
        Modifier = (save as ProductionBSave).modifier;
        ProdStates = (save as ProductionBSave).ProdStates;
        if (!ProdStates.supplied)
        {
            SceneRefs.jobQueue.AddJob(JobState.Supply, this);
        }
        if (constructed && GetDiff(new()).ammount.Sum() > 0)
        {
            SceneRefs.jobQueue.AddJob(JobState.Pickup, this);
        }
        ((IResourceProduction)this).Init(constructed);
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
        if (ProductionCost.ammount.Sum() == 0)
        {
            ProdStates.supplied = true;
            return;
        }
        ((IResourceProduction)this).RefreshStatus();
        ((IResourceProduction)this).RequestRestock();
    }

    /// <inheritdoc/>
    public override void OrderDeconstruct()
    {
        JobQueue queue = SceneRefs.jobQueue;
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
                else if(InputResource.carriers.Count > 0)
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
        SceneRefs.jobQueue.CancelJob(JobState.Supply, this);
        Chunk c = base.Deconstruct(instantPos);
        if (InputResource.stored.ammount.Sum() > 0)
        {
            if (!c)
            {
                c = SceneRefs.objectFactory.CreateAChunk(instantPos, InputResource.stored, true);
            }
            else
                MyRes.ManageRes(c.LocalRes.stored, InputResource.stored, 1);
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
            Resource x = new();
            MyRes.ManageRes(x, ProductionCost, 2);
            return MyRes.DiffRes(x, InputResource.Future(), r);
        }
    }

    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can employ up to {AssignLimit} workers";
        strings.Insert(1, $"<u>Produces</u>: \n{ProductionYield.GetDisplayText()}");
        if (ProductionCost.ammount.Sum() > 0)
            strings[1] += $", from: \n{ProductionCost.GetDisplayText()}";
        return strings;
    }
    #endregion

    #region Assign
    public bool ManageAssigned(Human human, bool add)
    {
        if (add)
        {
            if (Assigned.Count == assignLimit)
                return false;
            JobData job = PathFinder.FindPath(
                new List<ClickableObject>() { this },
                human);
            if (job.interest)
            {
                Assigned.Add(human);
                human.transform.SetParent(SceneRefs.humans.transform.GetChild(1).transform);
                human.workplace = this;
                job.job = JobState.FullTime;

                SceneRefs.jobQueue.FreeHuman(human);
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
            human.transform.SetParent(SceneRefs.humans.transform.GetChild(0).transform);
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
        return SceneRefs.humans.GetPartTime();
    }
    #endregion
}