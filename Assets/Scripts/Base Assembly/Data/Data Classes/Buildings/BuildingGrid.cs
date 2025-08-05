using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>A "Blueprint" containing the grid data of a building.</summary>
[Serializable]
public class BuildingGrid
{
    /// <summary>Dimensions of the building.</summary>
    [SerializeField] public GridPos size;
    /// <summary>Anchor offset from the center(position).</summary>
    [SerializeField] public GridPos moveBy;
    /// <summary>Relative anchor position.</summary>
    [SerializeField] public GridPos anchor;
    /// <summary>List of tiles.</summary>
    [SerializeField] public List<NeededGridItem> itemList;
}