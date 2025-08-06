using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>All available resource types.</summary>
public enum ResourceType
{
    None,
    Coal,
    Metal,
    Stone,
    Food,
    Wood,
}

/// <summary>A util library for resource operations</summary>
public static class MyRes
{
    #region Variables
    /// <summary>Used for faster determening new jobs faster.</summary>
    public static int globalStorageSpace;
    /// <summary>All storage buildings.</summary>
    static List<IStorage> storage;
    /// <summary>Reference to global resource storage counter.</summary>
    public static ResourceDisplay resDataSource;
    #endregion

    #region Getters
    /// <summary>Reference to global money.</summary>
    public static int Money => resDataSource.Money;
    #endregion

    #region Init

    public static void PreLoad()
    {
        resDataSource = SceneRefs.Stats.GetComponent<ResourceDisplay>();
    }
    /// <summary>
    /// Starting function for the resource system.
    /// </summary>
    /// <param name="setupStorages"></param>
    public static void ActivateResources()
    {
        // Update text, or display error
        try
        {
            resDataSource.InitializeResources();
            globalStorageSpace = 0;

            storage = MyGrid.GetBuildings<IStorage>(q => q != null);
            JobQueue jQ = SceneRefs.JobQueue;
            foreach (IStorage _s in storage)
            {
                jQ.storages.Add(_s);
                globalStorageSpace += _s.LocalResources.capacity - _s.LocalResources.Sum();
                resDataSource.GlobalResources.Manage(_s.LocalResources, true);
            }
            foreach(Chunk chunk in MyGrid.chunks)
            {
                resDataSource.GlobalResources.Manage(chunk.LocalRes, true);
            }
            resDataSource.UIUpdate(nameof(ResourceDisplay.GlobalResources));
            resDataSource.UIUpdate(nameof(ResourceDisplay.Money));
        }
        catch (Exception e)
        {
            Debug.LogError("Idiote!, zapomel jsi zadat nejaky parametr do 'Resources'");
            Debug.LogError(e);
        }
    }
    #endregion

    #region UI Updates
    /// <summary>
    /// Updates Global resource(JUST UI, and comparing) and forces an update.
    /// </summary>
    /// <param name="cost"><inheritdoc cref="ManageRes(Resource, Resource, float)" path="/param[@name='source']"/></param>
    /// <param name="mod"><inheritdoc cref="ManageRes(Resource, Resource, float)" path="/param[@name='mod']"/></param>
    public static void UpdateResource(Resource cost, bool add)
    {
        resDataSource.GlobalResources.Manage(cost, add);
        resDataSource.UIUpdate(nameof(ResourceDisplay.GlobalResources));
    }
    #endregion

    #region Resource moving


    /// <summary>
    /// moves resource from "source" to "destination"
    /// </summary>
    /// <param name="destination">resource to add to</param>
    /// <param name="source">resource to remove from</param>
    /// <param name="diff">resource to exchange</param>
    /// <param name="ammountToTransfer">ammount to transfer(how many resources to be moved this tick)</param>
    /// <param name="ingoreCapacity">Used when constructing to eliminate the resource cap.</param>
    /// <returns>true = continue exchange, false = stop exchange</returns>
    public static bool MoveRes(
        CapacityResource destination,
        Resource source,
        Resource diff,
        int ammountToTransfer,
        bool ingoreCapacity = false)
    {
        try
        {
            for (int i = 0; i < diff.types.Count; i++)
            {
                if (diff.ammounts[i] == 0)
                {
                    diff.ammounts.RemoveAt(i);
                    diff.types.RemoveAt(i);
                    continue;
                }
                int dIndex = destination.types.IndexOf(diff.types[i]);
                int sIndex = source.types.IndexOf(diff.types[i]);
                if (sIndex == -1 || source.ammounts[sIndex] == 0)
                {
                    Debug.LogWarning($"source is missing: {diff.ammounts[i]} units of {diff.types[i]}");
                    continue;
                }
                if (dIndex == -1)
                {
                    dIndex = destination.types.Count;
                    destination.ammounts.Add(0);
                    destination.types.Add(diff.types[i]);
                }
                // can Transfer setting
                int canTransfer = source.ammounts[sIndex] > diff.ammounts[i]
                    ? diff.ammounts[i] : source.ammounts[sIndex];
                if (!ingoreCapacity)
                    canTransfer = canTransfer > destination.FreeSpace
                        ? destination.FreeSpace
                        : canTransfer;

                if (ammountToTransfer > -1)
                {
                    canTransfer = canTransfer > ammountToTransfer
                        ? ammountToTransfer : canTransfer;
                }

                // moving the resource
                if (canTransfer > 0)
                {
                    destination.ammounts[dIndex] += canTransfer;
                    source.ammounts[sIndex] -= canTransfer;
                    ammountToTransfer -= canTransfer;
                    diff.ammounts[i] -= canTransfer;
                    if (ammountToTransfer == 0)
                        return true; // max transfer this tick reached wait for the next one
                }
                if (destination.FreeSpace < 1)
                {
                    return false; // leave this storage and find another one
                }
            }
            return false; // leave this storage, either there's no diff or source
        }
        catch (Exception e)
        {
            Debug.LogError($"{destination}, {e}");
            return false;
        }

    }
    #endregion

