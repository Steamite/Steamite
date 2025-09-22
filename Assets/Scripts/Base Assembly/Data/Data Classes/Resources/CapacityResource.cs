using Newtonsoft.Json;
using System;
using Unity.Properties;
using UnityEngine;

[Serializable]
public class CapacityResource : Resource
{
    /// <summary>Capacity in this resource (-1 means there's no limit)</summary>
    [SerializeField][CreateProperty] public ModifiableInteger capacity;

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
}