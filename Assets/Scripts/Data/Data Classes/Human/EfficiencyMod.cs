using System;
using UnityEngine;

public enum ModType
{
    Food,
    House,
    Lazy
}

[Serializable]
public class EfficiencyMod
{
    [SerializeField]
    string name;
    public ModType modType;
    public int count = 1;
    [Header("positive")]
    public int posCount;
    public float posInfluence;
    [Header("negative")]
    public int negCount;
    public float negInfluence;

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
