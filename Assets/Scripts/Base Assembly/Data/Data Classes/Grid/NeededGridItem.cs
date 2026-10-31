

using System;
using Unity.Mathematics;
using UnityEngine;

/// <summary>Types of building tiles.</summary>
public enum GridItemType
{
    /// <summary>doesn't matter</summary>
    None,
    /// <summary>must be on a road tile</summary>
    Road,
    /// <summary>atleast one of them must be on a road tile</summary>
    Entrance,
    /// <summary>Pipe itself</summary>
    PassableRoad,
    Elevator,
    Pipe,
    /// <summary>must be on a water tile</summary>
    WaterSource,
    /// <summary>must be on a tile</summary>
    ResourceSource,
}