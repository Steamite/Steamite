using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using Unity.Properties;

/// <summary>Adds Production Handling.</summary>
public class ProductionBuilding : AssignBuilding
{
    #region Variables
    /// <summary>The Storage for paying the production cost.</summary>
    StorageResource inputResource = new();

    /// <summary>Cost of one production cycle.</summary>
    [Header("Production")] public Resource productionCost = new();
    /// <summary>Production cycle yeild.</summary>
    public Resource production = new();

    /// <summary>Time it takes to finish one production cycle.</summary>
    [Header("Time")] public float prodTime = 20;
    /// <summary>Current progress.</summary>
    protected float currentTime = 0;
    /// <summary>Multiplies the weight of progress additions.</summary>
    public int modifier = 1;

    /// <summary>List of currently pressent and working <see cref="Human"/>s.</summary>
    [Header("States")] public List<Human> working = new();
    /// <summary>Holds data about production status.</summary>
    public ProductionStates pStates = new();
    #endregion

    #region Properties
    /// <inheritdoc cref="inputResource"/>
    [CreateProperty] public StorageResource InputResource
    {
        get => inputResource;
        set
        {
            inputResource = value;
            UIUpdate(nameof(InputResource));
        }
    }

    /// <inheritdoc cref="currentTime"/>
    [CreateProperty] public float CurrentTime
    {
        get => currentTime;
        set
        {
            currentTime = value;
            UIUpdate(nameof(CurrentTime));
        }
    }
    #endregion

