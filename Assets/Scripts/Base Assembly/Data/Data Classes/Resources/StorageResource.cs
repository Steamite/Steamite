using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>Helps with fulfiling resource orders and make logistics more efficient.</summary>
[Serializable]
public class StorageResource : CapacityResource
{
    #region Variables
    Resource futureCashe;
    bool casheValid = false;
    /// <summary>All resources that were requested(store && take).</summary>
    List<StorageRequest> requests;
    public List<StorageRequest> Requests => requests;
    /// <summary>ID of carriers for loading(is needed to ensure the correct aligment with <see cref="requests"/>).</summary>
    private List<int> carrierIDs;
    #endregion

    #region Constructors
    public StorageResource()
    {
        requests = new();
    }

    public StorageResource(Resource res)
    {
        types = res.types.ToList();
        ammounts = res.ammounts.ToList();
        requests = new();
    }
    #endregion

    #region Requests
    /// <summary>
    /// Adds a request for moving resources.<br/>
    /// </summary>
    /// <param name="resource">requested resource</param>
    /// <param name="human">human who requested it</param>
    /// <param name="mod">add(1) or remove(-1)</param>
    public void AddRequest(Resource resource, Human human, StorageRequestType mod)
    {
        if (resource.Sum() > 0)
            casheValid = false;
        requests.Add(new(resource, human, mod));
    }


    int IndexByHuman(Human human)
        => requests.FindIndex(q => q.carrier == human);
    public StorageRequest GetRequestByHuman(Human human)
    {
        int index = IndexByHuman(human);
        if(index == -1)
        {
            Debug.LogError("No request for this human");
            Debug.DebugBreak();
            return null;
        }
        return requests[index];
    }

    /// <summary>
    /// cancels a request
    /// </summary>
    /// <param name="human">human who requested it</param>
    public void RemoveRequest(Human human)
    {
        int index = IndexByHuman(human);
        RemoveRequestAt(index);
    }

    void RemoveRequestAt(int index, bool freeHuman = false)
    {
        if (index == -1)
            return;

        if (requests[index].resource.Sum() > 0)
            casheValid = false;
        if (freeHuman)
            requests[index].carrier.SetJob(JobState.Free);

        requests.RemoveAt(index);
    }

    /// <summary>
    /// Reassign when setting a building to deconstruction.
    /// </summary>
    /// <param name="assign">Set the first one to deconstruct the building.</param>
    public Human ReassignCarriers(JobState newState)
    {
        Human human = null;
        if (requests.Count > 0)
        {
            if (newState != JobState.Free)
            {
                human = requests[0].carrier;
                requests[0].SetRequestToAction(newState);
            }
            ClearRequests(1);
        }
        return human;
    }

    public void ClearRequests(int startIndex = 0)
    {
        for (int i = requests.Count - 1; i >= startIndex; i++)
        {
            RemoveRequestAt(i, true);
        }
    }

    /// <summary>
    /// returns future resources
    /// </summary>
    /// <param name="onlyStored"> true = return only resource available right now (removes the reserved ones)</param>
    /// <returns></returns>
    public Resource Future(bool onlyStored = false)
    {
        if (!casheValid)
        {
            futureCashe = new(this);
            for (int i = 0; i < requests.Count; i++)
            {
                StorageRequest req = requests[i];
                StorageRequestType mod = req.mod;
                Resource r = req.resource;

                for (int j = 0; j < r.types.Count; j++)
                {
                    int index;
                    int toAdd = r.ammounts[j];
                    switch (mod)
                    {
                        case StorageRequestType.Take:
                            toAdd = -toAdd;
                            break;
                        case StorageRequestType.Store:
                            if (onlyStored)
                                toAdd = 0;
                            break;
                        case StorageRequestType.Action:
                            toAdd = 0;
                            break;
                    }
                    if ((index = futureCashe.types.IndexOf(r.types[j])) == -1)
                    {
                        futureCashe.types.Add(r.types[j]);
                        futureCashe.ammounts.Add(toAdd);
                    }
                    else
                    {
                        futureCashe.ammounts[index] += toAdd;
                    }
                }
            }
            casheValid = true;
        }

        return futureCashe;
    }

    /// <summary>
    /// Links the <paramref name="h"/> using <see cref="carrierIDs"/>, which is assigned when loading.
    /// </summary>
    /// <param name="h"><see cref="Human"/> that is to be linked.</param>
    public void LinkHuman(Human h)
    {
        int index = carrierIDs.FindIndex(q => q == h.id);
        if (index > -1)
        {
            requests[index].carrier = h;
        }
    }

    public void Load(StorageResSave resSave)
    {
        types = ResFluidTypes.LoadTypeList(resSave.types);
        ammounts = resSave.ammounts;

        requests = resSave.Requests.Select(q => new StorageRequest(q)).ToList();
        carrierIDs = resSave.Requests.Select(q=> q.humanId).ToList();
    }

    public void Dump()
    {
        types.Clear();
        ammounts.Clear();
    }

    public bool HasNoCarriers()
        => requests.Count == 0;
    #endregion

    public override bool Equals(object _resource)
    {
        if (_resource is not StorageResource _res)
            return false;
        return _res.GetHashCode().Equals(GetHashCode());

        /*foreach (var item in _res.requests)
        {
            if (!requests.Contains(item))
                return false;
        }
        foreach (var item in _res.carriers)
        {
            if (!carriers.Contains(item))
                return false;
        }
        foreach (var item in _res.mods)
        {
            if (!mods.Contains(item))
                return false;
        }
        return base.Equals(_resource);*/
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(types);
        hash.Add(ammounts);
        hash.Add(capacity);
        hash.Add(FreeSpace);
        hash.Add(futureCashe);
        hash.Add(casheValid);
        hash.Add(requests);
        return hash.ToHashCode();
    }
}