using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

/// <summary>Categorizes all jobs.</summary>
public enum JobState
{
    /// <summary>Idle, will do temporary work if he finds his way.</summary>
    Free,
    /// <summary>Digging out a <see cref="Rock"/>.</summary>
    Digging,
    /// <summary>Constructing a <see cref="Building"/>.</summary>
    Constructing,
    /// <summary>Deconstructing a <see cref="Building"/>.</summary>
    Deconstructing,
    /// <summary>Getting resources for a construction or production.</summary>
    Pickup,
    /// <summary>Delivering found resources.</summary>
    Supply,
    /// <summary>Assigned in a <see cref="IAssign"/>.</summary>
    FullTime,
    /// <summary>Cleaning <see cref="Chunk"/>s.</summary>
    Cleanup
}

/// <summary>Specialization of the worker, provides bonuses in certain areas. (WIP)</summary>
public enum Specializations
{
    Worker,
    Farmer,
    Miner
}

/// <summary>Humans walk around the map and complete tasks ordered by the player.</summary>
public class Human : ClickableObject
{
    #region Variables
    /// <summary>Determines the rate of proggers when doing a job.</summary>
    [Header("Human stats")]
    Efficiency efficiency = new();
    /// <summary>Specialization of the worker, provides bonuses in certain areas. (WIP)</summary>
    public Specializations specialization = Specializations.Worker;
    /// <summary>If it's time to go to sleep.</summary>
    public bool nightTime = false;
    /// <summary>Fail-save to prevent looking for a job multiple times at once, Remove after testing.</summary>
    public bool lookingForAJob = false;
    /// <summary>Pointer to the assigned <see cref="House"/>.</summary>
    public House home;

    /// <summary>Holds data about the current assigment.</summary>
    [Header("Action data")]
    JobData jData = new();

    /// <summary>
    /// Used for delivery jobs, marks the destination.<br/> 
    /// Because <see cref="JobData.interest"/> has first the storage to take from.
    /// </summary>
    public Building destination;
    /// <summary>Marks the assigned workplace.</summary>
    public IAssign workplace;
    /// <summary>Action to execute next tick.</summary>
    Action<Human> repetableAction;
    /// <summary>Resources that are being carried.</summary>
    [SerializeField] CapacityResource inventory = new();

    #endregion

    #region Properties
    /// <inheritdoc cref="inventory"/>
    [CreateProperty] public CapacityResource Inventory { get => inventory; set => inventory = value; }


    /// <inheritdoc cref="efficiency"/>
    [CreateProperty] public float Efficiency => efficiency.efficiency;
    /// <summary>
    /// Updates efficiency modifiers and UI.
    /// </summary>
    /// <param name="_modType"><inheritdoc cref="Efficiency.ManageModifier(ModType, bool)" path="/param[@name='_modType']"/></param>
    /// <param name="improvement"><inheritdoc cref="Efficiency.ManageModifier(ModType, bool)" path="/param[@name='improvement']"/></param>
    public void ModifyEfficiency(ModType _modType, bool improvement)
    {
        efficiency.ManageModifier(_modType, improvement);
        UIUpdate("Efficiency");
    }

    /// <inheritdoc cref="jData"/>
    [CreateProperty]
    public JobData Job => jData;
    /// <summary>
    /// Updates whole <see cref="jData"/> and updates UI.
    /// </summary>
    /// <param name="data">New <see cref="jData"/></param>
    public void SetJob(JobData data, bool shouldDecide = true)
    {
        jData = data;
        UIUpdate(nameof(Job));
        if (shouldDecide)
            Decide();
#if UNITY_EDITOR
        transform.GetChild(0).GetComponent<TMP_Text>().text = jData.job.ToString();
#endif
    }
    /// <summary>
    /// Updates a part of <see cref="jData"/>.
    /// </summary>
    /// <param name="state">New job.</param>
    /// <param name="interest">New interest.</param>
    /// <param name="path">New path.</param>
    public void SetJob(JobState state, ClickableObject interest = null, List<GridPos> path = null, bool shouldDecide = true)
    {
        if(state == JobState.Free)
        {
            SceneRefs.JobQueue.FreeHuman(this);
        }
            
        jData.job = state;
        if (interest)
            jData.interest = interest;
        if (path != null)
            jData.path = path;
        UIUpdate(nameof(Job));
        if (shouldDecide)
            Decide();

#if UNITY_EDITOR
        transform.GetChild(0).GetComponent<TMP_Text>().text = jData.job.ToString();
#endif
    }
    #endregion

