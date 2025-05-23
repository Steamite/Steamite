﻿public class Water : ClickableObject
{
    public readonly int quality = 50;
    public int ammount = 50;

    #region Basic Operations
    public override GridPos GetPos()
    {
        return new(transform.position.x, transform.position.y / 2, transform.position.z);
    }
    #endregion

    #region Window
    /*public override InfoWindow OpenWindow()
    {
        InfoWindow info;
        if (info = base.OpenWindow())
        {
            info.SwitchMods(InfoMode.Water);
            return info;
        }
        throw new ArgumentException();
    }*/
    /*protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
        //info.clickObjectTransform.GetChild((int)InfoMode.Water).GetChild(0).GetComponent<TMP_Text>().text = $"Storing: {ammount}";
    }*/
    #endregion

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
        base.Load(save);
    }
    #endregion Saving
}
