using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Class for handling efficiency of <see cref="Human"/> actions.</summary>
[Serializable]
public class Efficiency
{
    #region Variables
    /// <summary>All currently active modifiers.</summary>
    [SerializeField] List<EfficiencyMod> modifiers = new();
    /// <summary>Base efficiency without modifiers.</summary>
    [SerializeField] float baseEfficiecny = 1;
    /// <summary>Current efficiency.</summary>
    [NonSerialized] public float efficiency = 1;
    #endregion

    /// <summary>
    /// Adds or removes a modifier, then recalculates <see cref="efficiency"/>.
    /// </summary>
    /// <param name="_modType">Mod type to add.</param>
    /// <param name="improvement">Add or Remove.</param>
    public void ManageModifier(ModType _modType, bool improvement)
    {
        EfficiencyMod mod = modifiers.FirstOrDefault(q => q.modType == _modType);
        if (mod != null)
        {
            if (improvement)
            {
                if (mod.posCount > mod.count)
                    mod.count++;
                else
                    Debug.LogWarning($"Already satisfied to max. {_modType}");
            }
            else
            {
                if (mod.negCount > -mod.count)
                    mod.count--;
            }

            if (mod.count == 0)
            {
                modifiers.Remove(mod);
            }
        }
        else
        {
            mod = SceneRefs.Humans.modifiers.GetModifier(_modType);
            mod.count = improvement ? 1 : -1;
            modifiers.Add(mod);
        }
        CalculateEfficiecy();
    }
    public void SetModifier(ModType _modType, int state)
    {
        EfficiencyMod mod = modifiers.FirstOrDefault(q => q.modType == _modType);
        if (mod != null)
        {
            if(state == 0)
            {
                modifiers.Remove(mod);
                CalculateEfficiecy();
                return;
            }
        }
        else
        {
            mod = SceneRefs.Humans.modifiers.GetModifier(_modType);
            modifiers.Add(mod);
        }
        mod.count = state;
        CalculateEfficiecy();
    }

    /// <summary>Recalculates new <see cref="efficiency"/>.</summary>
    void CalculateEfficiecy()
    {
        efficiency = baseEfficiecny;
        foreach (EfficiencyMod mod in modifiers)
        {
            if (mod.count < 0)
            {
                efficiency += mod.negInfluence * mod.count / 100;
            }
            else
            {
                efficiency += mod.posInfluence * mod.count / 100;
            }
        }
        if (efficiency < 0)
            efficiency = 0.1f;
    }

    public List<(ModType, int)> Save()
    {
        return modifiers.Where(q => q.count != 0).Select(q => (q.modType, q.count)).ToList();
    }

    public void Load(List<(ModType, int)> mods)
    {
        modifiers = new();
        if (mods == null)
            return;
        foreach (var item in mods)
        {
            EfficiencyMod mod = SceneRefs.Humans.modifiers.GetModifier(item.Item1);
            mod.count = item.Item2;
            modifiers.Add(mod);
        }
        CalculateEfficiecy();
    }
}
