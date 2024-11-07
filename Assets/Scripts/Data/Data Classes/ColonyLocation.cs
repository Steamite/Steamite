using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct PassiveProduction
{
    public string productionName;
    [Range(1, 5)]
    public int currentProduction;
    [Range(1, 5)]
    public int maxProduction;

    public PassiveProduction(string _name, int _current, int _max)
    {
        productionName = _name;
        currentProduction = _current;
        maxProduction = _max;
    }
}

[Serializable]
public class ColonyLocation
{
    public string name;
    public GridPos pos;
    [SerializeField] public List<PassiveProduction> passiveProductions;
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