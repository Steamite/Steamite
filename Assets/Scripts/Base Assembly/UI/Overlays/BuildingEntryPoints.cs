using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct EntryTile 
{
    public GridPos pos;
    public bool isActive;

    public EntryTile(GridPos pos, bool isActive)
    {
        this.pos = pos;
        this.isActive = isActive;
    }
} 

[Serializable]
public class BuildingEntryPoints
{
    List<EntryTile> entryTiles;

    public RectTransform Group => group;
    readonly RectTransform group;


    public IEnumerable<GridPos> AllTiles 
        => entryTiles.Select(q => q.pos);
    public IEnumerable<GridPos> EnabledTiles
        => entryTiles.Where(q => q.isActive).Select(q => q.pos);
    
    public int Enabled => entryTiles.Count(q => q.isActive);
    public int Disabled => entryTiles.Count(q => !q.isActive);

    public int Count => entryTiles.Count;

    public BuildingEntryPoints(List<EntryTile> entryTiles, RectTransform group)
    {
        this.entryTiles = entryTiles;
        this.group = group;
    }

    public bool IsActiveAt(GridPos pos)
    {
        int i = entryTiles.FindIndex(q => q.pos.Equals(pos));
        if (i == -1)
            return false;

        return entryTiles[i].isActive;
    }

    public void TogglePoint(GridPos pos, bool newState)
    {
        int i = entryTiles.FindIndex(q => q.pos.Equals(pos));
        if (i == -1)
            return;
        entryTiles[i] = new(pos, newState);
        group.GetChild(i).GetComponent<Image>().enabled = newState;
    }
}