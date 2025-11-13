using Newtonsoft.Json;
using System;
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

    public bool HasSpace(Resource resource)
    {
        return ammounts.Sum() + resource.ammounts.Sum() <= +capacity;
    }
}