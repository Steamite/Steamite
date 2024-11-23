
using UnityEngine;

public class MapTile
{
    public Resource resource;
    public int hardness;

    public MapTile(MinableRes minable)
    {
        resource = new Resource(new() { minable.resource }, new() { Random.Range(minable.minAmmount, minable.maxAmmount) });
        hardness = minable.hardness;
    }

    public MapTile()
    {

    }
}
