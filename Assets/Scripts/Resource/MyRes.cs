using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;

[Serializable]
public enum ResourceType
{
    Coal,
    Metal,
    Stone,
    Food,
    Wood,
}

public static class MyRes
{
    // fill with ALL of active resource objects!!!
    public static int globalStorageSpace;
    static Storage[] storage;
    static ResourceDisplay resDisplay;

    public static int Money
    {
        get => resDisplay.Money;
    }

    /// <summary>
    /// Starting function for the resource system.
    /// </summary>
    /// <param name="setupStorages"></param>
    public static void ActivateResources(bool setupStorages)
    {
        // Update text, or display error
        try
        {
            resDisplay = SceneRefs.stats.GetComponent<ResourceDisplay>();
            Resource resource = resDisplay.InitializeResources(setupStorages);
            globalStorageSpace = 0;

            storage = MyGrid.buildings.Select(q => q.GetComponent<Storage>()).Where(q => q != null).ToArray();
            JobQueue jQ = SceneRefs.humans.GetComponent<JobQueue>();
            foreach (Storage _s in storage)
            {
                if (setupStorages)
                {
                    _s.SetupStorage(resource, jQ);
                }
                globalStorageSpace += _s.LocalRes.stored.capacity - _s.LocalRes.stored.ammount.Sum();
                ManageRes(resource, _s.LocalRes.stored, 1);
            }
            Transform resourceInfo = GameObject.Find("Resource Info").gameObject.transform.transform;
            resDisplay.UIUpdate(nameof(ResourceDisplay.GlobalResources));
            resDisplay.UIUpdate(nameof(ResourceDisplay.Money));
        }
        catch (Exception e)
        {
            Debug.LogError("Idiote!, zapomel jsi zadat nejaky parametr do 'Resources'");
            Debug.LogError(e);
        }
    }
    public static void ManageMoney(int change)
    {
        resDisplay.Money += change;
    }

    /// <summary>
    /// for generic resources
    /// </summary>
    /// <param name="r"></param>
    /// <returns>data to display in the info window</returns>
    public static string GetDisplayText(Resource r)
    {
        string s = "";
        for (int i = 0; i < r.type.Count && i < r.ammount.Count; i++)
        {
            if (r.ammount[i] > 0)
                s += $"{Enum.GetName(typeof(ResourceType), r.type[i])}: {r.ammount[i]}\n";
        }
        if (s.Length > 0)
            return s.Remove(s.Length - 1);
        else
            return "Nothing";
    }


    /// <summary>
    /// adds or removes resources in destination
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
        List<StorageObject> stores = MyGrid.chunks.Union(jQ.pickupNeeded.Union(jQ.storages)).ToList();
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
                    StorageResource sResource = job.interest.GetComponent<StorageObject>().LocalRes;
                    Resource resource = new();
                    resource.capacity = human.Inventory.capacity - human.Inventory.ammount.Sum();
                    Resource future = sResource.Future(true);

                    MoveRes(resource, future, diff, -1);
                    sResource.AddRequest(resource, human, -1);
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
    public static int DiffInPositive(int a, int b)
    {
        int diff = a - b;
        return diff < 0 ? 0 : diff;
    }
    // Changes the state of a resources
    public static void UpdateResource(Resource cost, int mod)
    {
        ManageRes(resDisplay.GlobalResources, cost, mod);
        resDisplay.UIUpdate(nameof(ResourceDisplay.GlobalResources));
    }

    /// <summary>
    /// find only where to store, not how to get there
    /// </summary>
    /// <param name="r"></param>
    /// <param name="h"></param>
    public static void FindStorage(Resource r, Human h)
    {
        List<Storage> storages = FilterStorages(r, h, true);
        h.destination = PathFinder.FindPath(storages.Select(q => q.GetComponent<ClickableObject>()).ToList(), h).interest.GetComponent<Building>();
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
        List<Storage> storages = FilterStorages(h.Inventory, h, true);
        JobData job = PathFinder.FindPath(storages.Cast<ClickableObject>().ToList(), h);
        if (job.interest)
        {
            job.interest.GetComponent<StorageObject>().RequestRes(h.Inventory, h, 1);
            h.destination = (Building)job.interest;
            job.job = JobState.Supply;
            h.ChangeAction(HumanActions.Move);
            h.SetJob(job);
        }
    }
    static List<Storage> FilterStorages(Resource r, Human h, bool perfect)
    {
        JobQueue jQ = h.transform.parent.parent.GetComponent<JobQueue>();
        List<Storage> storages = jQ.storages.ToList();
        int wantToStore = r.ammount.Sum();
        for (int i = storages.Count - 1; i >= 0; i--)
        {
            int spaceToStore = storages[i].LocalRes.stored.capacity - storages[i].LocalRes.Future().ammount.Sum();
            if (spaceToStore <= 0)
                continue;
            for (int j = 0; j < r.type.Count; j++)
            {
                if (storages[i].canStore[(int)r.type[j]] == true)
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

    public static void EatFood(Human human)
    {
        Storage store = storage.FirstOrDefault(q => q.LocalRes.stored.ammount[q.LocalRes.Future(true).type.IndexOf(ResourceType.Food)] > 0);
        if (store)
        {
            store.DestroyResource(ResourceType.Food, 1);
            UpdateResource(new Resource(new() { ResourceType.Food }, new() { 1 }), -1);
            human.hasEaten = true;
            human.Efficiency.ManageModifier(ModType.Food, true);
        }
        else
        {
            human.hasEaten = false;
            human.Efficiency.ManageModifier(ModType.Food, false);
        }
    }
    public static void DeliverToElevator(Resource resource)
    {
        Storage store = storage.FirstOrDefault(q => ((Elevator)q).main);
        if (store)
        {
            MoveRes(store.LocalRes.stored, resource, resource, resource.ammount.Sum());
        }
    }

    public static void TakeFromGlobalStorage(Resource r)
    {
        UpdateResource(r, -1);
        for (int i = 0; i < storage.Length; i++)
        {
            Resource diff = DiffRes(r, storage[i].LocalRes.Future(true));
            for(int j = r.type.Count-1; j >= 0; j--)
            {
                int x = diff.type.IndexOf(r.type[j]);
                if(x == -1)
                {
                    storage[i].LocalRes.stored.ammount[storage[i].LocalRes.stored.type.IndexOf(r.type[j])] -= r.ammount[j];
                    r.ammount.RemoveAt(j);
                    r.type.RemoveAt(j);
                }
                else
                {
                    int change = r.ammount[j] - diff.ammount[x];
                    storage[i].LocalRes.stored.ammount[storage[i].LocalRes.stored.type.IndexOf(r.type[j])] -= change;
                    r.ammount[j] -= change;
                }
            }
        }
    }

    public static bool CanAfford(Resource cost)
    {
        return DiffRes(cost, resDisplay.GlobalResources).ammount.Sum() == 0;
    }
}
