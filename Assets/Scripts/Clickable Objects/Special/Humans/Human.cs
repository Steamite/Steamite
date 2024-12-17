using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public enum JobState
{
    Unset,
    Free,
    Digging, // dig out rock
    Constructing, // progress constructions
    Deconstructing, // progress deconstruction
    Pickup, // taking resources to a storage
    Supply, // taking resources from a storage (elevator, chunk) to a building
    FullTime, // progress production
    Cleanup
}
public enum Specializations
{
    Worker,
    Farmer,
    Miner
}
public class Human : ClickableObject
{

    [Header("Human stats")]
    
    Efficiency efficiency = new();
    public Efficiency Efficiency
    {
        get { return efficiency; }
        set { efficiency = value; }
    }

    public Specializations specialization = Specializations.Worker;

    public bool nightTime = false;
    public bool hasEaten = true;
    public bool lookingForAJob = false;

    // job data
    public House home;

    // Action data
    [Header("Action data")]

    JobData jData;

    #region Job Setters
    [CreateProperty]
    public JobData Job
    {
        get { return jData; }
    }
    public void SetJob(JobData data)
    {
        jData = data;
        UpdateWindow(nameof(Job));
    }
    public void SetJob(JobState state = JobState.Unset, ClickableObject interest = null, List<GridPos> path = null)
    {
        if(state != JobState.Unset)
            jData.job = state;
        if (interest)
            jData.interest = interest;
        if (path != null)
            jData.path = path;
        UpdateWindow(nameof(Job));
    }
    #endregion
    //public 
    public Building destination;
    public ProductionBuilding workplace;
    public Action<Human> repetableAction;

    Resource inventory = new();
    [CreateProperty]
    public Resource Inventory => inventory;

    #region Basic Operations
    public override GridPos GetPos()
    {
        return new(transform.position.x, transform.localPosition.y / ClickabeObjectFactory.LEVEL_HEIGHT, transform.position.z);
    }
    public override void UniqueID() // creates a random int
    {
        List<int> ids = transform.parent.parent.GetChild(0).GetComponentsInChildren<Human>().Select(q => q.id).ToList();
        ids.AddRange(transform.parent.parent.GetChild(1).GetComponentsInChildren<Human>().Select(q => q.id).ToList());
        jData = new();
        jData.job = JobState.Free;
        CreateNewId(ids);
    }

    public void ActivateHuman()
    {
#if UNITY_EDITOR
        transform.GetChild(0).gameObject.SetActive(true);
#endif
        transform.parent.parent.GetComponent<Humans>().ticks.tickAction += DoRepetableAction;
        DayTime dT = SceneRefs.tick.timeController;
        dT.dayStart += Day;
        dT.nightStart += Night;

        if (jData.job <= JobState.Free)
        {
            HumanActions.LookForNew(this);
            return;
        }
        ChangeAction(HumanActions.Move);
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
    #endregion

    #region Window
    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Human);
        info.humanElement.Q<Label>("Specialization-Value").text = specialization.ToString();

        return info;
    }

    /*protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
        VisualElement group = info.human.Q<VisualElement>("Stats");
        
        group.Q<Label>("Efficiency-Value").text = efficiency.ToString();

        group = group.parent.Q<VisualElement>("Job");
        group.Q<Label>("Type-Value").text = jData.job.ToString();
        group.Q<Label>("Position-Value").text = jData.interest?.GetPos().ToString();
        group.Q<Label>("Object-Value").text = jData.interest?.name;
    }*/
    #endregion Window

    #region Saving
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

    #endregion

    #region Human actions
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
        repetableAction = action;
    }

    public void Decide()
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
        ClickableObject el = MyGrid.buildings.First(q => q.tag == "Elevator" && q.GetComponent<Elevator>().main);
        if (!el.GetPos().Equals(GetPos())) // if not standing on the elevator
        {
            JobData data = PathFinder.FindPath(new() { el }, this);
            data.job = JobState.Free;
            if (data.interest != null)
            {
                SetJob(data);
                ChangeAction(HumanActions.Move); //  go to the elevator and look for a new Job 
            }
        }
        else if (jData.job != JobState.Free)
            SetJob(JobState.Free);
    }
    #endregion

    #region Day cycle
    public void Day()
    {
        nightTime = false; // stop sleeping
        if (jData.job <= JobState.Free)
            HumanActions.LookForNew(this);
        else
        {

            SetJob(path: PathFinder.FindPath(new() { jData.interest }, this).path);
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
    #endregion
}