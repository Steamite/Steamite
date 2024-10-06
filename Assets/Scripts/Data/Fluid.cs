using System;
using System.Collections.Generic;

public enum FluidType
{
    Water,
    Steam
}

[Serializable]
public class Fluid
{
    public List<FluidType> type = new();
    public List<int> ammount = new();
    public List<int> capacity = new();
}