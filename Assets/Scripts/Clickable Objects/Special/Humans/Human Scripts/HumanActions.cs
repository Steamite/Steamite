using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Actions that can <see cref="Human"/>s do, and looking for new jobs.</summary>
public static class HumanActions
{
    #region Variables

    /// <summary>Max ammount of resources that can be transfered each tick.</summary>
    static readonly int transferPerTick = 5;

    //Efficiency modifiers
    /// <summary>Multiplies progress of <see cref="Dig(Human)"/> action.</summary>
    static readonly float digSpeed = 1f;
    /// <summary>Multiplies progress of <see cref="Build(Human)"/> action.</summary>
    static readonly float buildSpeed = 1f;
    /// <summary>Multiplies progress of <see cref="Deconstruct(Human)"/> action.</summary>
    static readonly float decontructSpeed = 1f;
    /// <summary>Multiplies progress of <see cref="DoProduction(Human)"/> action.</summary>
    static readonly float productionSpeed = 1f;
    #endregion
    public static void ChangeEfficiency(JobState toChange, int ammount)
    {
        throw new NotImplementedException();
        /*switch (toChange)
        {
            case JobState.Digging:
                digSpeed += ammount;
                break;
            case JobState.Constructing:
            case JobState.Deconstructing:
                buildSpeed += ammount;
                break;
            case JobState.FullTime:
                productionSpeed += ammount;
                break;
            default:
                Debug.LogError($"Undifined jobState, {toChange}");
                break;
        }*/
    }

    #region Movement
    /// <summary>
    /// Moves worker to the next position in list, and deletes it. Upon reaching the end, decide what to do next(using jData).
    /// </summary>
    /// <param name="h">Actor doing the action.</param>
    public static void Move(Human h)
    {
        JobData job = h.Job;
        if (job.path.Count > 0)
        {
            Rotate(h, job.path[0]);
            int y = Mathf.RoundToInt(h.transform.localPosition.y);
            h.transform.localPosition = job.path[0].ToVec();
            if (Mathf.RoundToInt(h.transform.localPosition.y) != y)
            {
                h.gameObject.SetActive(MyGrid.currentLevel == h.GetPos().y);
            }
            job.path.RemoveAt(0);
        }
        else
        {
            if (job.interest != null)
                Rotate(h, job.interest.GetPos());
            h.Decide();
        }
    }

    /// <summary>
    /// After <see cref="Move(Human)"/> rotates toward next position.
    /// </summary>
    /// <param name="h"><inheritdoc cref="Move(Human)"/></param>
    /// <param name="point">next position</param>
    static void Rotate(Human h, GridPos point)
    {
        GridPos humanPos = h.GetPos();
        if (point.x > humanPos.x)
            h.transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (point.x < humanPos.x)
            h.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (point.z > humanPos.z)
            h.transform.rotation = Quaternion.Euler(0, 90, 0);
        else if (point.z < humanPos.z)
            h.transform.rotation = Quaternion.Euler(0, 270, 0);
    }
    #endregion

    #region Rocks
    /// <summary>
    /// Lowers integrity of the interest(must be <see cref="Rock"/>). If <paramref name="h"/> destroyes it, removes coresponding job.
    /// </summary>
    /// <param name="h"><inheritdoc cref="Move(Human)"/></param>
    public static void Dig(Human h)
    {
        if (h.Job.interest.GetComponent<Rock>().DamageRock(digSpeed * h.Efficiency))
        {
            SceneRefs.jobQueue.CancelJob(JobState.Digging, h.Job.interest); // removes job order
            h.SetJob(JobState.Free);
        }
    }
    #endregion

    #region Production
    /// <summary>
    /// adds to production progress, if there's no workplace go Idle
    /// </summary>
    /// <param name="h"></param>
    public static void DoProduction(Human h)
    {
        if (h.workplace != null)
        {
            ((IProduction)h.workplace).ProgressProduction(h.Efficiency * productionSpeed);
        }
        else
        {
            h.Idle();
        }
    }
    #endregion

    #region Buildings
    /// <summary>
    /// Adds to production progress
    /// </summary>
    /// <param name="h"></param>
    public static void Build(Human h)
    {
        Building building = h.Job.interest as Building;
        if (building.ProgressConstruction(h.Efficiency * buildSpeed))
        {
            building.LocalRes.RemoveRequest(h);
            SceneRefs.jobQueue.CancelJob(JobState.Constructing, building);
            h.SetJob(JobState.Free);
        }
    }

    /// <summary>
    /// progresses demolish on building
    /// </summary>
    /// <param name="h"></param>
    public static void Deconstruct(Human h)
    {
        Building building = h.Job.interest as Building;
        if(building.ProgressDeconstruction(h.Efficiency * decontructSpeed, h))
        {
            SceneRefs.jobQueue.CancelJob(JobState.Deconstructing, building);
            h.SetJob(JobState.Free);
        }
    }

    #endregion

