using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public interface IAssign
{
    [CreateProperty] public List<Human> Assigned { get; set; }
    /// <summary>Needs to have a field in the class, so it can be serialized.</summary>
    [CreateProperty] public ModifiableInteger AssignLimit { get; set; }

    #region Assigment
    /// <summary>
    /// Assigns or unassigns the <paramref name="human"/>.
    /// </summary>
    /// <param name="human">To modify.</param>
    /// <param name="add">Add or remove.</param>
    /// <returns>True if succesful, false if operation failed</returns>
    public bool ManageAssigned(Human human, bool add)
    {
        if (add)
        {
            if (Assigned.Count == AssignLimit.currentValue)
                return false;
            JobData job = PathFinder.FindPath(
                new List<ClickableObject>() { (ClickableObject)this },
                human);
            if (job.interest)
            {
                Assigned.Add(human);
                human.transform.SetParent(SceneRefs.humans.transform.GetChild(1).transform);
                human.workplace = this;
                job.job = JobState.FullTime;

                SceneRefs.jobQueue.FreeHuman(human);
                if (!human.nightTime)
                    human.SetJob(job);
                else
                    human.SetJob(JobState.FullTime, job.interest);
                human.Decide();
                human.lookingForAJob = false;

            }
            else
            {
                Debug.LogError("cant find way here");
                return false;
            }
        }
        else
        {
            Assigned.Remove(human);
            human.workplace = null;
            human.transform.SetParent(SceneRefs.humans.transform.GetChild(0).transform);
            human.SetJob(JobState.Free);
            human.Idle();
        }
        ((IUpdatable)this).UIUpdate(nameof(Assigned));
        return true;
    
}


    /// <summary>
    /// Returns humans that are not assigned in the buildings.
    /// </summary>
    /// <returns><see cref="NotImplementedException"/> </returns>
    public List<Human> GetUnassigned()
    {
        return SceneRefs.humans.GetPartTime();
    }
    #endregion
}
