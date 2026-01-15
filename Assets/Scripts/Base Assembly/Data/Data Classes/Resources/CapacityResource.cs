using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;

[Serializable]
public class CapacityResource : Resource
{
    /// <summary>Capacity in this resource (-1 means there's no limit)</summary>
    [SerializeField, CreateProperty] public ModifiableInteger capacity;

    [JsonIgnore]
    public int FreeSpace
    {
        get
        {
            if (capacity == -1)
                return int.MaxValue;
            return capacity - Sum();
        }
    }


    public CapacityResource()
    {
    }


    public CapacityResource(int baseCapacity)
    {
        capacity = new(baseCapacity);
        ((IModifiable)capacity).Init();
    }

    public CapacityResource(ResourceSave save, int _capacity) : base(save)
    {
        capacity = new(_capacity);
        ((IModifiable)capacity).Init();
    }


    public override bool Equals(object _resource)
    {
        if (_resource is not CapacityResource)
            return false;
        return base.Equals(_resource);
    }



    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), types, ammounts, capacity, FreeSpace);
    }

    public bool HasSpace(Resource resource, bool checkTypes = false)
    {
        bool res = ammounts.Sum() + resource.ammounts.Sum() <= +capacity;
        if (res == false || checkTypes == false)
            return res;

        foreach (var item in resource.types)
        {
            if(types.Contains(item) == false)
                return false;
        }
        return true;
    }

    public void InitCapacity()
    {
        capacity.RecalculateMod();
    }

    public void ChangeFluidStorage(List<ResourceType> _types, IFluidWork work)
    {
        if (Sum() > 0)
        {
            work.StoreInNetwork(this, out bool succes, false);
        }

        List<int> tempAmmounts = new();
        List<ResourceType> tempTypes = _types.ToList();
        for (int i = 0; i < tempTypes.Count; i++)
            tempAmmounts.Add(0);

        for (int i = 0; i < types.Count; i++)
        {
            if (ammounts[i] > 0)
            {
                int x = tempTypes.IndexOf(types[i]);
                if(x == -1)
                {
                    tempTypes.Add(types[i]);
                    tempAmmounts.Add(ammounts[i]);
                }
                else
                {
                    tempAmmounts[x] = ammounts[i];
                }
            }
        }
        types = tempTypes;
        ammounts = tempAmmounts;
    }
}