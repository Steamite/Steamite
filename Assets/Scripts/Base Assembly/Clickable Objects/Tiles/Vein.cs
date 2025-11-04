using Unity.Properties;
using UnityEngine;

public class Vein : TileSource
{
    [SerializeField] Resource storing = new();
    [CreateProperty] public Resource Storing { get => storing; set => storing = value; }

    public int xSize;
    public int zSize;

    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Vein);
        return info;
    }
    public override object RemoveFromSource(int ammount, bool remove)
    {
        Resource change = new();
        Storing.GetResOfAmmount(change, ammount, remove);
        UIUpdate(nameof(Storing));
        if (storing.Sum() == 0)
            HasResources = false;
        return change;
    }
    public override GridPos GetPos()
    {
        return new(
            Mathf.FloorToInt(transform.position.x),
            (transform.position.y - ClickableObjectFactory.ROCK_OFFSET) / ClickableObjectFactory.LEVEL_HEIGHT,
            Mathf.FloorToInt(transform.position.z));
    }

    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new VeinSave();
        (clickable as VeinSave).gridPos = this.GetPos();
        (clickable as VeinSave).resource = new(Storing);
        (clickable as VeinSave).sizeX = xSize;
        (clickable as VeinSave).sizeZ = zSize;
        (clickable as VeinSave).veinColor = new(GetComponent<MeshRenderer>().sharedMaterial.color);

        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        Storing = new((save as VeinSave).resource);
        xSize = (save as VeinSave).sizeX;
        zSize = (save as VeinSave).sizeZ;
        HasResources = Storing.Sum() > 0;
        GetComponent<MeshRenderer>().material.color = (save as VeinSave).veinColor.ConvertColor();

        MyGrid.veins.Add(this);
        base.Load(save);
    }
    #endregion
}