    #region Job finding
    /// <summary>
    /// Finds the closest stockpite with needed resources, if found some, orders to move to it.
    /// </summary>
    /// <param name="human">human to assign</param>
    /// <param name="building">building to assign to, and take diff</param>
    /// <param name="j">job to cancel if all requested resource have been requested</param>
    public static bool FindResources(Human human, Building building, JobState j)
    {
        JobQueue jQ = SceneRefs.JobQueue;
        Resource diff = building.GetDiff(human.Inventory);
        List<StorageObject> stores = MyGrid.chunks.Union(jQ.pickupNeeded.Union(jQ.storages.Cast<Building>())).ToList();
        if (stores.Count > 0)
        {
            List<ClickableObject> filtered = ResCmp(diff, stores, true, human.Inventory.capacity - human.Inventory.Sum());
            if (filtered.Count > 0)
            {
                JobData job = PathFinder.FindPath(filtered, human);
                if (job.interest && PathFinder.FindPath(new() { building }, human).interest != null)
                {
                    human.destination = building;
                    job.job = JobState.Pickup;
                    StorageObject sResource = job.interest as StorageObject;
                    CapacityResource resource = new(human.Inventory.capacity - human.Inventory.Sum());
                    Resource future = sResource.LocalRes.Future(true);

                    MoveRes(resource, future, diff, -1);
                    sResource.RequestRes(resource, human, -1);
                    human.destination.RequestRes(new(resource), human, 1);
                    human.SetJob(job);
                    human.lookingForAJob = false;
                    if (diff.Sum() == 0)
                    {
                        //diff = diff;
                        //human.transform.parent.parent.GetComponent<JobQueue>().CancelJob(j, human.jData.interest);
                    }
                    return true;
                }
                human.SetJob(job);
            }
        }
        return false;
    }

    /// <summary>
    /// find only where to store, not how to get there
    /// </summary>
    /// <param name="r"></param>
    /// <param name="h"></param>
    public static void FindStorage(Resource r, Human h)
    {
        List<IStorage> storages = FilterStorages(r, h, true);
        List<ClickableObject> objects = storages.Select(
            q => ((MonoBehaviour)q).GetComponent<ClickableObject>()).ToList();
        h.destination = PathFinder.FindPath(objects, h).interest?.GetComponent<Building>();
    }

