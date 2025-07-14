using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FluidType
{
    Water,
    Steam,
}

[Serializable]
public class Fluid : ResAmmount<FluidType>
{
    public List<int> capacities = new();

    #region Constructor
    public Fluid() { }
    public Fluid(Fluid fluid) : base(fluid) { }
    public Fluid(List<FluidType> _type, List<int> _ammount, List<int> _capacity) 
        : base(_type, _ammount)
    {
        capacities = _capacity;
    }

    public Fluid(IEnumerable<FluidType> _types, int currentValue)
    {
        foreach (var item in _types)
        {
            types.Add(item);
            ammounts.Add(0);
            capacities.Add(currentValue);
        }
    }
    #endregion

    #region Adding
    protected override void Add(FluidType type, int ammount)
    {
        int i = types.IndexOf(type);
        if (i == -1)
        {
            Debug.LogError($"{type} not present in this fluid: {this}, adding");
        }
        else
            ammounts[i] += ammount;
    }
    #endregion

    protected override int Remove(FluidType type, int change)
    {
        int i = types.IndexOf(type);
        if (i == -1)
        {
            Debug.LogError($"{type} not present in this fluid: {this}, decresing");
            return 0;
        }
        else
        {
            if (ammounts[i] < change)
                change = ammounts[i];
            ammounts[i] -= change;
        }
        return change;
    }




    public bool HasSpace(FluidType type, int ammount)
    {
        return capacities[types.IndexOf(type)] - this[type] >= ammount;
    }

    public bool HasSpace(Fluid fluid)
    {
        for (int i = 0; i < fluid.types.Count; i++)
        {
            if (HasSpace(fluid.types[i], fluid.ammounts[i]) == false)
                return false;
        }
        return true;
    }

    public bool Contains(Fluid fluidCost)
    {
        for (int i = 0; i < fluidCost.types.Count; i++)
        {
            if (types.Contains(fluidCost.types[i]))
            {
                if (this[fluidCost.types[i]] - fluidCost.ammounts[i] < 0)
                    return false;
            }
            else
                return false;
        }
        return true;
    }


    public Fluid Diff(Fluid FluidCost)
    {
        Fluid fluid = new Fluid();
        Diff(fluid, FluidCost);
        return fluid;
    }

}