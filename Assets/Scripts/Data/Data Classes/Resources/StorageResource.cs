using System.Collections.Generic;
using System;

[Serializable]
public class StorageResource
{
    public Resource stored;
    public List<Resource> requests;
    public List<Human> carriers;
    private List<int> carrierIDs;
    public List<int> mods;
    public StorageResource()
    {
        stored = new();
        requests = new();
        carriers = new();
        mods = new();
    }
    public StorageResource(StorageResSave resSave)
    {
        stored = resSave.stored;
        requests = resSave.requests;
        mods = resSave.mod;
        carriers = new();
        carrierIDs = resSave.carriers;
    }

    /// <summary>
    /// Adds a request for moving resources.<br/>
    /// </summary>
    /// <param name="resource">requested resource</param>
    /// <param name="human">human who requested it</param>
    /// <param name="mod">add(1) or remove(-1)</param>
    public void AddRequest(Resource resource, Human human, int mod)
    {
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
        requests.RemoveAt(index);
        carriers.RemoveAt(index);
        mods.RemoveAt(index);
    }

    public void ReassignCarriers(bool assign = true)
    {
        if (carriers.Count > 0)
        {
            if (assign)
            {
                carriers[0].SetJob(JobState.Deconstructing);
                carriers[0].ChangeAction(HumanActions.Demolish);
            }
            for (int i = carriers.Count - 1; i > 0; i++)
            {
                HumanActions.LookForNew(carriers[i]);
                RemoveRequest(carriers[i]);
            }
        }
    }
    /// <summary>
    /// returns future resources
    /// </summary>
    /// <param name="_stored"> true = return only resource available right now (removes the reserved ones)</param>
    /// <returns></returns>
    public Resource Future(bool _stored = false)
    {
        Resource futureRes = stored.Clone();
        for (int i = 0; i < requests.Count; i++)
        {
            int mod = mods[i];
            Resource r = requests[i];
            for (int j = 0; j < r.type.Count; j++)
            {
                int index;
                int toAdd = _stored ? (mods[i] == 1 ? 0 : r.ammount[j] * mod) : r.ammount[j] * mod;
                if ((index = futureRes.type.IndexOf(r.type[j])) == -1)
                {
                    futureRes.type.Add(r.type[j]);
                    futureRes.ammount.Add(toAdd);
                }
                else
                {
                    futureRes.ammount[index] += toAdd;
                }
            }
        }
        return futureRes;
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
}