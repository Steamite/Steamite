using System;
using UnityEngine;

public class MapTile
{
    public Resource resource;
    public int hardness;
    public string name;

    public MapTile(MinableRes _minable, int _resAmmount)
    {
        resource = new Resource(new() { _minable.resource }, new() { _resAmmount });
        hardness = _minable.hardness;
        name = Enum.GetName(typeof(ResourceType), _minable.resource);//minable.resource;
    }

    public MapTile()
    {
        resource = new();
        name = "Dirt";
    }
}