    #region Basic Operations
    /// <inheritdoc/>
    public override GridPos GetPos() => new(transform.position.x, transform.localPosition.y / ClickableObjectFactory.LEVEL_HEIGHT, transform.position.z);

    /// <summary>
    /// Creates a list from <see cref="HumanUtil.humans"/>.
    /// </summary>
    public override void UniqueID() => CreateNewId(SceneRefs.Humans.GetHumen().Select(q => q.id).ToList());

    /// <summary>
    /// Links tick, day and night actions.
    /// </summary>
    public void ActivateHuman()
    {
#if UNITY_EDITOR
        transform.GetChild(0).gameObject.SetActive(true);
#endif
        SceneRefs.Tick.SubscribeToEvent(DoRepetableAction, Tick.TimeEventType.Ticks);
        //SceneRefs.Tick.SubscribeToEvent(Day, Tick.TimeEventType.DayStart);
        ((IModifiable)Inventory.capacity).Init();
        //SceneRefs.tick.SubscribeToEvent(Night, Tick.TimeEventType.Night);
    }

    /// <summary>
    /// Displays path in editor.
    /// </summary>
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
    /// <summary>
    /// <inheritdoc/>
    /// And opens the info window with <see cref="InfoMode.Human"/>.
    /// </summary>
    /// <returns><inheritdoc/></returns>
    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Human);
        return info;
    }
    #endregion Window

    #region Saving
    /// <inheritdoc/>
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new HumanSave();
        (clickable as HumanSave).color = new(transform.GetChild(1).GetComponent<MeshRenderer>().material.color); // saves color of hat
        (clickable as HumanSave).gridPos = GetPos();
        (clickable as HumanSave).rotation = transform.rotation.eulerAngles.y;
        (clickable as HumanSave).jobSave = new(jData, destination);
        (clickable as HumanSave).jobSave.destinationID = destination ? destination.id : -1;
        //(clickable as HumanSave).jobSave.destType = ;// = destination ? destination.id : -1;

        (clickable as HumanSave).inventory = new(inventory);
        (clickable as HumanSave).specs = specialization;
        (clickable as HumanSave).houseID = home ? home.id : -1;
        (clickable as HumanSave).workplaceId = workplace != null ? ((ClickableObject)workplace).id : -1;
        return base.Save(clickable);
    }

    /// <inheritdoc/>
    public override void Load(ClickableObjectSave save)
    {
        HumanSave s = save as HumanSave;
        transform.GetChild(1).GetComponent<MeshRenderer>().material.color = s.color.ConvertColor();
        id = save.id;
        objectName = save.objectName;
        Inventory = new(20);
        // house assigment
        if (s.houseID != -1)
            MyGrid.Buildings.Where(q => q.id == s.houseID).
                SingleOrDefault().GetComponent<House>().ManageAssigned(this, true);

        // workplace assigment
        if (s.workplaceId != -1)
            MyGrid.Buildings.Where(q => q.id == s.workplaceId).
                SingleOrDefault().GetComponent<IAssign>().ManageAssigned(this, true);
        SetJob(new JobData(s.jobSave, this));
        Inventory.Manage(new(s.inventory), true);
        specialization = s.specs;



        base.Load(save);
    }

    #endregion

    #region Human actions
    /// <summary>Called each tick, either calls the action or looks for a new job.</summary>
    public void DoRepetableAction()
    {
        if (repetableAction != null)
        {
            repetableAction(this);
        }
        else if (!nightTime)
        {
            HumanActions.LookForNew(this);
            if (repetableAction != null)
                DoRepetableAction();
        }
    }

    /// <summary>
    /// Reassignes <see cref="repetableAction"/>.
    /// </summary>
    /// <param name="action">New <see cref="repetableAction"/>.</param>
    void ChangeAction(Action<Human> action) => repetableAction = action;

    /// <summary>Changes <see cref="repetableAction"/> according to <see cref="jData"/>.</summary>
    public void Decide()
    {
        if (nightTime)
        {
            if (GoHome().path.Count == 0) // should be called after arriving home
                ChangeAction(null);
            else // decided during the night
                Debug.LogWarning("I'm sleeping, don't bother me!");
            return;
        }
        if (jData.path?.Count > 0)
        {
            ChangeAction(HumanActions.Move);
            return;
        }

        switch (jData.job)
        {
            case JobState.Free:
                if (workplace != null)
                    Idle();
                else
                    HumanActions.LookForNew(this);
                break;
            case JobState.Digging:
                ChangeAction(HumanActions.Dig);
                break;
            case JobState.Constructing:
                ChangeAction(HumanActions.Build);
                break;
            case JobState.Deconstructing:
                ChangeAction(HumanActions.Deconstruct);
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

    /// <summary>Gets the Main <see cref="Elevator"/> and if the Human isn't there move to it, else set free state.</summary>
    public void Idle()
    {
        if (workplace == null)
        {
            GridPos pos = GetPos();
            Elevator el = MyGrid.GetLevelElevator(pos.y);// ClickableObject el = Elevator.main;
            if (!el.IsInside(pos)) // if not standing on the elevator
            {
                JobData data = PathFinder.FindPath(new() { el }, this);
                data.job = JobState.Free;
                if (data.interest != null)
                {
                    data.interest = null;
                    SetJob(data);
                }
                return;
            }
            if (jData.job != JobState.Free)
            {
                SetJob(JobState.Free, path: new());
            }
            else
                ChangeAction(null);
        }
        else if (workplace is IDiggerHut digger)
        {
            if (!HumanActions.FindRockToDig(this))
            {
                if (!(digger as Building).IsInside(GetPos()))
                {
                    JobData data = PathFinder.FindPath(new() { (digger as Building) }, this);
                    data.job = JobState.FullTime;
                    if (data.interest != null)
                    {
                        data.interest = null;
                        SetJob(data);
                    }
                    Debug.Log("Going to work(dig)!");
                }
                else
                {
                    ChangeAction((_) => Idle());
                }
            }
        }
        else if(workplace is IBuilderHut builder)
        {
            if (!HumanActions.FindBuildingsToConstruct(this))
            {
                if (!(builder as Building).IsInside(GetPos()))
                {
                    JobData data = PathFinder.FindPath(new() { (builder as Building) }, this);
                    data.job = JobState.FullTime;
                    if (data.interest != null)
                    {
                        data.interest = null;
                        SetJob(data);
                    }
                    Debug.Log("Going to work(construct)!");
                }
                else
                {
                    ChangeAction((_) => Idle());
                }
            }
        }
    }
    #endregion

    #region Day cycle
    /// <summary>Triggered on day action, resumes the last job, or finds a new one.</summary>
    public void Day()
    {
        nightTime = false; // stop sleeping
        if (jData.job <= JobState.Free)
            SetJob(JobState.Free);
        else
        {
            SetJob(jData.job, path: PathFinder.FindPath(new() { jData.interest }, this).path);
        }
    }

    /// <summary>Triggered on night action, eats food and goes to sleep.</summary>
    public void Night()
    {
        if (!nightTime)
        {
            nightTime = true;
            MyRes.EatFood(this);
        }
        jData.path = GoHome().path;
        ChangeAction(HumanActions.Move);
    }

    /// <summary>
    /// Tries to find way home, if there's no way home then atleast to the elevator.
    /// </summary>
    /// <returns>Found path.</returns>
    public JobData GoHome()
    {
        JobData _jData;
        if (home)
        {
            _jData = PathFinder.FindPath(new() { home }, this);
            if (_jData.interest)
            {
                ModifyEfficiency(ModType.House, true);
                return _jData;
            }
        }
        _jData = PathFinder.FindPath(new() { MyGrid.GetLevelElevator(GetPos().y) }, this);
        ModifyEfficiency(ModType.House, false);
        if (_jData.interest)
        {
            return _jData;
        }
        return new();
    }
    #endregion
}