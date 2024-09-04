using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
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
    Eating, //
    Sleeping, //
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
    // human info and stats
    //public int health = 100;
    //public float speed = 0.5f;
    public JobData jData = new();
    public Resource inventory = new();
    public Specs specialization = Specs.Worker;

    // statuses
    public bool nightTime = false;
    public int sleep = 0;
    public bool hasEaten = true;
    public bool lookingForAJob = false;

    // job data
    public House home;
    public Building destination;
    public ProductionBuilding workplace;
    public Action<Human> repetableAction;
    public void ActivateHuman()
    {
        transform.parent.parent.GetComponent<Humans>().ticks.tickAction += DoRepetableAction;
        DayTime.day += Day;
        DayTime.night += Night;
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
            repetableAction.Invoke(this);
        else
            HumanActions.LookForNew(this);

    }
    public override void UniqueID() // creates a random int
    {
        List<int> ids = transform.parent.parent.GetChild(0).GetComponentsInChildren<Human>().Select(q => q.id).ToList();
        ids.AddRange(transform.parent.parent.GetChild(1).GetComponentsInChildren<Human>().Select(q => q.id).ToList());
        CreateNewId(ids);
    }
    public void ChangeAction(Action<Human> action)
    {
        transform.GetChild(0).GetComponent<TMP_Text>().text = jData.job.ToString();
        OpenWindow();
        repetableAction = action;
    }
    public void Decide() // for new Jobs or for managing fullTime jobs
    {
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
            case JobState.Eating:
                StartCoroutine(Eat());
                break;
            default:
                HumanActions.LookForNew(this);
                Debug.LogError("Sometnig went wrong, don't know what to do!!!");
                break;
        }
    }
    public void Idle()
    {
        jData = new JobData();
        GridPos v = new(GameObject.FindGameObjectsWithTag("Elevator").First(q => q.GetComponent<Elevator>().main).transform.localPosition);
        if (!v.Equals(new GridPos(transform.localPosition))) // if not standing on the elevator
        {
            jData = PathFinder.FindPath(new() { GameObject.FindGameObjectsWithTag("Elevator").First(q => q.GetComponent<Elevator>().main).GetComponent<Elevator>() }, this);
            ChangeAction(HumanActions.Move); //  go to the elevator and look for a new Job 
        }
        else
        {
            OpenWindow();
        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //--------------------------------------------------------DAY/NIGHT CYCLE-----------------------------------------------------------------/
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void Day()
    {
        /*nightTime = false; // stop sleeping
        StopC();
        switch (jData.job)
        {
            case jobs.free:
                RepetableActions.LookForNew(this);
                print("I'm free");
                break;
            case jobs.building:
                c.FindResources();
                break;
            case jobs.carrying:
                d.Carry();
                break;
            case jobs.demolishing:
            case jobs.digging:
            case jobs.fullTime:
            case jobs.supply:
            case jobs.pickup:
            case jobs.eating:
            default:
                jData.path = gameObject.GetComponent<PathFinder>().FindPath(new() {jData.interest}, this).Result.path;
                changeAction(RepetableActions.Move);
                break;
        }*/
    }
    public void Night()
    {
        /*StopC();
        sleep = 0;
        nightTime = true;
        if(jData.job == jobs.fullTime)
        {
            jData.interest.GetComponent<ProductionBuilding>().working.Remove(this); // removes from working
            if (jData.interest.GetComponent<Research_Production>()) // if working in research building
            {
                transform.parent.parent.GetComponent<Humans>().Research_Script.number_of_researchers--; // removes from researchers
            }
        }
        FindPlaceToEat();*/
    }

    void FindPlaceToEat()
    {
        /*List<ClickableObject> diners = MyGrid.buildings
            .Where(q => q.GetComponent<Diner>()).Select(q => q.GetComponent<Diner>())
            .Where(q => MyRes.GetAmmount(q.localRes, 3) > 0).Select(q => q.GetComponent<ClickableObject>()).ToList();

        RepetableActions.LookForNew(this);

        if (diners.Count > 0) // if there are diner with food
        {
            jData = gameObject.GetComponent<PathFinder>().FindPath(new(transform.localPosition), diners, this).Result;
        }
        else // there are not
        {
            diners = MyGrid.buildings
            .Where(q => q.GetComponent<Storage>()).Select(q => q.GetComponent<Storage>())
            .Where(q => MyRes.GetAmmount(q.localRes, 3) > 0).Select(q => q.GetComponent<ClickableObject>()).ToList();
            if (diners.Count > 0)
            {
                jData = gameObject.GetComponent<PathFinder>().FindPath(new(transform.localPosition), diners, this).Result;
            }
        }

        if(jData.interest == null)
        {
            hasEaten = false;
            GoHome();
        }
        else
        {
            jData.job = jobs.eating;
            changeAction(RepetableActions.Move);
        }*/
    }

    public void GoHome()
    {
        /*OpenWindow();
        if (home != null)
        {
            if (home.transform.localPosition != transform.localPosition) // sends worker home to sleep
            {
                jData.job = jobs.sleeping;
                jData.path = gameObject.GetComponent<PathFinder>().FindPath(new(transform.localPosition), new() { home }, this).Result.path;
                changeAction(RepetableActions.Move);
                return;
            }
            changeAction(RepetableActions.Sleep); // worker starts sleeping
        }
        else // if no home, move to elevator
        {
            StorageObject el = GameObject.Find("Elevator").GetComponent<StorageObject>();
            if (el.transform.localPosition != transform.localPosition)
            {
                jData.job = jobs.sleeping;
                jData.path = gameObject.GetComponent<PathFinder>().FindPath(new(transform.localPosition), new() { el }, this).Result.path;
                changeAction(RepetableActions.Move);
                return;
            }
        }*/
    }

    IEnumerator Eat()
    {
        /*Building building = jData.interest.GetComponent<Building>();
        if (building.localRes.ammount[4] > 0)
        {
            building.localRes.ammount[4]--;
            if (building.selected && building.GetComponent<Diner>())
            {
                string _s = "";
                Resource r = jData.interest.GetComponent<Diner>().localRes;
                for (int i = 0; i < r.ammount.Count; i++)
                {
                    if (r.ammount[i] > 0)
                    {
                        _s += $"{r.id[i]}: {r.ammount[i]}";
                    }
                }
                GameObject.Find("Generated Resource").GetComponent<TMP_Text>().text = _s;
            }
            else
            {
                building.OpenWindow();
            }
            hasEaten = true;
            print("nom");
            yield return new WaitForSeconds(0.5f);
            print("nom nom nom");
            GoHome();
        }
        else
        {
            FindPlaceToEat(); 
            yield break;
        }*/
        yield return new();
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
                info.SwitchMods(1, name);
            }
            string s;
            // JOB INFO
            s =
                $"Job - {jData.job}\n" +
                $"Object - {(jData.interest ? jData.interest.name: "")}";
            info.transform.GetChild(1).GetChild(1).GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = s;
            // STATUS INFO
            s = 
                $"Sleep - {sleep}\n" +
                $"HasEaten - {hasEaten}\n" +
                $"Inventory - {MyRes.GetDisplayText(inventory)}";
            info.transform.GetChild(1).GetChild(1).GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = s;
        }
        // floating text update
        transform.GetChild(0).GetComponent<TMP_Text>().text = jData.job.ToString();
        return null;
    }
}