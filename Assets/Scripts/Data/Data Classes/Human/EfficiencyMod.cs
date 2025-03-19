using System;
using UnityEngine;

/// <summary>Efficiency modifier, works in stacks(the longer it is the stronger it grows).</summary>
[Serializable]
public class EfficiencyMod
{
    #region Variables
    /// <summary>Modifier name for.</summary>
    [SerializeField] public string name;
    /// <summary>Modifier name for.</summary>
    [HideInInspector] public ModType modType;
    /// <summary>Current modifier state(can range from <see cref="negCount"/> to <see cref="posCount"/>).</summary>
    public int count = 1;

    /// <summary>Max count</summary>
    [Header("positive")] public int posCount;
    /// <summary>Efficiency influence for each stack</summary>
    public float posInfluence;

    /// <summary>Min count</summary>
    [Header("negative")] public int negCount;
    /// <summary>Efficiency influence for each negative stack</summary>
    public float negInfluence;
    #endregion

    public EfficiencyMod()
    {
        count = 1;
        posCount = -1;
        negCount = -1;
    }

    public EfficiencyMod(EfficiencyMod mod)
    {
        name = mod.name;
        modType = mod.modType;
        count = 0;
        posCount = mod.posCount;
        posInfluence = mod.posInfluence;
        negCount = mod.negCount;
        negInfluence = mod.negInfluence;
    }
}
