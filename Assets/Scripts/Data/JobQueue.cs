using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

public class JobQueue : MonoBehaviour
{
    [Header("Job Objects")]
    public List<Rock> toBeDug = new();                      // digging
    public List<Building> constructions = new();            // building
    public List<Building> deconstructions = new();          // deconstructing
    public List<ProductionBuilding> supplyNeeded = new();   // supplying
    public List<StorageObject> pickupNeeded = new();        // supplying, pickup
    public List<Storage> storages = new();                  // supplying
    //public List<Chunk> chunks = new();              // supplying, cleanup
    [Header("Modifiable")]
    public List<JobState> priority;

    /// <summary>
    /// moves job priority up or down
    /// </summary>
    /// <param name="previus"></param>
    /// <param name="newPrio"></param>
    public void ChangePriority(int previus, int newPrio)
    {
        JobState j = priority[previus];
        if (previus > newPrio)
        {
            for (int i = previus; i > newPrio; i--)
            {
                priority[previus] = priority[previus - 1];
            }
        }
        else
        {
            for (int i = previus; i < newPrio; i++)
            {
                priority[previus] = priority[previus + 1];
            }
        }
        priority[newPrio] = j;
    }
    public void AddJob(JobState job, ClickableObject interest) // logs a new object that needs to be done
    {
        switch (job)
        {
            case JobState.Digging:
                toBeDug.Add(interest.GetComponent<Rock>());
                break;
            case JobState.Constructing:
                constructions.Add(interest.GetComponent<Building>());
                break;
            case JobState.Deconstructing:
                deconstructions.Add(interest.GetComponent<Building>());
                break;
            case JobState.Supply:
                supplyNeeded.Add(interest.GetComponent<ProductionBuilding>());
                break;
            case JobState.Pickup:
                pickupNeeded.Add(interest.GetComponent<ProductionBuilding>());
                break;
        }
    }

    public void CancelJob(JobState job, ClickableObject interest) // removes a logged object
    {
        List<Human> assigned = new();
        switch (job)
        {
            case JobState.Digging:
                toBeDug.RemoveAll(q => q.id == interest.id); // remove from the list
                assigned.Add(interest.GetComponent<Rock>().assigned);
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
