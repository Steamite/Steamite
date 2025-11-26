using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Helps with fulfiling resource orders and make logistics more efficient.</summary>
[Serializable]
public class StorageResource : CapacityResource
{
    #region Variables
    Resource futureCashe;
    bool casheValid = false;
    /// <summary>All resources that were requested(store && take).</summary>
    public List<Resource> requests;
    /// <summary>All carriers for resouces.</summary>
    public List<Human> carriers;
    /// <summary>ID of carriers for loading(is needed to ensure the correct aligment with <see cref="requests"/>).</summary>
    private List<int> carrierIDs;
    /// <summary>All states of requests(-1 = take, 1 = store).</summary>
    public List<int> mods;
    #endregion

    #region Constructors
    public StorageResource()
    {
        requests = new();
        carriers = new();
        mods = new();
    }

    public StorageResource(Resource res)
    {
        types = res.types.ToList();
        ammounts = res.ammounts.ToList();
        requests = new();
        carriers = new();
        mods = new();
    }
    #endregion

    #region Requests
    /// <summary>
    /// Adds a request for moving resources.<br/>
    /// </summary>
    /// <param name="resource">requested resource</param>
    /// <param name="human">human who requested it</param>
    /// <param name="mod">add(1) or remove(-1)</param>
    public void AddRequest(Resource resource, Human human, int mod)
    {
        if (resource.Sum() > 0)
            casheValid = false;
        requests.Add(resource);
        carriers.Add(human);
        mods.Add(mod);
    }

    /// <summary>
    /// cancels a request
    /// </summary>
    /// <param name="human">human who requested it</param>
    public void RemoveRequest(Human human)
    {
        int index = carriers.IndexOf(human);
        if (index == -1)
            return;

        if (requests[index].Sum() > 0)
            casheValid = false;
        requests.RemoveAt(index);
        carriers.RemoveAt(index);
        mods.RemoveAt(index);
    }

    /// <summary>
    /// Reassign when setting a building to deconstruction.
    /// </summary>
    /// <param name="assign"></param>
    public Human ReassignCarriers(bool assign = true)
    {
        Human human = null;
        if (carriers.Count > 0)
        {
            if (assign)
            {
                carriers[0].SetJob(JobState.Deconstructing);
                human = carriers[0];
            }
            for (int i = carriers.Count - 1; i > 0; i++)
            {
                RemoveRequest(carriers[i]);
                carriers[i].SetJob(JobState.Free);
            }
        }
        return human;
    }

    /// <summary>
    /// returns future resources
    /// </summary>
    /// <param name="_stored"> true = return only resource available right now (removes the reserved ones)</param>
    /// <returns></returns>
    public Resource Future(bool _stored = false)
    {
        if (!casheValid)
        {
            futureCashe = new(this);
            for (int i = 0; i < requests.Count; i++)
            {
                int mod = mods[i];
                Resource r = requests[i];
                for (int j = 0; j < r.types.Count; j++)
                {
                    int index;
                    int toAdd = _stored ? (mods[i] == 1 ? 0 : r.ammounts[j] * mod) : r.ammounts[j] * mod;
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
            carriers[index] = h;
        }
    }

    public void Load(StorageResSave resSave)
    {
        types = ResFluidTypes.LoadTypeList(resSave.types);//.Select(q => ResFluidTypes.GetResByIndex(q)).ToList();
        ammounts = resSave.ammounts;
        requests = resSave.Requests.Select(q => new Resource(q)).ToList();
        mods = resSave.mod;
        if (mods.Count == 1 && mods[0] == 0 && requests.Count == 0)
            requests.Add(new());
        carrierIDs = resSave.carriers.ToList();
        carriers = new();
        for (int i = 0; i < carrierIDs.Count; i++)
        {
            carriers.Add(null);
        }
    }

    public void Dump()
    {
        types.Clear();
        ammounts.Clear();
    }

    public bool HasNoCarriers()
        => mods.Count == 0;
    #endregion

    public override bool Equals(object _resource)
    {
        if (_resource is not StorageResource _res)
            return false;

        foreach (var item in _res.requests)
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
        return base.Equals(_resource);
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
        hash.Add(carriers);
        hash.Add(carrierIDs);
        hash.Add(mods);
        return hash.ToHashCode();
    }
}