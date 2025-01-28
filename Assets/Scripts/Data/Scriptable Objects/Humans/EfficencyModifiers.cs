using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Holds all modifiers in game.</summary>
[CreateAssetMenu(fileName = "Efficiency Modifiers", menuName = "ScriptableObjects/Efficiency Holder", order = 2)]
public class EfficencyModifiers : ScriptableObject
{ 
    /// <summary><inheritdoc cref="EfficencyModifiers"/>.</summary>
    [SerializeField] List<EfficiencyMod> modifiers = new();

    /// <summary>
    /// Returns <paramref name="modType"/> modifier.
    /// </summary>
    /// <param name="modType">Modifier</param>
    /// <returns></returns>
    public EfficiencyMod GetModifier(ModType modType)
    {
        foreach(EfficiencyMod mod in modifiers)
        {
            if(mod.modType == modType)
            {
                return new(mod);
            }
        }
        return null;
    }

    /// <summary>
    /// For editor.
    /// </summary>
    /// <returns></returns>
    public string[] GetModifierNames()
    {
        return modifiers.Select(q => q.name).ToArray();
    }
}
