using System;
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
    /// <summary>Chunks and Production buildings that have something to store.</summary>
    public List<StorageObject> pickupNeeded = new();

    /// <summary>Storages</summary>
    [Header("")] List<IStorage> storages = new();
    public void AddStorage(IStorage store)
    {
        storages.Add(store);
        MyRes.globalStorageSpace += store.LocalResources.capacity.currentValue;
    }
    [ReadOnly(true)] public List<IStorage> Storages => storages.ToList();
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
                var humans = SceneRefs.Humans.GetHumans().Where((q) =>
                {
                    return q.Job.job == JobState.FullTime && q.Workplace is IDiggerHut;
                });
                foreach (var human in humans)
                {
                    if (HumanActions.FindRockToDig(human))
                        break;
                }

                break;
            case JobState.Constructing:
                constructions.Add((Building)interest);
                break;
            case JobState.Deconstructing:
                deconstructions.Add((Building)interest);
                break;
            case JobState.Supply:
                supplyNeeded.Add(interest as IResourceProduction);
                break;
            case JobState.Pickup:
                pickupNeeded.Add((StorageObject)interest);
                break;
        }
    }

    /// <summary>
    /// Unregisters a job, either by completion or player canclation.
    /// Only updates the lists doesn't touch human actions.
    /// </summary>
    /// <param name="job">Which type of job was canceled.</param>
    /// <param name="interest">Job interest to remove.</param>
    public void CancelJob(JobState job, ClickableObject interest) // removes a logged object
    {
        switch (job)
        {
            case JobState.Digging:
                toBeDug.RemoveAll(q => q.id == interest.id); // remove from the list
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
                break;
            case JobState.Pickup:
                pickupNeeded.RemoveAll(q => q.id == interest.id);
                break;
        }
    }

}