    /// <summary>
    /// find and keep path to the storage
    /// </summary>
    /// <param name="h"></param>
    public static void FindStorage(Human h)
    {
        if (h.Inventory.Sum() > 0)
        {
            List<IStorage> storages = FilterStorages(h.Inventory, h, true);
            JobData job = PathFinder.FindPath(storages.Cast<ClickableObject>().ToList(), h);
            if (job.interest)
            {
                job.interest.GetComponent<StorageObject>().RequestRes(h.Inventory, h, 1);
                h.destination = (Building)job.interest;
                job.job = JobState.Supply;
                h.SetJob(job);
                return;
            }
        }
        h.SetJob(JobState.Free);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="r"></param>
    /// <param name="h"></param>
    /// <param name="perfect"></param>
    /// <returns></returns>
    static List<IStorage> FilterStorages(Resource r, Human h, bool perfect)
    {
        List<IStorage> storages = SceneRefs.JobQueue.storages.ToList();
        int wantToStore = r.Sum();
        for (int i = storages.Count - 1; i >= 0; i--)
        {
            int spaceToStore = storages[i].LocalResources.capacity - storages[i].LocalResources.Future().Sum();
            if (spaceToStore <= 0)
                continue;
            for (int j = 0; j < r.types.Count; j++)
            {
                int x = storages[i].LocalResources.types.IndexOf(r.types[j]);
                if(x > -1)
                {
                    if (storages[i].CanStore[x] == true)
                    {
                        if (perfect)
                        {
                            if (wantToStore <= spaceToStore)
                                continue;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                
                storages.RemoveAt(i);
                break;
            }
        }
        if (storages.Count == 0 && perfect)
        {
            return FilterStorages(r, h, false);
        }
        return storages;
    }
    #endregion

    #region Comparing
    /// <summary>
    /// Filters storages, first with those that have all that's needed, if none found try atleast part of the diff
    /// </summary>
    /// <param name="diff">resource to find</param>
    /// <param name="stores">list of storages to go filter</param>
    /// <param name="perfect">true = has all the resources</param>
    /// <returns></returns>
    static List<ClickableObject> ResCmp(Resource diff, List<StorageObject> stores, bool perfect, int resCap)
    {
        List<ClickableObject> storages = new();
        for (int i = 0; i < stores.Count; i++)
        {
            Resource future = stores[i].LocalRes.Future(true);
            for (int j = 0; j < diff.types.Count; j++)
            {
                int index = future.types.IndexOf(diff.types[j]);
                if (index > -1)
                {
                    if (perfect)
                    {
                        if (future.ammounts[index] >= diff.ammounts[j] || future.ammounts[index] >= resCap)
                        {
                            if (j == diff.types.Count - 1)
                            {
                                storages.Add(stores[i]);
                            }
                            continue;
                        }
                    }
                    else
                    {
                        if (future.ammounts[index] > 0)
                            storages.Add(stores[i]);
                    }
                }
                break;
            }
        }
        if (storages.Count > 0)
            return storages;
        else if (perfect)
            return ResCmp(diff, stores, false, resCap);
        else
            return new();
    }

    /// <summary>
    /// Compares against global resources.
    /// </summary>
    /// <param name="cost">Asking cost.</param>
    /// <returns>If the cost can be afforded.</returns>
    public static bool CanAfford(MoneyResource cost)
    {
        return cost.Money <= Money && resDataSource.GlobalResources.Diff(cost).Sum() == 0;
    }
    #endregion

    #region Magical actions
    /// <summary>
    /// Removes one unit of <see cref="ResourceType.Food"/> from a random storage.
    /// </summary>
    /// <param name="human">Human that is effected by result.</param>
    public static void EatFood(Human human)
    {
        IStorage store = storage.FirstOrDefault(q => q.LocalResources.ammounts[q.LocalResources.Future(true).types.IndexOf(ResourceType.Food)] > 0);
        if (store != null)
        {
            store.DestroyResource(ResourceType.Food, 1);
            UpdateResource(new Resource(new() { ResourceType.Food }, new() { 1 }), false);
            human.ModifyEfficiency(ModType.Food, true);
        }
        else
        {
            human.ModifyEfficiency(ModType.Food, false);
        }
    }

    /// <summary>
    /// Passive production or trade, just resources that come from the outside and "Magicly spawn".<br/>
    /// Deposited to main elevator regardles of capacity.
    /// </summary>
    /// <param name="resource">Resources to add.</param>
    public static void DeliverToElevator(Resource resource)
    {
        IStorage store = MyGrid.GetLevelElevator(0);
        if (store != null)
        {
            UpdateResource(resource, true);
            MoveRes(store.LocalResources, resource, resource, resource.Sum());
        }
        else
        {
            Debug.LogError("find a way to store over capacity.");
        }
    }

    /// <summary>
    /// Paying for trade or outposts, takes resources from any storage.
    /// </summary>
    /// <param name="cost">Cost to pay.</param>
    static void RemoveFromStorageGlobal(Resource cost)
    {
        UpdateResource(cost, false);
        Resource toRemove = new(cost);
        for (int i = 0; i < storage.Count; i++)
        {
            Resource diff = storage[i].LocalResources.Future(true).Diff(toRemove);
            storage[i].LocalResources.Manage(diff, false);
            toRemove.Manage(diff, false, removeEmpty: true);
            if (toRemove.types.Count == 0)
                break;
        }
    }

    /// <summary>
    /// Changes the ammount of money.
    /// </summary>
    /// <param name="change">Ammount to add(if negative subtracts).</param>
    public static void ManageMoneyGlobal(int change)
    {
        resDataSource.Money += change;
    }

    /// <summary>
    /// Removes resources from storages globaly.
    /// And pays the money cost from <paramref name="cost"/>.capacity.
    /// </summary>
    /// <param name="cost">Resource and money cost.</param>
    public static void PayCostGlobal(MoneyResource cost)
    {
        PayCostGlobal(cost, +cost.Money);
    }

    /// <summary>
    /// Removes resources from storages globaly.
    /// And pays the money cost from <paramref name="moneyCost"/>.
    /// </summary>
    /// <param name="cost">Resource cost.</param>
    /// <param name="moneyCost">Money cost - needs to be positive.</param>
    public static void PayCostGlobal(Resource cost, int moneyCost)
    {
        RemoveFromStorageGlobal(cost);
        if (moneyCost < 0)
        {
            Debug.LogError("carefull the cost is negative, converting to positive");
            moneyCost = -moneyCost;
        }
        ManageMoneyGlobal(-moneyCost);
    }
    #endregion
}
