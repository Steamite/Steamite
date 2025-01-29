using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Handles and stores job requests.</summary>
public class JobQueue : MonoBehaviour
{
    /// <summary>Rocks marked for digging out.</summary>
    [Header("Job Objects")] public List<Rock> toBeDug = new();
    /// <summary>Buildings in construction.</summary>
    public List<Building> constructions = new();
    /// <summary>Buildings in deconstruction.</summary>
    public List<Building> deconstructions = new();
    /// <summary>Production buildings that need input resources.</summary>
    public List<ProductionBuilding> supplyNeeded = new();
    /// <summary>Chunks and Production buildings that have something to store.</summary>
    public List<StorageObject> pickupNeeded = new();

    /// <summary>Storages</summary>
    [Header("")] public List<IStorage> storages = new();
    /// <summary>Job priority</summary>
    public List<JobState> priority;

    /// <summary>
    /// Registers new job.
    /// </summary>
    /// <param name="job">Which type of job was added.</param>
    /// <param name="interest">Job interest to store.</param>
    public void AddJob(JobState job, ClickableObject interest)
    {
        switch (job)
        {
            case JobState.Digging:
                toBeDug.Add((Rock)interest);
                break;
            case JobState.Constructing:
                constructions.Add((Building)interest);
                break;
            case JobState.Deconstructing:
                deconstructions.Add((Building)interest);
                break;
            case JobState.Supply:
                supplyNeeded.Add((ProductionBuilding)interest);
                break;
            case JobState.Pickup:
                pickupNeeded.Add((ProductionBuilding)interest);
                break;
        }
    }

    /// <summary>
    /// Unregisters a job, either by completion or player canclation.
    /// </summary>
    /// <param name="job">Which type of job was canceled.</param>
    /// <param name="interest">Job interest to remove.</param>
    public void CancelJob(JobState job, ClickableObject interest) // removes a logged object
    {
        List<Human> assigned = new();
        switch (job)
        {
            case JobState.Digging:
                toBeDug.RemoveAll(q => q.id == interest.id); // remove from the list

                Rock rock = interest.GetComponent<Rock>();
                assigned.Add(rock.Assigned);
                rock.Assigned = null;
                break;
            case JobState.Constructing:
                constructions.RemoveAll(q => q.id == interest.id); // remove from the list
                //assigned = interest.GetComponent<Building>().
                break;
            case JobState.Deconstructing:
                deconstructions.RemoveAll(q => q.id == interest.id);
                break;
            case JobState.Supply:
                supplyNeeded.RemoveAll(q => q.id == interest.id);
                break;
            case JobState.Pickup:
                pickupNeeded.RemoveAll(q => q.id == interest.id);
                break;
        }
        foreach(Human h in assigned)
        {
            if(h)
                HumanActions.LookForNew(h);
        }
    }
}
