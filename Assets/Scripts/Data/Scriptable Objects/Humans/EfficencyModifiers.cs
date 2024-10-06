using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Efficiency Modifiers", menuName = "ScriptableObjects/Efficiency Holder", order = 2)]
public class EfficencyModifiers : ScriptableObject
{
    [SerializeField]
    List<EfficiencyMod> modifiers = new();

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
}