    #region Storage
    public static void Store(Human h)
    {
        try
        {
            ((StorageObject)h.Job.interest).Store(h, transferPerTick);
            h.UIUpdate(nameof(Human.Inventory));
            h.Inventory.RemoveEmpty();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public static void Take(Human h)
    {
        try
        {
            h.Job.interest.GetComponent<StorageObject>().Take(h, transferPerTick);
            h.UIUpdate(nameof(Human.Inventory));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    #endregion

    #region Idle
    /// <summary>
    /// Looks for new jobs.
    /// </summary>
    /// <param name="h">Human that needs the new job.</param>
    public static void LookForNew(Human h)
    {
        h.destination = null;
        if (!h.nightTime && !h.lookingForAJob)
        {
            JobQueue jobQueue = SceneRefs.jobQueue;
            h.lookingForAJob = true;

            // go throuh the jobs according to priority
            foreach (JobState j in jobQueue.priority)
            {
                if (!HandleJobTypes(jobQueue, h, j))
                    return;
            }
        }
        else if (h.lookingForAJob)
            Debug.LogError("SOMETHING IS WRONG, should not get here!");
        h.Idle();
        h.lookingForAJob = false;
        return;
    }

    /// <summary>
    /// tries to find job of the suppplied type
    /// </summary>
    /// <param name="jobQueue"></param>
    /// <param name="h"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    public static bool HandleJobTypes(JobQueue jobQueue, Human h, JobState j)
    {
        switch (j)
        {
            case JobState.Digging:
                // find toBeDug with no assigned workers
                if (FindInterests(jobQueue.toBeDug.Where(q => q.Assigned == null), h, j)) // if found
                {
                    h.Job.interest.GetComponent<Rock>().Assigned = h;
                    return false;
                }
                break;
            case JobState.Constructing:
                if (jobQueue.constructions.Count == 0)
                    break;
                List<Building> missingResoucerces = new();
                List<Building> missingProgress = new();
                foreach (var building in jobQueue.constructions)
                {
                    if (!building.LocalRes.Future().Equals(building.Cost) && building.constructed == false)
                    {
                        missingResoucerces.Add(building);
                    }
                    else if(building.LocalRes.carriers.Count == 0)
                        missingProgress.Add(building);
                }

                // builds that are only missing progress not resources
                if (FindInterests(missingProgress, h, j))
                {
                    h.Job.interest.GetComponent<Building>().RequestRes(new(), h, 0);
                    return false;
                }

                // builds that are missing resources to progress further
                if (FilterBuilds(missingResoucerces, h, j))
                    return false;

                break;
            case JobState.Deconstructing:
                // deconstructions with no workers assigned
                if (FindInterests(jobQueue.deconstructions.Where(q => q.LocalRes.carriers.Count == 0), h, j))
                {
                    h.Job.interest.GetComponent<Building>().RequestRes(new(), h, 0);
                    return false;
                }
                break;
            case JobState.Pickup:
                // if there's any space for it
                if (MyRes.globalStorageSpace > 0)
                    if (FindInterests(jobQueue.pickupNeeded.Where(q => q.LocalRes.Future().Sum() > 0), h, j))
                    {
                        h.destination = h.Job.interest.GetComponent<Building>();
                        Resource toMove = h.destination.LocalRes.Future();
                        CapacityResource r = new(-1);
                        MyRes.MoveRes(r, 
                            toMove.Clone(), 
                            toMove, 
                            h.Inventory.capacity < MyRes.globalStorageSpace 
                                ? h.Inventory.capacity.currentValue 
                                : MyRes.globalStorageSpace);
                        h.destination.RequestRes(r, h, -1);
                        MyRes.FindStorage(r, h);
                        if (toMove.Sum() == 0)
                        {
                            jobQueue.CancelJob(JobState.Pickup, h.Job.interest);
                        }
                        return false;
                    }
                break;
            case JobState.Supply:
                if (jobQueue.supplyNeeded.Count == 0)
                    break;
                if (FilterBuilds(jobQueue.supplyNeeded.Select(q => (Building)q), h, j))
                    return false;
                break;
            case JobState.Cleanup:
                if (MyRes.globalStorageSpace == 0)
                    return true;
                IEnumerable<ClickableObject> chunks;
                if ((chunks = MyGrid.chunks.Where(q => q.LocalRes.Future().Sum() > 0)).Count() > 0)
                {
                    if (FindInterests(chunks, h, JobState.Pickup))
                    {
                        CapacityResource toMove = new(-1);
                        Chunk chunk = (Chunk)h.Job.interest;
                        Resource chunkStorage = chunk.LocalRes.Future();
                        MyRes.MoveRes(toMove, chunkStorage, chunkStorage, h.Inventory.capacity - h.Inventory.Sum());
                        chunk.RequestRes(toMove, h, -1);
                        return false;
                    }
                }
                break;

            default:
                break;
        }
        return true;
    }

    static bool FilterBuilds(IEnumerable<Building> constructions, Human h, JobState j)
    {
        foreach (Building building in constructions)
        {
            if (MyRes.FindResources(h, building, j))
                return true;
        }
        return false;
    }

    /// <summary>
    /// finds the closest build
    /// </summary>
    /// <param name="constructions"></param>
    /// <param name="h"></param>
    /// <param name="jobs"></param>
    /// <returns></returns>
    static bool FindInterests(IEnumerable<ClickableObject> interests, Human h, JobState jobs)
    {
        if (interests.Count() > 0)
        {
            JobData data = PathFinder.FindPath(interests.ToList(), h);
            if (data.interest)
            {
                data.job = jobs;
                h.SetJob(data);
                h.lookingForAJob = false;
                return true;
            }
        }
        return false;
    }
    #endregion 
}
