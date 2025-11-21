using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Used with <see cref="Building"/>s for resources that can be modified using stats.
/// Has a base resource that stores the base from which the resource is actualy calculated.
/// </summary>
[Serializable]
public class ModifiableResource : Resource, IModifiable
{
    /// <summary>Template resource</summary>
    [SerializeField] protected Resource baseResource = new Resource();

#if UNITY_EDITOR
    /// <inheritdoc cref="baseResource"/>
    public Resource EditorResource => baseResource;
#endif

    /// <summary>Current resource modifier(multiplier)</summary>
    public ModValue Modifier { get => mod; set => mod = value; }
    [SerializeField] ModValue mod;


    public ModifiableResource() : base() { }
    public ModifiableResource(ResourceSave save) : base(save) { }
    public ModifiableResource(Resource resAmmount) 
    {
        baseResource.types = resAmmount.types.ToList();
        baseResource.ammounts = resAmmount.ammounts.ToList();
        Init();

    }
    public ModifiableResource(List<ResourceType> types, List<int> ammounts) : base(types, ammounts) { }

    /// <summary>Cycles though the base resource and recalculates the current resource.</summary>
    public void RecalculateMod()
    {
        for (int x = 0; x < baseResource.ammounts.Count; x++)
        {
            ammounts[x] = Mathf.RoundToInt(baseResource.ammounts[x] * Modifier.percentMod) + Modifier.absoluteMod;
        }
    }

    /// <summary>Sets the <see cref="resourceModifier"/> to 1 and copies <see cref="baseResource"/>to the current one</summary>
    public virtual void Init()
    {
        types = baseResource.types.ToList();
        ammounts = baseResource.ammounts.ToList();
        Modifier = new();
        RecalculateMod();
    }/*

    public Resource RemoveFluids()
    {
        Resource fluids = new();
        for(int i = baseResource.types.Count - 1; i > -1; i--)
        {
            if (baseResource.types[i].)
        }
        return fluids;
    }*/
}