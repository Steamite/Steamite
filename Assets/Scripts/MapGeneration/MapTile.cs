using System;
using UnityEngine;

public class MapTile
{
    public Resource resource;
    public int hardness;
    public string name;

    public MapTile(MinableRes minable)
    {
        resource = new Resource(new() { minable.resource }, new() { UnityEngine.Random.Range(minable.minAmmount, minable.maxAmmount) });
        hardness = minable.hardness;
        name = Enum.GetName(typeof(ResourceType), minable.resource);//minable.resource;
    }

    public MapTile()
    {
        resource = new();
        name = "Dirt";
    }
}
