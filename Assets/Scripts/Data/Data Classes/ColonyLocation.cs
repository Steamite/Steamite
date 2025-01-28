using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Data about passive production upgrade state.</summary>
[Serializable]
public struct PassiveProduction
{
    /// <summary>Name of this production.</summary>
    public string productionName;
    /// <summary>Upgrade level.</summary>
    [Range(1, 5)]public int currentProduction;
    /// <summary>Max Upgrade level.</summary>
    [Range(1, 5)]public int maxProduction;

    public PassiveProduction(string _name, int _current, int _max)
    {
        productionName = _name;
        currentProduction = _current;
        maxProduction = _max;
    }
}

/// <summary>Colony location show where the colony is, provides some passive production.</summary>
[Serializable]
public class ColonyLocation
{
    /// <summary>Colony name.</summary>
    public string name;
    /// <summary>Colony position on the screen.</summary>
    public GridPos pos;
    /// <summary>Upgradable passive production data.</summary>
    [SerializeField] public List<PassiveProduction> passiveProductions;
    /// <summary>Colony stats data.</summary>
    [SerializeField] public List<PassiveProduction> stats;

    public int GetCurrentProduction(string prodName)
    {
        int i = passiveProductions.FindIndex(q => q.productionName == prodName);
        if (i > -1)
            return passiveProductions[i].currentProduction;
        return -1;
    }

    public int GetCurrentStats(string prodName)
    {
        int i = stats.FindIndex(q => q.productionName == prodName);
        if (i > -1)
            return stats[i].currentProduction;
        return -1;
    }

    public ColonyLocation(string _name)
    {
        name = _name;
    }
}