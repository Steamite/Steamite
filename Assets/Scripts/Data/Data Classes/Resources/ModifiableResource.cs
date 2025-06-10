using Mono.Cecil;
using System;
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
    [SerializeField] Resource baseResource = new Resource();

#if UNITY_EDITOR
    /// <inheritdoc cref="baseResource"/>
    public Resource EditorResource => baseResource;
#endif

    /// <summary>Current resource modifier(multiplier)</summary>
    public ModValue Modifier { get; set; }


    public ModifiableResource(){}

    /// <summary>Cycles though the base resource and recalculates the current resource.</summary>
    public void RecalculateMod()
    {
        for (int x = 0; x < baseResource.ammount.Count; x++)
        {
            ammount[x] = Mathf.RoundToInt(baseResource.ammount[x] * Modifier.percentMod) + Modifier.absoluteMod;
        }
    }

    /// <summary>Sets the <see cref="resourceModifier"/> to 1 and copies <see cref="baseResource"/>to the current one</summary>
    public void Init()
    {
        type = baseResource.type.ToList();
        ammount = baseResource.ammount.ToList();
        Modifier = new();
        RecalculateMod();
    }
}