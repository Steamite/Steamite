using UnityEngine;

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
    public override void PlaceBuilding(bool loading = false)
    {
        base.PlaceBuilding(loading);
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
