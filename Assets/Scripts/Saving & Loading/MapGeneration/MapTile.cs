using System;
using UnityEngine;

/// <summary>Generated data for each map tile.</summary>
public class MapTile
{
    #region Variables
    /// <summary>Tile color.</summary>
    public Color color;
    /// <summary>Resources that the tile is supposed to contain.</summary>
    public Resource resource;
    /// <summary>Integrity of the tile.</summary>
    public int hardness;
    /// <summary>Name of Rock.</summary>
    public string name;
    #endregion

    #region Constructors
    public MapTile(MinableRes _minable, int _resAmmount)
    {
        resource = new Resource(new() { _minable.resource }, new() { _resAmmount });
        hardness = _minable.hardness;
        color = _minable.color;
        name = Enum.GetName(typeof(ResourceType), _minable.resource);//minable.resource;
    }

    public MapTile()
    {
        resource = new();
        name = "Dirt";
    }
    #endregion
}
