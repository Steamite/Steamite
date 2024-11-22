using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum JobState
{
    Free,
    Digging, // dig out rock
    Constructing, // progress constructions
    Deconstructing, // progress deconstruction
    Pickup, // taking resources to a storage
    Supply, // taking resources from a storage (elevator, chunk) to a building
    FullTime, // progress production
    Cleanup
}
public enum Specs
{
    Worker,
    Farmer,
    Miner
}
public class Human : ClickableObject
{
    [Header("Human stats")]
    [SerializeField]
    public Efficiency efficiency = new();
    public Specs specialization = Specs.Worker;

    public bool nightTime = false;
    public bool hasEaten = true;
    public bool lookingForAJob = false;

    // job data
    public House home;

    // Action data
    [Header("Action data")]
    public JobData jData = new();
    public Building destination;
    public ProductionBuilding workplace;
    public Action<Human> repetableAction;
    public Resource inventory = new();
    ///////////////////////////////////////////////////
    ///////////////////Overrides///////////////////////
    ///////////////////////////////////////////////////
    public override void UniqueID() // creates a random int
    {
        List<int> ids = transform.parent.parent.GetChild(0).GetComponentsInChildren<Human>().Select(q => q.id).ToList();
        ids.AddRange(transform.parent.parent.GetChild(1).GetComponentsInChildren<Human>().Select(q => q.id).ToList());
        CreateNewId(ids);
    }
    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info;
        // if selected
        if ((info = base.OpenWindow(setUp)) != null)
        {
            // set window mod to humans
            if (setUp)
            {
                info.SwitchMods(InfoMode.Human, name);
            }
            string s;
            // JOB INFO
            s =
                $"Job - {jData.job}\n" +
                $"Object - {(jData.interest ? jData.interest.name : "")}";
            info.clickObjectTransform.GetChild((int)InfoMode.Human).GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = s;
            // STATUS INFO
            s =
                $"Efficiency - {efficiency.efficiency}\n" +
                $"HasEaten - {hasEaten}\n" +
                $"Inventory - {MyRes.GetDisplayText(inventory)}";
            info.clickObjectTransform.GetChild((int)InfoMode.Human).GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = s;
        }
        // floating text update
        transform.GetChild(0).GetComponent<TMP_Text>().text = jData.job.ToString();
        return null;
    }
    public void OnDrawGizmos()
    {
        if (selected)
        {
            foreach (GridPos gp in jData.path)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(new(gp.x, (gp.y * 2) + 1, gp.z), 0.2f);
            }
        }
    }

    public override GridPos GetPos()
    {
        return new(transform.position.x, transform.localPosition.y/2, transform.position.z);
    }

    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new HumanSave();
        (clickable as HumanSave).name = name;
        (clickable as HumanSave).color = new(transform.GetChild(1).GetComponent<MeshRenderer>().material.color); // saves color of hat
        (clickable as HumanSave).gridPos = GetPos();
        (clickable as HumanSave).jobSave = new(jData);
        (clickable as HumanSave).jobSave.destinationID = destination ? destination.id : -1;
        (clickable as HumanSave).inventory = inventory;
        (clickable as HumanSave).hasEaten = hasEaten;
        (clickable as HumanSave).specs = specialization;
        (clickable as HumanSave).houseID = home ? home.id : -1;
        (clickable as HumanSave).workplaceId = workplace ? workplace.id : -1;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
    }
    ///////////////////////////////////////////////////
    ///////////////////Methods/////////////////////////
    ///////////////////////////////////////////////////
    public void ActivateHuman()
    {
#if UNITY_EDITOR
        transform.GetChild(0).gameObject.SetActive(true);
#endif
        transform.parent.parent.GetComponent<Humans>().ticks.tickAction += DoRepetableAction;
        DayTime dT = SceneRefs.tick.timeController;
        dT.dayStart += Day;
        dT.nightStart += Night;

        if (jData.job == JobState.Free)
        {
            HumanActions.LookForNew(this);
            return;
        }
        ChangeAction(HumanActions.Move);
    }

    public void DoRepetableAction()
    {
        if (repetableAction != null && !lookingForAJob)
        {
            repetableAction.Invoke(this);
        }
        else if(!nightTime)
        {
            HumanActions.LookForNew(this);
        }
    }

    public void ChangeAction(Action<Human> action)
    {
        transform.GetChild(0).GetComponent<TMP_Text>().text = jData.job.ToString();
        OpenWindow();
        repetableAction = action;
    }

    public void Decide() // for new Jobs or for managing fullTime jobs
    {
        if (nightTime)
            return;
        switch (jData.job)
        {
            case JobState.Free:
                HumanActions.LookForNew(this);
                break;
            case JobState.Digging:
                ChangeAction(HumanActions.Dig);
                break;
            case JobState.Constructing:
                ChangeAction(HumanActions.Build);
                break;
            case JobState.Deconstructing:
                ChangeAction(HumanActions.Demolish);
                break;
            case JobState.Pickup:
                ChangeAction(HumanActions.Take);
                break;
            case JobState.Supply:
                ChangeAction(HumanActions.Store);
                break;
            case JobState.FullTime:
                ChangeAction(HumanActions.DoProduction);
                break;
            default:
                HumanActions.LookForNew(this);
                Debug.LogError("Something went wrong, don't know what to do!!!");
                break;
        }
    }

    public void Idle()
    {
        jData = new JobData();
        Elevator el = MyGrid.buildings.First(q => q.tag == "Elevator" && q.GetComponent<Elevator>().main).GetComponent<Elevator>();
        GridPos v = new(el.transform.localPosition);
        if (!v.Equals(new GridPos(transform.localPosition))) // if not standing on the elevator
        {
            jData = PathFinder.FindPath(new() { el }, this);
            if (jData.interest != null)
                ChangeAction(HumanActions.Move); //  go to the elevator and look for a new Job 
        }
        else
        {
            OpenWindow();
        }
    }


    ///////////////////////////////////////////////////
    ///////////////////Day/Night///////////////////////
    ///////////////////////////////////////////////////
    public void Day()
    {
        nightTime = false; // stop sleeping
        if (jData.job == JobState.Free)
            HumanActions.LookForNew(this);
        else
        {
            jData.path = PathFinder.FindPath(new() { jData.interest }, this).path;
            ChangeAction(HumanActions.Move);
        }
    }

    public void Night()
    {
        if (!nightTime)
        {
            MyRes.EatFood(this);
            nightTime = true;
        }
        jData.path = PathFinder.FindWayHome(this);
        ChangeAction(HumanActions.Move);
    }

}