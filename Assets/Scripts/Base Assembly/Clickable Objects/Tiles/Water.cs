using Unity.Properties;
using UnityEngine;

public class Water : TileSource
{
    public override GridPos GetPos()
    {
        return new(
            transform.position.x,
            (transform.position.y - ClickableObjectFactory.ROAD_OFFSET) / ClickableObjectFactory.LEVEL_HEIGHT,
            transform.position.z);
    }


    public override object RemoveFromSource(int ammount, bool remove)
    {
        Resource change = new();
        Storing.GetResOfAmmount(change, ammount, remove);
        if(remove)
            UIUpdate(nameof(Storing));
        if (Storing.Sum() == 0)
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
        (clickable as WaterSave).fluid = new(Storing);
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        Storing = new((save as WaterSave).fluid);
        HasResources = Storing.Sum() > 0;
        base.Load(save);
    }
}