using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class Water : TileSource
{
    [SerializeField] Fluid storing = new();
    [CreateProperty] public Fluid Storing { get => storing; set => storing = value; }
    public override GridPos GetPos()
    {
        return new(
            transform.position.x, 
            (transform.position.y - ClickableObjectFactory.ROAD_OFFSET) / ClickableObjectFactory.LEVEL_HEIGHT, 
            transform.position.z);
    }


    public override object RemoveFromSource(int ammount, bool remove)
    {
        Fluid change = new();
        Storing.GetResOfAmmount(change, ammount, remove);
        UIUpdate(nameof(Storing));
        if (storing.Sum() == 0)
            HasResources = false;
        return change;
    }


    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Water);
        return info;
    }

    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new WaterSave();
        (clickable as WaterSave).fluid = Storing as Fluid;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        Storing = (save as WaterSave).fluid;
        HasResources = Storing.Sum() > 0;
        base.Load(save);
    }
}