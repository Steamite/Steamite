using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.Data;
using UnityEngine.UIElements;

public class ProductionBuilding : AssignBuilding
{
    [Header("Production")]
    public Production pRes = new();
    [Header("Time")]
    public ProductionTime pTime = new();
    [Header("States")]
    public List<Human> working = new();
    public ProductionStates pStates = new();

    #region Window
    protected override void SetupWindow(InfoWindow info, List<string> toEnable)
    {
        if(toEnable.Count == 0)
            toEnable.Add("Production");
        base.SetupWindow(info, toEnable);
    }

    protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
        UpdateProductionInfo(info);
    }
    #endregion

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new ProductionBSave();
        (clickable as ProductionBSave).inputRes = new(pRes.inputResource);
        (clickable as ProductionBSave).pTime = pTime;
        (clickable as ProductionBSave).pStates = pStates;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        pRes.inputResource = new((save as ProductionBSave).inputRes);
        pTime = (save as ProductionBSave).pTime;
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

    public override void Store(Human h, int transferPerTick)
    {
        if (build.constructed)
        {
            int index = pRes.inputResource.carriers.IndexOf(h);
            if (index == -1)
                print("");
            MyRes.MoveRes(pRes.inputResource.stored, h.inventory, pRes.inputResource.requests[index], transferPerTick);
            OpenWindow();
            if (MyRes.DiffRes(pRes.productionCost, pRes.inputResource.stored).ammount.Sum() == 0)
            {
                h.transform.parent.parent.GetComponent<JobQueue>().CancelJob(JobState.Supply, this);
                pStates.supplied = true;
                RefreshStatus();
            }
            if (pRes.inputResource.requests[index].ammount.Sum() == 0)
            {
                pRes.inputResource.RemoveRequest(h);

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
                    h.jData.job = JobState.Constructing;
                    h.ChangeAction(HumanActions.Build);
                    return;
                }
                localRes.RemoveRequest(h);
                HumanActions.LookForNew(h);
                return;
            }
            MyRes.MoveRes(localRes.stored, h.inventory, localRes.requests[index], transferPerTick);
        }
        OpenWindow();
    }

    public override void RequestRes(Resource request, Human human, int mod)
    {
        StorageResource storage = null;
        if (build.constructed && mod == 1)
            storage = pRes.inputResource;
        else
            storage = localRes;
        storage.AddRequest(request, human, mod);
    }
    public override void TryLink(Human h)
    {
        pRes.inputResource.LinkHuman(h);
        base.TryLink(h);
    }

    public override void FinishBuild()
    {
        base.FinishBuild();
        if (pRes.productionCost.ammount.Sum() == 0)
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
                pRes.inputResource.ReassignCarriers(false);
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
                    human.jData = PathFinder.FindPath(new() {this}, human);
                    human.jData.job = JobState.Deconstructing;
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
        if(pRes.inputResource.stored.ammount.Sum() > 0){
            if (!c)
            {
                c = SceneRefs.objectFactory.CreateAChunk(instantPos, pRes.inputResource.stored);
            }
            else
                MyRes.ManageRes(c.localRes.stored, pRes.inputResource.stored, 1);
        }
        return c;
    }
    public override void DestoyBuilding()
    {
        base.DestoyBuilding();
        //foreach(Human carrier in )
    }

    public override Resource GetDiff(Resource r)
    {
        if (!build.constructed)
        {
            return base.GetDiff(r);
        }
        else
        {
            Resource x = new();
            MyRes.ManageRes(x, pRes.productionCost, 2);
            return MyRes.DiffRes(x, pRes.inputResource.Future(), r);
        }
    }
    public override List<string> GetInfoText()
    {
        List<string> strings = base.GetInfoText();
        strings[0] = $"Can employ up to {limit} workers";
        strings.Insert(1, $"<u>Produces</u>: \n{MyRes.GetDisplayText(pRes.production)}");
        if (pRes.productionCost.ammount.Sum() > 0)
            strings[1] += $", from: \n{MyRes.GetDisplayText(pRes.productionCost)}";
        return strings;
    }

    ///////////////////////////////////////////////////
    ////////////////////Methods////////////////////////
    ///////////////////////////////////////////////////
    protected virtual bool ManageInputRes()
    {
        if (pStates.supply && !pStates.supplied)
        {
            Resource r = MyRes.DiffRes(pRes.productionCost, pRes.inputResource.stored);

            if (r.ammount.Sum() == 0)
            {
                MyRes.ManageRes(pRes.inputResource.stored, pRes.productionCost, -1);
                MyRes.UpdateResource(pRes.productionCost, -1);
                pTime.currentTime = 0.1f;
                pStates.supplied = true;
                pStates.running = true;
                OpenWindow();
                return true;
            }
            else if (pRes.inputResource.carriers.Count == 0)
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
                pTime.currentTime += pTime.modifier * speed;
                OpenWindow();
                if(pTime.currentTime >= pTime.prodTime && pStates.space)
                {
                    Product();
                    AfterProduction();
                }
            }
        }
    }
    protected virtual void Product()
    {
        while (pTime.currentTime >= pTime.prodTime && (localRes.stored.capacity == -1 || localRes.stored.ammount.Sum() < localRes.stored.capacity))
        {
            pTime.currentTime -= pTime.prodTime;
            MyRes.ManageRes(localRes.stored, pRes.production, 1);
            MyRes.UpdateResource(pRes.production, 1);
            OpenWindow();
            pStates.supplied = false;
            if (!ManageInputRes())
                return;
        }
    }
    protected virtual void AfterProduction()
    {
        RequestPickup();
    }

    public virtual void RefreshStatus()
    {
        transform.GetChild(0).GetChild(0).gameObject.SetActive(pStates.stoped);
        transform.GetChild(0).GetChild(1).gameObject.SetActive(!pStates.supplied);
        transform.GetChild(0).GetChild(2).gameObject.SetActive(!pStates.space);
    }
    protected virtual void UpdateProductionInfo(InfoWindow info)
    {

        /*// production button
        t.GetChild(0).GetComponent<ProductionButton>().UpdateButtonState(pTime.currentTime, pTime.prodTime);

        // production cost
        t.GetChild(1).GetComponent<TMP_Text>()
                .text = MyRes.GetDisplayText(pRes.inputResource.stored, pRes.productionCost);

        // production
        t.GetChild(2).GetComponent<TMP_Text>()
            .text = MyRes.GetDisplayText(pRes.production);

        // stored
        t.GetChild(3).GetComponent<TMP_Text>()
        .text = MyRes.GetDisplayText(localRes.stored);*/
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


}