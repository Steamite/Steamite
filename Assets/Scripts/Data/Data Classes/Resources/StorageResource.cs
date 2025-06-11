using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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
        type = res.type.ToList();
        ammount = res.ammount.ToList();
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
            futureCashe = Clone();
            for (int i = 0; i < requests.Count; i++)
            {
                int mod = mods[i];
                Resource r = requests[i];
                for (int j = 0; j < r.type.Count; j++)
                {
                    int index;
                    int toAdd = _stored ? (mods[i] == 1 ? 0 : r.ammount[j] * mod) : r.ammount[j] * mod;
                    if ((index = futureCashe.type.IndexOf(r.type[j])) == -1)
                    {
                        futureCashe.type.Add(r.type[j]);
                        futureCashe.ammount.Add(toAdd);
                    }
                    else
                    {
                        futureCashe.ammount[index] += toAdd;
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
            carrierIDs.Remove(index);
            if (carrierIDs.Count == 0)
                carrierIDs = null;
            if (carriers.Count <= index)
            {
                carriers.Add(h);
            }
            else
            {
                carriers.Insert(index, h);
            }
        }
    }

    public void Load(StorageResSave resSave)
    {
        type = resSave.type.ToList();
        ammount = resSave.ammount.ToList();
        requests = resSave.requests;
        mods = resSave.mod;
        carriers = new();
        carrierIDs = resSave.carriers;
    }
    #endregion
}