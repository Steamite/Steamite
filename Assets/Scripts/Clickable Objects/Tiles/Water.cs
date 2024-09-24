using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

public class Water : ClickableObject
{
    public readonly int quality = 50;
    public int ammount = 50;
    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info = base.OpenWindow(setUp);
        if (info)
        {
            if(setUp)
                info.SwitchMods(4, name);
            info.transform.GetChild(1).GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = $"Storing: {ammount}";
        }
        return info;
    }
    public override string PrintText()
    {
        return "w";
    }
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
}
