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
        networkAccess.ID(transform.GetChild(2));
    }
    public override void PlaceBuilding(GridTiles gT)
    {
        base.PlaceBuilding(gT);
        networkAccess.PlacePipes(transform.GetChild(2));
    }
    public override bool CanPlace()
    {
        if (!networkAccess.ConnectPipes(transform.GetChild(2)) || !base.CanPlace())
            return false;
        return true;
    }
    public override void FinishBuild()
    {
        networkAccess.ConnectToNetwork(transform.GetChild(2));
        base.FinishBuild();
    }
    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info = base.OpenWindow(setUp);
        if (info)
        {
            Transform storageMenu = info.transform.GetChild(1).GetChild(0).GetChild(1).GetChild(1);
            if (setUp)
            {
                storageMenu.gameObject.SetActive(true);
                storageMenu.GetChild(0).gameObject.SetActive(false);
                storageMenu.GetChild(1).gameObject.SetActive(true);
                storageMenu.GetChild(1).GetChild(0).GetChild(3).GetComponent<Image>().color = fillColor;
            }
            storageMenu.GetChild(1).GetChild(0).GetChild(3).GetComponent<Image>().fillAmount = (float)networkAccess.fluid.ammount[0] / (float)networkAccess.fluid.capacity[0];
            storageMenu.GetChild(1).GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = $"{networkAccess.fluid.ammount[0]} / {networkAccess.fluid.capacity[0]}";
            storageMenu.GetChild(1).GetChild(0).GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = $"{networkAccess.fluid.ammount[0]} / {networkAccess.fluid.capacity[0]}";
        }
        return info;
    }
    public override void DestoyBuilding()
    {
        base.DestoyBuilding();
        networkAccess.DisconnectFromNetwork(transform.GetChild(2));
    }
    public override Fluid GetFluid()
    {
        return networkAccess.fluid;
    }

    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {
        if (clickable == null)
            clickable = new TankBSave();
        (clickable as TankBSave).fillColor = new MyColor(fillColor);
        (clickable as TankBSave).fluidSave = networkAccess.SaveFluidData(transform.GetChild(2));
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {
        networkAccess.fluid = (save as TankBSave).fluidSave.fluid;
        networkAccess.Load(transform.GetChild(2), (save as TankBSave).fluidSave.pipeSaves);
        base.Load(save);
    }
}
