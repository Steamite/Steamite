
using System;

[Serializable]
public class Production
{
    public StorageResource inputResource = new();
    public Resource productionCost = new();
    public Resource production = new();
    public Production()
    {

    }
}