using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class BuildingGrid
{
    [SerializeField]
    public GridPos size;
    [SerializeField]
    public GridPos moveBy;
    [SerializeField]
    public GridPos anchor;
    [SerializeField]
    public List<NeededGridItem> itemList;
}