    #region Window
    /// <summary>
    /// If there's no other component in <see href="toEnable"/> adds "Production", and fills it.
    /// </summary>
    /// <inheritdoc/>
    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        if (toEnable.Count == 0)
        {
            toEnable.Add("Production");
            base.OpenWindowWithToggle(info, toEnable);
            ((IUIElement)info.constructedElement.Q<VisualElement>("Production")).Fill(this);
        }
        else
        {
            base.OpenWindowWithToggle(info, toEnable);
        }
    }
    #endregion

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new ProductionBSave();
        (clickable as ProductionBSave).inputRes = new(inputResource);
        (clickable as ProductionBSave).prodTime = prodTime;
        (clickable as ProductionBSave).currentTime = currentTime;
        (clickable as ProductionBSave).modifier = modifier;
        (clickable as ProductionBSave).pStates = pStates;
        return base.Save(clickable);
    }
    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        inputResource = new((save as ProductionBSave).inputRes);

        prodTime = (save as ProductionBSave).prodTime;
        currentTime = (save as ProductionBSave).currentTime;
        modifier = (save as ProductionBSave).modifier;
        pStates = (save as ProductionBSave).pStates;
        if (pStates.supply && pStates.supplied == false)
        {
            SceneRefs.jobQueue.AddJob(JobState.Supply, this);
        }
        if (constructed && GetDiff(new()).ammount.Sum() > 0)
        {
            SceneRefs.jobQueue.AddJob(JobState.Pickup, this);
        }
        RefreshStatus();
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
            int index = inputResource.carriers.IndexOf(human);
            if (index == -1)
                print("");
            MyRes.MoveRes(inputResource.stored, human.Inventory, inputResource.requests[index], transferPerTick);
            UIUpdate(nameof(InputResource));
            if (MyRes.DiffRes(productionCost, inputResource.stored).ammount.Sum() == 0)
            {
                human.transform.parent.parent.GetComponent<JobQueue>().CancelJob(JobState.Supply, this);
                pStates.supplied = true;
                RefreshStatus();
            }
            if (inputResource.requests[index].ammount.Sum() == 0)
            {
                inputResource.RemoveRequest(human);

                human.destination = null;
                HumanActions.LookForNew(human);
                return;
            }
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
            storage = inputResource;
        else
            storage = localRes;
        storage.AddRequest(request, human, mod);
    }

    /// <inheritdoc/>
    public override void TryLink(Human h)
    {
        inputResource.LinkHuman(h);
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
        if (productionCost.ammount.Sum() == 0)
        {
            pStates.supplied = true;
            pStates.supply = false;
            return;
        }
        RefreshStatus();
        RequestRestock();
    }

    /// <inheritdoc/>
    public override void OrderDeconstruct()
    {
        base.OrderDeconstruct();
        if (constructed)
        {
            if (deconstructing) // has just been ordered
            {
                inputResource.ReassignCarriers(false);
                Human human = localRes.carriers.Count > 0 && assigned.Count > 0 ? assigned[0] : null;
                foreach (Human h in assigned)
                {
                    h.workplace = null;
                    if(h != human)
                        HumanActions.LookForNew(h);
                    working.RemoveAll(q => h);
                    StopAllCoroutines();
                }
                if (human)
                {
                    JobData data = PathFinder.FindPath(new() {this}, human);
                    data.job = JobState.Deconstructing;
                    human.SetJob(data);
                    human.ChangeAction(HumanActions.Demolish);
                }
            }
            else // has just been canceled
            {
                foreach(Human h in assigned)
                {

                }
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
        if(inputResource.stored.ammount.Sum() > 0){
            if (!c)
            {
                c = SceneRefs.objectFactory.CreateAChunk(instantPos, inputResource.stored);
            }
            else
                MyRes.ManageRes(c.LocalRes.stored, inputResource.stored, 1);
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
            MyRes.ManageRes(x, productionCost, 2);
            return MyRes.DiffRes(x, inputResource.Future(), r);
        }
    }

    /// <inheritdoc/>
    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can employ up to {limit} workers";
        strings.Insert(1, $"<u>Produces</u>: \n{MyRes.GetDisplayText(production)}");
        if (productionCost.ammount.Sum() > 0)
            strings[1] += $", from: \n{MyRes.GetDisplayText(productionCost)}";
        return strings;
    }
    #endregion

    #region Production
    /// <summary>
    /// Adds <paramref name="progress"/> to <see cref="currentTime"/>.
    /// </summary>
    /// <param name="progress">Ammount to add.</param>
    public virtual void ProgressProduction(float progress)
    {
        if (!pStates.stoped)
        {
            if (!pStates.running && pStates.supply)
            {
                ManageInputRes();
            }
            else
            {
                CurrentTime += modifier * progress;
                if(currentTime >= prodTime && pStates.space)
                {
                    Product();
                    AfterProduction();
                }
            }
        }
    }

    /// <summary>Called when <see cref="currentTime"/> reaches <see cref="prodTime"/>.</summary>
    protected virtual void Product()
    {
        while (currentTime >= prodTime && (localRes.stored.capacity == -1 || localRes.stored.ammount.Sum() < localRes.stored.capacity))
        {
            currentTime -= prodTime;
            MyRes.ManageRes(localRes.stored, production, 1);
            UIUpdate(nameof(LocalRes));
            MyRes.UpdateResource(production, 1);
            pStates.supplied = false;
            if (!ManageInputRes())
                return;
        }
    }
    /// <summary>Place for custom implementations on child classes.</summary>
    protected virtual void AfterProduction()
    {
        RequestPickup();
    }

    /// <summary>
    /// Manual production toggle.
    /// </summary>
    /// <returns>New toggle state.</returns>
    public bool StopProduction()
    {
        pStates.stoped = !pStates.stoped;
        transform.GetChild(0).GetChild(0).gameObject.SetActive(pStates.stoped);
        return pStates.stoped;
    }
    /// <summary>Toggles state indicators.</summary>
    public virtual void RefreshStatus()
    {
        transform.GetChild(0).GetChild(0).gameObject.SetActive(pStates.stoped);
        transform.GetChild(0).GetChild(1).gameObject.SetActive(!pStates.supplied);
        transform.GetChild(0).GetChild(2).gameObject.SetActive(!pStates.space);
    }
    #endregion

    #region Production Resources
    /// <summary>
    /// Triggered before starting each production cycle. <br/>
    /// Consumes production cost or requests new resources.
    /// </summary>
    /// <returns>Returns production is running.</returns>
    protected virtual bool ManageInputRes()
    {
        if (pStates.supply && !pStates.supplied)
        {
            Resource r = MyRes.DiffRes(productionCost, inputResource.stored);

            if (r.ammount.Sum() == 0)
            {
                MyRes.ManageRes(inputResource.stored, productionCost, -1);
                UIUpdate(nameof(InputResource));
                MyRes.UpdateResource(productionCost, -1);
                currentTime = 0.1f;
                pStates.supplied = true;
                pStates.running = true;
                return true;
            }
            else if (inputResource.carriers.Count == 0)
            {
                RequestRestock();
                transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
                pStates.supplied = false;
            }
            return false;
        }
        pStates.running = true;
        return true;
    }

    /// <summary>Adds a <see cref="JobState.Supply"/> job order.</summary>
    public void RequestRestock()
    {
        JobQueue jQ = SceneRefs.jobQueue;
        if (!jQ.supplyNeeded.Contains(this) && pStates.supply)
            jQ.AddJob(JobState.Supply, this);
    }

    /// <summary>Adds a <see cref="JobState.Pickup"/> job order.</summary>
    public void RequestPickup()
    {
        JobQueue jQ = SceneRefs.jobQueue;
        if (!jQ.pickupNeeded.Contains(this))
        {
            jQ.AddJob(JobState.Pickup, this);
        }
    }

    #endregion

    #region Assign
    public override void ManageAssigned(Human human, bool add)
    {
        if (add)
        {
            JobData job = PathFinder.FindPath(
                new List<ClickableObject>() { this },
                human);
            if (job.interest)
            {
                assigned.Add(human);
                human.transform.SetParent(SceneRefs.humans.transform.GetChild(1).transform);
                human.workplace = this;
                job.job = JobState.FullTime;

                if (!human.nightTime)
                    human.SetJob(job);
                else
                    human.SetJob(JobState.FullTime, job.interest);
                human.ChangeAction(HumanActions.Move);
                human.lookingForAJob = false;
            }
            else
            {
                Debug.LogError("cant find way here");
                return;
            }
        }
        else
        {
            assigned.Remove(human);
            human.workplace = null;
            human.transform.SetParent(SceneRefs.humans.transform.GetChild(0).transform);
            human.SetJob(JobState.Free);
            human.Idle();
        }
        UIUpdate(nameof(Assigned));
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override List<Human> GetUnassigned()
    {
        return SceneRefs.humans.GetPartTime();
    }
    #endregion
}