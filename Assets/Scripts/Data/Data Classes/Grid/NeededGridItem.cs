using System;
using UnityEngine;

public enum GridItemType
{
    None, // doesn't matter
    Road, // must be free
    Water, // must be on a water tile
    Entrance, //here's an entry point - instantiate the entry points there
    Anchor // where the cursor is when moving
}

[Serializable]
public class NeededGridItem
{
    [SerializeField]
    public GridPos pos;
    [SerializeField]
    public GridItemType itemType;
    public NeededGridItem(GridPos _pos, GridItemType _itemType)
    {
        pos = _pos;
        itemType = _itemType;
    }
}
