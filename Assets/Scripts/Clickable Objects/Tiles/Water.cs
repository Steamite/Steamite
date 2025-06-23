using Unity.Properties;
using UnityEngine;

public class Water : ClickableObject
{
    public readonly int quality = 50;
    [SerializeField] int ammount = 50;
    [CreateProperty]
    public int Ammount
    {
        get => ammount;
        set
        {
            ammount = value;
            if (ammount == 0)
                hasResources = false;
            UIUpdate(nameof(Ammount));
        }
    }
    public bool hasResources;

    #region Basic Operations
    public override GridPos GetPos()
    {
        return new(transform.position.x, transform.position.y / 2, transform.position.z);
    }
    #endregion

    public override InfoWindow OpenWindow()
    {
        InfoWindow info = base.OpenWindow();
        info.Open(this, InfoMode.Water);
        return info;
    }


    #region Saving
    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new WaterSave();
        (clickable as WaterSave).ammount = ammount;
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        ammount = (save as WaterSave).ammount;
        hasResources = ammount != 0;
        base.Load(save);
    }
    #endregion Saving
}
