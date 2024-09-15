using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class HumanActions
{
    static int transferPerTick = 5;
    /// <summary>
    /// moves worker to the next position in list, and deletes it. Upon reaching the end, decide what to do next(using jData)
    /// </summary>
    /// <param name="h"></param>
    public static void Move(Human h)
    {
        if (h.jData.path.Count > 0)
        {
            Rotate(h, h.jData.path[0]);
            h.transform.localPosition = h.jData.path[0].ToVec();
            h.jData.path.RemoveAt(0);
        }
        else
        {
            Rotate(h, new(h.jData.interest.gameObject));
            h.ChangeAction(null);
            h.Decide();
        }
    }
    static void Rotate(Human h, GridPos point)
    {
        GridPos humanPos = new(h.gameObject);
        if(point.x > humanPos.x)
            h.transform.rotation = Quaternion.Euler(0, 180, 0);
        else if(point.x < humanPos.x)
            h.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (point.z > humanPos.z)
            h.transform.rotation = Quaternion.Euler(0, 90, 0);
        else if (point.z < humanPos.z)
            h.transform.rotation = Quaternion.Euler(0, 270, 0);
    }
    /// <summary>
    /// digs the stone(lowers the rock integrity)
    /// </summary>
    /// <param name="h"></param>
    public static void Dig(Human h)
    {
        Rock r = h.jData.interest.GetComponent<Rock>();
        r.integrity--;
        r.OpenWindow();
        if(r.integrity <= 0)
        {
            if (r.rockYield.ammount.Sum() > 0)
                r.ChunkCreation(MyGrid.specialPrefabs.GetPrefab("Chunk") as Chunk);
            // destroys the mined block
            MyGrid.RemoveTiles(r);
            h.transform.parent.parent.GetComponent<JobQueue>().CancelJob(JobState.Digging, h.jData.interest); // removes job order
        }
    }
    /// <summary>
    /// if there're all needed resources, progress build, otherwise store
    /// </summary>
    /// <param name="h"></param>
    public static void Build(Human h)
    {
        Building building = h.jData.interest.GetComponent<Building>();
        building.build.constructionProgress++;
        if (building.build.constructionProgress >= building.build.maximalProgress)
        {
            h.jData.interest.GetComponent<Building>().FinishBuild();
            h.GetComponentInParent<JobQueue>().CancelJob(JobState.Constructing, building.GetComponent<ClickableObject>());
            building.localRes.RemoveRequest(h);
            LookForNew(h);
            return;
        }
        building.OpenWindow(); // change to UpdateWindow
    }
    /// <summary>
    /// progresses demolish on building
    /// </summary>
    /// <param name="h"></param>
    public static void Demolish(Human h)
    {
        Building building = h.jData.interest.GetComponent<Building>();
        building.build.constructionProgress -= 2;
        building.OpenWindow();
        if (building.build.constructionProgress <= 0)
        {
            building.Deconstruct(h.transform.localPosition);
            h.GetComponentInParent<JobQueue>().CancelJob(JobState.Deconstructing, h.jData.interest);
            LookForNew(h);
        }
    }
    public static void Store(Human h)
    {
        try
        {
            h.jData.interest.GetComponent<StorageObject>().Store(h, transferPerTick);
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
            h.jData.interest.GetComponent<StorageObject>().Take(h, transferPerTick);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// looks for new jobs, or chunks to empty, TODO: resuply and emptying is not a job, make it like a chunk wíth higher priority
    /// </summary>
    /// <param name="h"></param>
    public static void LookForNew(Human h)
    {
        if (!h.nightTime && !h.lookingForAJob)
        {
            JobQueue jobQueue = h.transform.parent.GetComponentInParent<JobQueue>();
            h.lookingForAJob = true;
            h.jData = new();

            // go throuh the jobs according to priority
            foreach(JobState j in jobQueue.priority)
            {
                if (!HandleCases(jobQueue, h, j))
                    return;
            }
        }
        h.ChangeAction(null);
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
    public static bool HandleCases(JobQueue jobQueue, Human h, JobState j)
    {
        switch (j)
        {
            case JobState.Digging:
                // find toBeDug with no assigned workers
                if (FindInterests(jobQueue.toBeDug.Where(q => q.assigned == null), h, j)) // if found
                {
                    h.jData.interest.GetComponent<Rock>().assigned = h;
                    return false;
                }
                break;
            case JobState.Constructing:
                // builds that are only missing progress not resources
                if (jobQueue.constructions.Count == 0)
                    break;
                List<Building> buildings = jobQueue.constructions.Where(q => q.localRes.Future(false).Equals(q.build.cost)).ToList();
                if (FindInterests(buildings.Where(q => q.localRes.carriers.Count == 0).Cast<ClickableObject>(), h, j))
                {
                    h.jData.interest.GetComponent<Building>().localRes.AddRequest(new(), h, 0);
                    return false;
                }
                // builds that are missing resources to progress further
                if (FilterBuilds(jobQueue.constructions.Where(q => !q.localRes.Future(false).Equals(q.build.cost)), h, j))
                    return false;
                break;
            case JobState.Deconstructing:
                // deconstructions with no workers assigned
                if (FindInterests(jobQueue.deconstructions.Where(q => q.localRes.carriers.Count == 0).Cast<ClickableObject>(), h, j))
                {
                    h.jData.interest.GetComponent<Building>().RequestRes(new(), h, 0);
                    return false;
                }
                break;
            case JobState.Pickup:
                // if there's any space for it
                if (MyRes.globalStorageSpace > 0)
                    if (FindInterests(jobQueue.pickupNeeded.Where(q => q.localRes.Future(false).ammount.Sum() > 0).Cast<ClickableObject>(), h, j))
                    {
                        h.destination = h.jData.interest.GetComponent<Building>();
                        Resource toMove = h.destination.localRes.Future(false);
                        Resource r = new();
                        MyRes.MoveRes(r, toMove.Clone(), toMove, h.inventory.capacity < MyRes.globalStorageSpace ? h.inventory.capacity : MyRes.globalStorageSpace);
                        h.destination.localRes.AddRequest(r, h, -1);
                        MyRes.FindStorage(r, h);
                        if (toMove.ammount.Sum() == 0)
                        {
                            jobQueue.CancelJob(JobState.Pickup, h.jData.interest);
                        }
                        return false;
                    }
                break;
            case JobState.Supply:
                if (jobQueue.supplyNeeded.Count == 0)
                    break;
                if (FilterBuilds(jobQueue.supplyNeeded.Select(q => q.GetComponent<Building>()), h, j))
                    return false;
                break;
            case JobState.Cleanup:
                if (MyRes.globalStorageSpace == 0)
                    return true;
                IEnumerable<ClickableObject> chunks;
                if ((chunks = MyGrid.chunks.Where(q => q.localRes.Future(false).ammount.Sum() > 0).Cast<ClickableObject>()).Count() > 0)
                {
                    if (FindInterests(chunks, h, JobState.Pickup))
                    {
                        Resource toMove = new();
                        Chunk chunk = (Chunk)h.jData.interest;
                        Resource chunkStorage = chunk.localRes.Future(false);
                        MyRes.MoveRes(toMove, chunkStorage, chunkStorage, h.inventory.capacity - h.inventory.ammount.Sum());
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
        foreach(Building building in constructions)
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
        if(interests.Count() > 0)
        {
            h.jData = PathFinder.FindPath(interests.ToList(), h);
            if (h.jData.interest)
            {
                h.jData.job = jobs;
                h.ChangeAction(Move);
                h.lookingForAJob = false;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// adds to production progress, if there's no workplace go Idle
    /// </summary>
    /// <param name="h"></param>
    public static void DoProduction(Human h)
    {
        if (h.workplace)
        {
            h.workplace.Produce();
        }
        else
        {
            h.Idle();
        }
    }

    /// <summary>
    /// proggresses sleep
    /// </summary>
    /// <param name="h"></param>
    public static void Sleep(Human h)
    {
        h.OpenWindow();
        h.sleep++;
    }
}
