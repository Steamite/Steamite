

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>A "Blueprint" containing the grid data of a building.</summary>
[Serializable]
public struct BuildingGrid
{
    /// <summary>Dimensions of the building.</summary>
    [SerializeField] public int2 size;
    /// <summary>Anchor offset from the center(position).</summary>
    [SerializeField] public float2 renderSize;
    /// <summary>Relative anchor position.</summary>
    [SerializeField] public int anchorIndex;

    [SerializeField] public Mesh mesh;
    /// <summary>List of tiles.</summary>
    [FormerlySerializedAs("itemList")]
    [SerializeField] public GridItemType[] itemArray;
}