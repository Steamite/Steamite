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
    static IStorage[] storage;
    /// <summary>Reference to global resource storage counter.</summary>
    public static ResourceDisplay resDisplay;
    #endregion

    #region Getters
    /// <summary>Reference to global money.</summary>
    public static int Money => resDisplay.Money;
    #endregion

    #region Init
    /// <summary>
    /// Starting function for the resource system.
    /// </summary>
    /// <param name="setupStorages"></param>
    public static void ActivateResources(bool setupStorages)
    {
        // Update text, or display error
        try
        {
            resDisplay = SceneRefs.BottomBar.GetComponent<ResourceDisplay>();
            Resource resource = resDisplay.InitializeResources(setupStorages);
            globalStorageSpace = 0;

            storage = MyGrid.buildings.Select(q => q.GetComponent<IStorage>()).Where(q => q != null).ToArray();
            JobQueue jQ = SceneRefs.jobQueue;
            foreach (IStorage _s in storage)
            {
                if (setupStorages)
                {
                    _s.SetupStorage(resource, jQ);
                }
                else
                    jQ.storages.Add(_s);
                globalStorageSpace += _s.LocalResources.stored.capacity - _s.LocalResources.stored.ammount.Sum();
                ManageRes(resource, _s.LocalResources.stored, 1);
            }
            resDisplay.UIUpdate(nameof(ResourceDisplay.GlobalResources));
            resDisplay.UIUpdate(nameof(ResourceDisplay.Money));
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
    public static void UpdateResource(Resource cost, int mod)
    {
        ManageRes(resDisplay.GlobalResources, cost, mod);
        resDisplay.UIUpdate(nameof(ResourceDisplay.GlobalResources));
    }
    #endregion

    #region Resource moving


    /// <summary>
    /// Adds or removes resources in destination.
    /// </summary>
    /// <param name="destination">What will change</param>
    /// <param name="source">Change ammount</param>
    /// <param name="mod">1 = add, -1 = remove</param>
    public static void ManageRes(Resource destination, Resource source, float mod) // destination is the resource/removed from; source ammount to be transfered
    {
        try
        {
            if (destination.ammount.Sum() == 0 && mod == 1)
            {
                destination.type = source.type.ToList();
                destination.ammount = source.ammount.ToList();
                return;
            }
            for (int i = 0; i < source.type.Count; i++)
            {
                int j = destination.type.IndexOf(source.type[i]);
                if (j > -1)
                {
                    destination.ammount[j] += Mathf.FloorToInt(source.ammount[i] * mod);
                    continue;
                }
                else if (mod > -1)
                {
                    if (source.ammount[i] > 0)
                    {
                        destination.ammount.Add(Mathf.FloorToInt(source.ammount[i] * mod));
                        destination.type.Add(source.type[i]);
                    }
                    continue;
                }
                Debug.LogError("Not found!");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e + "\n");
            Debug.Log(string.Join(';', destination) + "\n");
            Debug.Log(string.Join(';', source) + "\n");
            Debug.Log(mod);
        }
    }

    /// <summary>
    /// moves resource from "source" to "destination"
    /// </summary>
    /// <param name="destination">resource to add to</param>
    /// <param name="source">resource to remove from</param>
    /// <param name="diff">resource to exchange</param>
    /// <param name="ammountToTransfer">ammount to transfer(how many resources to be moved this tick)</param>
    /// <returns>true = continue exchange, false = stop exchange</returns>
    public static bool MoveRes(Resource destination, Resource source, Resource diff, int ammountToTransfer)
    {
        try
        {
            for (int i = 0; i < diff.type.Count; i++)
            {
                if (diff.ammount[i] == 0)
                {
                    diff.ammount.RemoveAt(i);
                    diff.type.RemoveAt(i);
                    continue;
                }
                int dIndex = destination.type.IndexOf(diff.type[i]);
                int sIndex = source.type.IndexOf(diff.type[i]);
                if (sIndex == -1 || source.ammount[sIndex] == 0)
                {
                    Debug.LogWarning($"source is missing: {diff.ammount[i]} units of {diff.type[i]}");
                    continue;
                }
                if (dIndex == -1)
                {
                    dIndex = destination.type.Count;
                    destination.ammount.Add(0);
                    destination.type.Add(diff.type[i]);
                }
                // can Transfer setting
                int canTransfer = source.ammount[sIndex] > diff.ammount[i]
                    ? diff.ammount[i] : source.ammount[sIndex];
                if (destination.capacity > -1)
                {
                    canTransfer = canTransfer > destination.capacity - destination.ammount.Sum()
                        ? destination.capacity - destination.ammount.Sum() : canTransfer;
                }
                if (ammountToTransfer > -1)
                {
                    canTransfer = canTransfer > ammountToTransfer
                        ? ammountToTransfer : canTransfer;
                }

                // moving the resource
                if (canTransfer > 0)
                {
                    destination.ammount[dIndex] += canTransfer;
                    source.ammount[sIndex] -= canTransfer;
                    ammountToTransfer -= canTransfer;
                    diff.ammount[i] -= canTransfer;
                    if (ammountToTransfer == 0)
                        return true; // max transfer this tick reached wait for the next one
                }
                if (destination.capacity > 0 && destination.ammount.Sum() == destination.capacity)
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
        JobQueue jQ = GameObject.FindWithTag("Humans").GetComponent<JobQueue>();
        Resource diff = building.GetDiff(human.Inventory);
        List<StorageObject> stores = MyGrid.chunks.Union(jQ.pickupNeeded.Union(jQ.storages.Cast<Building>())).ToList();
        if (stores.Count > 0)
        {
            List<ClickableObject> filtered = ResCmp(diff, stores, true, human.Inventory.capacity - human.Inventory.ammount.Sum());
            if (filtered.Count > 0)
            {
                JobData job = PathFinder.FindPath(filtered, human);
                if (job.interest && PathFinder.FindPath(new() { building }, human).interest != null)
                {
                    human.destination = building;
                    job.job = JobState.Pickup;
                    human.SetJob(job);
                    StorageObject sResource = job.interest.GetComponent<StorageObject>();
                    Resource resource = new();
                    resource.capacity = human.Inventory.capacity - human.Inventory.ammount.Sum();
                    Resource future = sResource.LocalRes.Future(true);

                    MoveRes(resource, future, diff, -1);
                    sResource.RequestRes(resource, human, -1);
                    human.destination.RequestRes(resource.Clone(), human, 1);
                    human.ChangeAction(HumanActions.Move);
                    human.lookingForAJob = false;
                    if (diff.ammount.Sum() == 0)
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
        h.destination = PathFinder.FindPath(storages.Select(q => ((MonoBehaviour)q).GetComponent<ClickableObject>()).ToList(), h).interest.GetComponent<Building>();
        if (h.destination)
        {
            h.destination.RequestRes(r, h, 1);
        }
    }

    /// <summary>
    /// find and keep path to the storage
    /// </summary>
    /// <param name="h"></param>
    public static void FindStorage(Human h)
    {
        List<IStorage> storages = FilterStorages(h.Inventory, h, true);
        JobData job = PathFinder.FindPath(storages.Cast<ClickableObject>().ToList(), h);
        if (job.interest)
        {
            job.interest.GetComponent<StorageObject>().RequestRes(h.Inventory, h, 1);
            h.destination = (Building)job.interest;
            job.job = JobState.Supply;
            h.ChangeAction(HumanActions.Move);
            h.SetJob(job);
        }
        else
        {
            h.SetJob(JobState.Free, null, null);
        }
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
        JobQueue jQ = h.transform.parent.parent.GetComponent<JobQueue>();
        List<IStorage> storages = jQ.storages.ToList();
        int wantToStore = r.ammount.Sum();
        for (int i = storages.Count - 1; i >= 0; i--)
        {
            int spaceToStore = storages[i].LocalResources.stored.capacity - storages[i].LocalResources.Future().ammount.Sum();
            if (spaceToStore <= 0)
                continue;
            for (int j = 0; j < r.type.Count; j++)
            {
                if (storages[i].CanStore[(int)r.type[j]] == true)
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
            for (int j = 0; j < diff.type.Count; j++)
            {
                int index = future.type.IndexOf(diff.type[j]);
                if (index > -1)
                {
                    if (perfect)
                    {
                        if (future.ammount[index] >= diff.ammount[j] || future.ammount[index] >= resCap)
                        {
                            if (j == diff.type.Count - 1)
                            {
                                storages.Add(stores[i]);
                            }
                            continue;
                        }
                    }
                    else
                    {
                        if (future.ammount[index] > 0)
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
    /// Compares both paramets and returns what is needed, but not available.
    /// </summary>
    /// <param name="cost">Needed resources.</param>
    /// <param name="storA">Available resources.</param>
    /// <returns>Resources that are missing.</returns>
    public static Resource DiffRes(Resource cost, Resource storA)
    {
        Resource ret = new();
        for (int i = 0; i < cost.type.Count; i++)
        {
            int minus; // total stored
            int j = storA.type.IndexOf(cost.type[i]);
            minus = j > -1 ? storA.ammount[j] : 0;
            if (minus > 0)
            {
                int exchange = DiffInPositive(cost.ammount[i], minus);
                if (exchange > 0)
                {
                    ret.ammount.Add(exchange);
                    ret.type.Add(cost.type[i]);
                }
            }
            else
            {
                ret.ammount.Add(cost.ammount[i]);
                ret.type.Add(cost.type[i]);
            }
        }
        return ret;
    }

    /// <summary>
    /// <inheritdoc cref="DiffRes(Resource, Resource)"/>
    /// </summary>
    /// <param name="cost"><inheritdoc cref="DiffRes(Resource, Resource)" path="/param[@name='cost']"/></param>
    /// <param name="storA"><inheritdoc cref="DiffRes(Resource, Resource)" path="/param[@name='storA']"/></param>
    /// <param name="storB">Second available resource.</param>
    /// <returns></returns>
    public static Resource DiffRes(Resource cost, Resource storA, Resource storB)
    {
        Resource ret = new();
        for (int i = 0; i < cost.type.Count; i++)
        {
            int minus; // total stored
            int j = storA.type.IndexOf(cost.type[i]);
            minus = j > -1 ? storA.ammount[j] : 0;
            j = storB.type.IndexOf(cost.type[i]);
            minus += j > -1 ? storB.ammount[j] : 0;
            if (minus > 0)
            {
                int exchange = DiffInPositive(cost.ammount[i], minus);
                if (exchange > 0)
                {
                    ret.ammount.Add(exchange);
                    ret.type.Add(cost.type[i]);
                }
            }
            else
            {
                ret.ammount.Add(cost.ammount[i]);
                ret.type.Add(cost.type[i]);
            }
        }
        return ret;
    }

    /// <summary>
    /// Compares values and returns the difference between <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    static int DiffInPositive(int a, int b)
    {
        int diff = a - b;
        return diff < 0 ? 0 : diff;
    }

    /// <summary>
    /// Compares against global resources.
    /// </summary>
    /// <param name="cost">Asking cost.</param>
    /// <returns>If the cost can be afforded.</returns>
    public static bool CanAfford(Resource cost)
    {
        return cost.capacity <= Money && DiffRes(cost, resDisplay.GlobalResources).ammount.Sum() == 0;
    }
    #endregion

    #region Magical actions
    /// <summary>
    /// Removes one unit of <see cref="ResourceType.Food"/> from a random storage.
    /// </summary>
    /// <param name="human">Human that is effected by result.</param>
    public static void EatFood(Human human)
    {
        IStorage store = storage.FirstOrDefault(q => q.LocalResources.stored.ammount[q.LocalResources.Future(true).type.IndexOf(ResourceType.Food)] > 0);
        if (store != null)
        {
            store.DestroyResource(ResourceType.Food, 1);
            UpdateResource(new Resource(new() { ResourceType.Food }, new() { 1 }), -1);
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
        IStorage store = Elevator.main;
        if (store != null)
        {
            MoveRes(store.LocalResources.stored, resource, resource, resource.ammount.Sum());
            UpdateResource(resource, 1);
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
        UpdateResource(cost, -1);
        for (int i = 0; i < storage.Length; i++)
        {
            Resource diff = DiffRes(cost, storage[i].LocalResources.Future(true));
            for (int j = cost.type.Count - 1; j >= 0; j--)
            {
                int x = diff.type.IndexOf(cost.type[j]);
                if (x == -1)
                {
                    storage[i].LocalResources.stored[cost.type[j]] -= cost.ammount[j];
                    cost.ammount.RemoveAt(j);
                    cost.type.RemoveAt(j);
                }
                else
                {
                    int change = cost.ammount[j] - diff.ammount[x];
                    storage[i].LocalResources.stored[cost.type[j]] -= change;
                    cost.ammount[j] -= change;
                }
            }
        }
    }

    /// <summary>
    /// Changes the ammount of money.
    /// </summary>
    /// <param name="change">Ammount to add(if negative subtracts).</param>
    public static void ManageMoneyGlobal(int change)
    {
        resDisplay.Money += change;
    }

    /// <summary>
    /// Removes resources from storages globaly.
    /// And pays the money cost from <paramref name="cost"/>.capacity.
    /// </summary>
    /// <param name="cost">Resource and money cost.</param>
    public static void PayCostGlobal(Resource cost)
    {
        RemoveFromStorageGlobal(cost);
        ManageMoneyGlobal(-cost.capacity);
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
