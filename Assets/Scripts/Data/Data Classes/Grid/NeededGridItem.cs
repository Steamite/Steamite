using System;
using UnityEngine;

/// <summary>Types of building tiles.</summary>
public enum GridItemType
{
    /// <summary>doesn't matter</summary>
    None,
    /// <summary>only one for each blueprint, where the cursor is when moving</summary>
    Anchor,
    /// <summary>must be on a road tile</summary>
    Road,
    /// <summary>atleast one of them must be on a road tile</summary>
    Entrance,
    /// <summary>Pipe itself</summary>
    Pipe,
    /// <summary>must be on a water tile</summary>
    WaterSource,
    /// <summary>must be on a tile</summary>
    ResourceSource,
}

/// <summary>Represents one tile of a building.</summary>
[Serializable]
public class NeededGridItem
{
    /// <summary>Position of the tile.</summary>
    [SerializeField] public GridPos pos;
    /// <summary>Tile type.</summary>
    [SerializeField] public GridItemType itemType;

    public NeededGridItem(GridPos _pos, GridItemType _itemType)
    {
        pos = _pos;
        itemType = _itemType;
    }
}
