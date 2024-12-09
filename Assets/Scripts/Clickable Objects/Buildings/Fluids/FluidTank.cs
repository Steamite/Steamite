using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FluidTank : Building
{
    [Header("Fluid")]
    [SerializeField] NetworkAccesBuilding networkAccess = new();
    [SerializeField] Color fillColor;

    public override void UniqueID()
    {
        base.UniqueID();
        networkAccess.ID(transform.GetChild(1));
    }
    public override void PlaceBuilding(GridTiles gT)
    {
        base.PlaceBuilding(gT);
        networkAccess.PlacePipes(transform.GetChild(2));
    }
    public override bool CanPlace()
    {
        bool res = true;
        if (!base.CanPlace())
            res = false;
        if (!networkAccess.ConnectPipes(transform.GetChild(2)))
            res = false;
        return res;
    }
    public override void FinishBuild()
    {
        networkAccess.ConnectToNetwork(transform.GetChild(2));
        base.FinishBuild();
    }
    #region Window
    protected override void SetupWindow(InfoWindow info, List<string> toEnable)
    {
        base.SetupWindow(info, toEnable);
        /*Transform storageMenu = info.transform.GetChild(1).GetChild(0).GetChild(1).GetChild(1);
        storageMenu.gameObject.SetActive(true);
        storageMenu.GetChild(0).gameObject.SetActive(false);
        storageMenu.GetChild(1).gameObject.SetActive(true);
        storageMenu.GetChild(1).GetChild(0).GetChild(3).GetComponent<Image>().color = fillColor;*/
    }

    protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
        /*storageMenu.GetChild(1).GetChild(0).GetChild(3).GetComponent<Image>().fillAmount = (float)networkAccess.fluid.ammount[0] / (float)networkAccess.fluid.capacity[0];
        storageMenu.GetChild(1).GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = $"{networkAccess.fluid.ammount[0]} / {networkAccess.fluid.capacity[0]}";
        storageMenu.GetChild(1).GetChild(0).GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = $"{networkAccess.fluid.ammount[0]}*/
    }
    #endregion

    public override void DestoyBuilding()
    {
        base.DestoyBuilding();
        networkAccess.DisconnectFromNetwork(transform.GetChild(1));
    }
    /*public override Fluid GetFluid()
    {
        return networkAccess.fluid;
    }*/

    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new TankBSave();
        (clickable as TankBSave).fillColor = new MyColor(fillColor);
        (clickable as TankBSave).fluidSave = networkAccess.SaveFluidData(transform.GetChild(1));
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        networkAccess.fluid = (save as TankBSave).fluidSave.fluid;
        networkAccess.Load(transform.GetChild(1), (save as TankBSave).fluidSave.pipeSaves);
        base.Load(save);
    }
}
