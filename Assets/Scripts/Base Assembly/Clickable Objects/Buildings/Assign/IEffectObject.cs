using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public interface IEffectObject
{
    public ModifiableInteger Range { get; set; }
    public List<Road> EffectRoads { get; set; }

    GridPos EffectPos { get; set; }

    void RepaintTiles()
    {
        if(this is ClickableObject clickable && clickable.selected)
            MyGrid.GetOverlay().CreateTileOverlay(EffectRoads.Select(q => q.GetPos()));
    }

    public void RecalculateRange()
    {
        UpdateRange(true);
        RepaintTiles();
    }

    void UpdateRange(bool enable)
    {
        // GetPos() + blueprint.moveBy.Rotate(transform.rotation.eulerAngles.y)
        IEnumerable<ClickableObject> obj = MyGrid.GetTilesInRange(
            Range.currentValue,
            EffectPos,
            typeof(Road));
        List<Road> temp = EffectRoads.ToList();
        EffectRoads.Clear();

        if (enable == true)
        {
            foreach (Road road in obj.Cast<Road>())
            {
                temp.Remove(road);
                EffectRoads.Add(road);

                if (road == null) continue;
                road.AddEffect(this);
            }

            foreach (Road road in temp)
            {
                road.RemoveEffect(this);
            }
        }
        else
        {
            foreach (Road item in obj)
            {
                item.RemoveEffect(this);
            }
        }

    }

    public void AddEffectMod(Building building)
    {
        switch (building)
        {
            case House house:
                house.HasPub = true;
                break;
        }
    }

    public void RemoveEffectMod(Building building, bool stillContains)
    {
        if (stillContains) return;
        switch (building)
        {
            case House house:
                house.HasPub = false;
                break;
        }
    }
}
