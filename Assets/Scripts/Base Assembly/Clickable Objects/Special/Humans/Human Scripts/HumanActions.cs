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
            if (h.Job.path.Count == 0 && h.Job.job != JobState.Free)
            {
                h.DoRepetableAction();
            }
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
        if ((h.Job.interest as Rock).DamageRock(digSpeed * h.Efficiency, h))
        {
            SceneRefs.JobQueue.CancelJob(JobState.Digging, h.Job.interest); // removes job order
            if (FindRockToDig(h) == false)
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
        if (h.workplace is IProduction prod)
        {
            prod.ProgressProduction(h.Efficiency * productionSpeed);
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
            SceneRefs.JobQueue.CancelJob(JobState.Constructing, building);
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
        if (building.ProgressDeconstruction(h.Efficiency * decontructSpeed, h))
        {
            SceneRefs.JobQueue.CancelJob(JobState.Deconstructing, building);
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
            ((StorageObject)h.Job.interest).Take(h, transferPerTick);
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
            JobQueue jobQueue = SceneRefs.JobQueue;
            h.lookingForAJob = true;

            // go throuh the jobs according to priority
            foreach (JobState j in jobQueue.priority)
            {
                if (HandleJobTypes(jobQueue, h, j))
                    return;
            }
        }
        else if (h.lookingForAJob)
        {
            Debug.LogError("SOMETHING IS WRONG, should not get here!");
            return;
        }
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
    /// <returns>True if job was found and assigned.</returns>
    public static bool HandleJobTypes(JobQueue jobQueue, Human h, JobState j)
    {
        switch (j)
        {
            case JobState.Pickup:
                // if there's any space for it
                if (MyRes.globalStorageSpace > 0)
                    if (FindInterests(jobQueue.pickupNeeded.Where(q => q.LocalRes.Future().Sum() > 0), h, j))
                    {
                        Building pickupObject = h.Job.interest as Building;
                        Resource toMove = pickupObject.LocalRes.Future();
                        CapacityResource r = new(h.Inventory.FreeSpace);
                        MyRes.MoveRes(r,
                            new(toMove),
                            toMove,
                            h.Inventory.capacity < MyRes.globalStorageSpace
                                ? h.Inventory.capacity.currentValue
                                : MyRes.globalStorageSpace);
                        MyRes.FindStorage(r, h);
                        if (h.destination)
                        {
                            h.destination.RequestRes(r, h, 1);
                            pickupObject.RequestRes(r, h, -1);
                            return true;
                        }
                    }
                break;
            case JobState.Supply:
                if (FilterBuilds(jobQueue.supplyNeeded.Select(q => (Building)q), h, j))
                    return true;
                break;
            case JobState.Cleanup:
                if (MyRes.globalStorageSpace == 0)
                    break;
                IEnumerable<ClickableObject> chunks;
                if ((chunks = MyGrid.chunks.Where(q => q.LocalRes.Future().Sum() > 0)).Count() > 0)
                {
                    if (FindInterests(chunks, h, JobState.Pickup))
                    {
                        CapacityResource toMove = new(-1);
                        Chunk chunk = (Chunk)h.Job.interest;
                        Resource chunkStorage = chunk.LocalRes.Future();
                        MyRes.MoveRes(
                            toMove,
                            chunkStorage,
                            chunkStorage,
                            h.Inventory.capacity - h.Inventory.Sum());
                        chunk.RequestRes(toMove, h, -1);
                        return true;
                    }
                }
                break;

            default:
                break;
        }
        return false;
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
    /// Tries to find a rock that's available for digging.
    /// </summary>
    /// <param name="h">Human that's looking.</param>
    /// <returns>If a rock was found and assigned to the human.</returns>
    public static bool FindRockToDig(Human h)
    {
        // find toBeDug with no assigned workers
        if (FindInterests(SceneRefs.JobQueue.toBeDug.Where(q => q.Assigned == null), h, JobState.Digging)) // if found
        {
            (h.Job.interest as Rock).Assigned = h;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tries to find a builing that needs resources or to be build.
    /// </summary>
    /// <param name="h">Human that's looking.</param>
    /// <returns>If a suitable Building was found and assigned.</returns>
    public static bool FindBuildingsToConstruct(Human h)
    {
        JobQueue jobQueue = SceneRefs.JobQueue;
        if (jobQueue.constructions.Count == 0)
            return false;
        List<Building> missingResoucerces = new();
        List<Building> missingProgress = new();
        foreach (var building in jobQueue.constructions)
        {
            if (!building.Cost.Same(building.LocalRes.Future()) && building.constructed == false)
            {
                missingResoucerces.Add(building);
            }
            else if (building.LocalRes.HasNoCarriers())
                missingProgress.Add(building);
        }

        // builds that are only missing progress not resources
        if (FindInterests(missingProgress, h, JobState.Constructing))
        {
            (h.Job.interest as Building).RequestRes(new(), h, 0);
            return true;
        }

        // builds that are missing resources to progress further
        if (FilterBuilds(missingResoucerces, h, JobState.Constructing))
            return true;

        

        return false;
    }

    public static bool FindBuildingsToDeconstruct(Human h)
    {
        JobQueue jobQueue = SceneRefs.JobQueue;
        if (jobQueue.deconstructions.Count == 0)
            return false;
        if (FindInterests(jobQueue.deconstructions.Where(q => q.LocalRes.carriers.Count == 0), h, JobState.Deconstructing))
        {
            h.Job.interest.GetComponent<Building>().RequestRes(new(), h, 0);
            return true;
        }
        return false;
    }


    /// <summary>
    /// finds the closest interest
    /// </summary>
    /// <param name="constructions"></param>
    /// <param name="h"></param>
    /// <param name="job"></param>
    /// <returns></returns>
    static bool FindInterests(IEnumerable<ClickableObject> interests, Human h, JobState job)
    {
        if (interests.Count() > 0)
        {
            if (job == JobState.Pickup || job == JobState.Cleanup)
            {
                List<StorageObject> objects = new();
                foreach (var item in interests)
                {
                    Resource resource = (item as StorageObject).LocalRes.Future();
                    if (resource.ammounts.Sum() > 0 && MyRes.CanStore(resource, h))
                        objects.Add(item as StorageObject);
                }
                interests = objects;
            }
            JobData data = PathFinder.FindPath(interests.ToList(), h);
            if (data.interest)
            {
                data.job = job;
                h.SetJob(data);
                h.lookingForAJob = false;
                return true;
            }
        }
        return false;
    }
    #endregion 
}
