using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    public List<IResourceProduction> supplyNeeded = new();
    public int Count = 0;
    /// <summary>Chunks and Production buildings that have something to store.</summary>
    public List<StorageObject> pickupNeeded = new();

    /// <summary>Storages</summary>
    [Header("")] public List<IStorage> storages = new();
    [ReadOnly(true)] public List<MonoBehaviour> Storages => storages.Cast<MonoBehaviour>().ToList();
    /// <summary>Job priority</summary>
    [HideInInspector] public List<JobState> priority;

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
                supplyNeeded.Add(interest as IResourceProduction);
                Count++;
                break;
            case JobState.Pickup:
                pickupNeeded.Add((ResourceProductionBuilding)interest);
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
        switch (job)
        {
            case JobState.Digging:
                toBeDug.RemoveAll(q => q.id == interest.id); // remove from the list
                Rock rock = ((Rock)interest);
                break;
            case JobState.Constructing:
                constructions.RemoveAll(q => q.id == interest.id); // remove from the list
                //assigned = interest.GetComponent<Building>().
                break;
            case JobState.Deconstructing:
                deconstructions.RemoveAll(q => q.id == interest.id);
                break;
            case JobState.Supply:
                supplyNeeded.RemoveAll(q => ((ClickableObject)q).id == interest.id);
                Count--;
                break;
            case JobState.Pickup:
                pickupNeeded.RemoveAll(q => q.id == interest.id);
                break;
        }
    }

    /// <summary>
    /// Takes a human away from a job, 
    /// if you need to assign a new job but don't want to destroy the previous.
    /// </summary>
    /// <param name="human"></param>
    public void FreeHuman(Human human)
    {
        ClickableObject interest = human.Job.interest;
        if (!interest)
            return;
        switch (human.Job.job)
        {
            case JobState.Digging:
                ((Rock)interest).Assigned = null;
                break;
            case JobState.Constructing:
            case JobState.Deconstructing:
                ((Building)interest).LocalRes.RemoveRequest(human);
                break;
            case JobState.Supply:
            case JobState.Pickup:
                if (human.Job.interest != human.destination)
                    ((StorageObject)human.Job.interest).LocalRes.RemoveRequest(human);
                if (human.destination)
                {
                    if (human.destination.constructed && human.destination is IResourceProduction)
                        ((IResourceProduction)human.destination).InputResource.RemoveRequest(human);
                    else
                        human.destination.LocalRes.RemoveRequest(human);
                }

                SceneRefs.ObjectFactory.CreateChunk(human.GetPos(), human.Inventory, false);
                human.Inventory.Clear();
                break;
        }
    }
}
