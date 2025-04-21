using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

/// <summary>Building that doesn't produce resources but creates research.</summary>
public class Research_Production : Building, IProduction, IAssign
{
    [CreateProperty]
    public List<Human> Assigned { get; set; } = new();
    public int AssignLimit { get => assignLimit; set => assignLimit = value; }
    [SerializeField] int assignLimit;
    public float ProdTime { get; set; }
    public float CurrentTime { get; set; }
    public int Modifier { get; set; }
    public bool Stoped { get; set; }

    #region Window
    /// <summary>
    /// Adds "Research" to <paramref name="toEnable"/>. <br/>
    /// <inheritdoc cref="Building.OpenWindowWithToggle(InfoWindow, List{string})"/>
    /// </summary>
    /// <inheritdoc/>
    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Research");
        toEnable.Add("Assign");
        base.OpenWindowWithToggle(info, toEnable);
    }
    #endregion


    #region Production
    /// <summary>
    /// Triggers research event.
    /// </summary>
    /// <param name="speed"></param>
    public void ProgressProduction(float speed)
    {
        SceneRefs.researchAdapter.DoProduction(speed);
    }

    public bool ManageAssigned(Human human, bool add)
    {
        if (add)
        {
            if (Assigned.Count == assignLimit)
                return false;
            JobData job = PathFinder.FindPath(
                new List<ClickableObject>() { this },
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
                human.ChangeAction(HumanActions.Move);
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
        UIUpdate(nameof(Assigned));
        return true;
    }

    public List<Human> GetUnassigned()
    {
        return SceneRefs.humans.GetPartTime();
    }

    public void Product()
    {
        // Stop animation
    }

    #endregion
}
