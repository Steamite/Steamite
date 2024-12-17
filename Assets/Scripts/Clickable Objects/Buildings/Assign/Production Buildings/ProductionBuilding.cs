using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using Unity.Properties;

public class ProductionBuilding : AssignBuilding
{
    StorageResource inputResource = new();
    [CreateProperty]
    public StorageResource InputResource
    {
        get => inputResource;
        set
        {
            inputResource = value;
            UpdateWindow(nameof(InputResource));
        }
    }
    [Header("Production")]
    public Resource productionCost = new();
    public Resource production = new();
    [Header("Time")]
    public float prodTime = 20;
    protected float currentTime = 0;
    [CreateProperty] public float CurrentTime
    {
        get => currentTime;
        set
        {
            currentTime = value;
            UpdateWindow(nameof(CurrentTime));
        }
    }
    public int modifier = 1;
    [Header("States")]
    public List<Human> working = new();
    public ProductionStates pStates = new();



    #region Window
    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        if (toEnable.Count == 0)
            toEnable.Add("Production");
        base.OpenWindowWithToggle(info, toEnable);
        ((IUIElement)info.constructedElement.Q<VisualElement>("Production")).Fill(this);
    }
    #endregion

    #region Saving
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
    public override void Load(ClickableObjectSave save)
    {
        inputResource = new((save as ProductionBSave).inputRes);

        prodTime = (save as ProductionBSave).prodTime;
        currentTime = (save as ProductionBSave).currentTime;
        modifier = (save as ProductionBSave).modifier;
        pStates = (save as ProductionBSave).pStates;
        if (pStates.supply && pStates.supplied == false)
        {
            SceneRefs.humans.GetComponent<JobQueue>().AddJob(JobState.Supply, this);
        }
        if (build.constructed && GetDiff(new()).ammount.Sum() > 0)
        {
            SceneRefs.humans.GetComponent<JobQueue>().AddJob(JobState.Pickup, this);
        }
        RefreshStatus();
        base.Load(save);
    }
    #endregion

    #region Storage
    public override void Store(Human h, int transferPerTick)
    {
        if (build.constructed)
        {
            int index = inputResource.carriers.IndexOf(h);
            if (index == -1)
                print("");
            MyRes.MoveRes(inputResource.stored, h.Inventory, inputResource.requests[index], transferPerTick);
            UpdateWindow(nameof(InputResource));
            if (MyRes.DiffRes(productionCost, inputResource.stored).ammount.Sum() == 0)
            {
                h.transform.parent.parent.GetComponent<JobQueue>().CancelJob(JobState.Supply, this);
                pStates.supplied = true;
                RefreshStatus();
            }
            if (inputResource.requests[index].ammount.Sum() == 0)
            {
                inputResource.RemoveRequest(h);

                h.destination = null;
                HumanActions.LookForNew(h);
                return;
            }
        }
        else
        {
            int index = localRes.carriers.IndexOf(h);
            if (localRes.requests[index].ammount.Sum() == 0)
            {
                if (localRes.stored.Equals(build.cost))
                {
                    localRes.requests[index].ammount = new();
                    localRes.requests[index].type = new();
                    h.SetJob(JobState.Constructing);
                    h.ChangeAction(HumanActions.Build);
                    return;
                }
                localRes.RemoveRequest(h);
                HumanActions.LookForNew(h);
                return;
            }
            MyRes.MoveRes(localRes.stored, h.Inventory, localRes.requests[index], transferPerTick);
        }
    }
    public override void RequestRes(Resource request, Human human, int mod)
    {
        StorageResource storage = null;
        if (build.constructed && mod == 1)
            storage = inputResource;
        else
            storage = localRes;
        storage.AddRequest(request, human, mod);
    }
    public override void TryLink(Human h)
    {
        inputResource.LinkHuman(h);
        base.TryLink(h);
    }
    #endregion

    #region Construction & Deconstruction
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

    public override void OrderDeconstruct()
    {
        base.OrderDeconstruct();
        if (build.constructed)
        {
            if (build.deconstructing) // has just been ordered
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
    public override Chunk Deconstruct(GridPos instantPos)
    {
        SceneRefs.humans.GetComponent<JobQueue>().CancelJob(JobState.Supply, this);
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
    public override void DestoyBuilding()
    {
        base.DestoyBuilding();
        //foreach(Human carrier in )
    }

    #endregion

    #region Placing
    public override Resource GetDiff(Resource r)
    {
        if (!build.constructed)
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
    public virtual void Produce(float speed)
    {
        if (!pStates.stoped)
        {
            if (!pStates.running && pStates.supply)
            {
                ManageInputRes();
            }
            else
            {
                CurrentTime += modifier * speed;
                if(currentTime >= prodTime && pStates.space)
                {
                    Product();
                    AfterProduction();
                }
            }
        }
    }
    protected virtual void Product()
    {
        while (currentTime >= prodTime && (localRes.stored.capacity == -1 || localRes.stored.ammount.Sum() < localRes.stored.capacity))
        {
            currentTime -= prodTime;
            MyRes.ManageRes(localRes.stored, production, 1);
            UpdateWindow(nameof(LocalRes));
            MyRes.UpdateResource(production, 1);
            pStates.supplied = false;
            if (!ManageInputRes())
                return;
        }
    }
    protected virtual void AfterProduction()
    {
        RequestPickup();
    }

    public bool StopProduction()
    {
        pStates.stoped = !pStates.stoped;
        transform.GetChild(0).GetChild(0).gameObject.SetActive(pStates.stoped);
        PauseProduction();
        return pStates.stoped;
    }
    public void PauseProduction()
    {
        if (pStates.stoped || !pStates.space || !pStates.supplied)
        {
            //  pStates.running = false;
        }
    }
    #endregion

    public virtual void RefreshStatus()
    {
        transform.GetChild(0).GetChild(0).gameObject.SetActive(pStates.stoped);
        transform.GetChild(0).GetChild(1).gameObject.SetActive(!pStates.supplied);
        transform.GetChild(0).GetChild(2).gameObject.SetActive(!pStates.space);
    }
    protected virtual void UpdateProductionInfo(InfoWindow info)
    {
        
        /*// production button
        t.GetChild(0).GetComponent<ProductionButton>().UpdateButtonState(currentTime, prodTime);

        // production cost
        t.GetChild(1).GetComponent<TMP_Text>()
                .text = MyRes.GetDisplayText(inputResource.stored, productionCost);

        // production
        t.GetChild(2).GetComponent<TMP_Text>()
            .text = MyRes.GetDisplayText(production);

        // stored
        t.GetChild(3).GetComponent<TMP_Text>()
        .text = MyRes.GetDisplayText(localRes.stored);*/
    }

    #region Production Resources
    protected virtual bool ManageInputRes()
    {
        if (pStates.supply && !pStates.supplied)
        {
            Resource r = MyRes.DiffRes(productionCost, inputResource.stored);

            if (r.ammount.Sum() == 0)
            {
                MyRes.ManageRes(inputResource.stored, productionCost, -1);
                UpdateWindow(nameof(InputResource));
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
                PauseProduction();
            }
            return false;
        }
        pStates.running = true;
        return true;
    }

    public void RequestRestock()
    {
        JobQueue jQ = SceneRefs.humans.GetComponent<JobQueue>();
        if (!jQ.supplyNeeded.Contains(this) && pStates.supply)
        {
            jQ.AddJob(JobState.Supply, this);
        }
    }

    public void RequestPickup()
    {
        JobQueue jQ = SceneRefs.humans.GetComponent<JobQueue>();
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
        UpdateWindow(nameof(Assigned));
    }
    public override List<Human> GetUnassigned()
    {
        return SceneRefs.humans.GetPartTime();
    }
    #endregion
}