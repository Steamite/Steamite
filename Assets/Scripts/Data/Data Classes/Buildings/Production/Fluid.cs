using System;
using System.Collections.Generic;

public enum FluidType
{
    Water,
    Steam,
}

[Serializable]
public class Fluid
{
    public List<FluidType> types = new();
    public List<int> ammounts = new();
    public List<int> capacities = new();

    public int this[FluidType type] => ammounts[types.IndexOf(type)];
    public void AddFluid(FluidType type, int ammount, int capacity = -1)
    {
        int i = types.IndexOf(type);
        if(i == -1)
        {
            types.Add(type);
            ammounts.Add(ammount);
            capacities.Add(capacity);
        }
        else
            ammounts[i] += ammount;
    }

    public bool HasSpace(FluidType type, int ammount)
    {
        return capacities[types.IndexOf(type)] - this[type] >= ammount;
    }

    public Fluid() { }
    public Fluid(List<FluidType> types, List<int> ammounts, List<int> capacities)
    {
        this.types = types;
        this.ammounts = ammounts;
        this.capacities = capacities;
    }
}