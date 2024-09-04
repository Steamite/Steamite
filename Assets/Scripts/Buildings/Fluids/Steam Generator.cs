using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class SteamGenerator : ProductionBuilding
{
    [Header("Fluid")]
    [SerializeReference] NetworkAccesBuilding networkAccess = new();
    [SerializeField] int waterCost = 2;
    [SerializeField] int steamProduction = 1;
    [SerializeField] bool fluidSupplied = false;
    Building lastWaterAccessNetworkElem = null;
    Building lastSteamAccessNetworkElem = null;
    public override void UniqueID()
    {
        base.UniqueID();
        networkAccess.ID(transform.GetChild(2));
    }
    public override bool CanPlace()
    {
        if (!networkAccess.ConnectPipes(transform.GetChild(2)) || !base.CanPlace())
            return false;
        return true;
    }
    public override void PlaceBuilding(GridTiles gT)
    {
        base.PlaceBuilding(gT);
        networkAccess.PlacePipes(transform.GetChild(2));
    }
    public override void FinishBuild()
    {
        base.FinishBuild();
        networkAccess.ConnectToNetwork(transform.GetChild(2));
    }
    public override void DestoyBuilding()
    {
        base.DestoyBuilding();
        networkAccess.DisconnectFromNetwork(transform.GetChild(2));
    }
    protected override bool ManageInputRes()
    {
        if (!base.ManageInputRes())
            return false;
        if (!fluidSupplied)
        {
            pStates.running = false;
            if (networkAccess.fluid.ammount[0] < waterCost)
            {
                if (!lastWaterAccessNetworkElem)
                    lastWaterAccessNetworkElem = networkAccess.FindStore(FluidType.water, transform.GetChild(2), false);
                if (lastWaterAccessNetworkElem)
                {
                    int index = lastWaterAccessNetworkElem.GetFluid().type.IndexOf(FluidType.water);
                    int ammountToTransfer = lastWaterAccessNetworkElem.GetFluid().ammount[index];
                    if(ammountToTransfer > 0)
                    {
                        TakeWater();
                        if (networkAccess.fluid.ammount[0] < waterCost)
                            return false;
                    }
                }
                else
                    return false;
            }
            networkAccess.fluid.ammount[0] -= waterCost;
            fluidSupplied = true;
            pStates.running = true;
        }
        return true;
    }
    protected override void Product()
    {
        while (pTime.currentTime >= pTime.prodTime)
        {
            pTime.currentTime -= pTime.prodTime;
            if (networkAccess.fluid.capacity[1] > networkAccess.fluid.ammount[1])
            {
                networkAccess.fluid.ammount[1] += steamProduction;
            }
            else
            {
                if (lastSteamAccessNetworkElem)
                {
                    if (lastSteamAccessNetworkElem.GetFluid().ammount[0] < lastSteamAccessNetworkElem.GetFluid().capacity[0])
                    {
                        lastSteamAccessNetworkElem.GetFluid().ammount[1] += steamProduction;
                        continue;
                    }
                    lastSteamAccessNetworkElem = null;
                }
                if ((lastSteamAccessNetworkElem = networkAccess.FindStore(FluidType.steam, transform.GetChild(2), true)) != null)
                {
                    lastSteamAccessNetworkElem.GetFluid().ammount[1] += steamProduction;
                }
                else
                {
                    pStates.space = false;
                    RefreshStatus();
                }
            }
            OpenWindow();
            fluidSupplied = false;
            pStates.supplied = false;
            if (!ManageInputRes())
            {
                return;
            }
        }
    }
    /// <summary>
    /// Call after assinging lastWaterAccessNetworkElem
    /// </summary>
    void TakeWater()
    {
        int index = lastWaterAccessNetworkElem.GetFluid().type.IndexOf(FluidType.water);
        int ammountToExtract = lastWaterAccessNetworkElem.GetFluid().ammount[index];
        if (ammountToExtract > networkAccess.fluid.capacity[0] - networkAccess.fluid.ammount[0])
            ammountToExtract = networkAccess.fluid.capacity[0] - networkAccess.fluid.ammount[0];
        networkAccess.fluid.ammount[0] += ammountToExtract;
        lastWaterAccessNetworkElem.GetFluid().ammount[index] -= ammountToExtract;
        lastWaterAccessNetworkElem.OpenWindow();
    }
    protected override void AfterProduction()
    {
        base.AfterProduction();
    }
    public override Fluid GetFluid()
    {
        return networkAccess.fluid;
    }
    protected override void UpdateText(Transform t)
    {
        t = t.GetChild(2);
        t.GetChild(0).GetComponent<ProductionButton>().UpdateButtonState(pTime.currentTime, pTime.prodTime);
        // production cost
        t.GetChild(1).GetComponent<TMP_Text>()
            .text = MyRes.GetDisplayText(pRes.inputResource.stored, pRes.productionCost) + $"{networkAccess.fluid.type[0]} {networkAccess.fluid.ammount[0]}/{waterCost}";

        // production
        t.GetChild(2).GetComponent<TMP_Text>()
            .text = $"Steam: {steamProduction}";

        // stored
        t.GetChild(3).GetComponent<TMP_Text>()
            .text = $"Steam: {networkAccess.fluid.ammount[1]}/{networkAccess.fluid.capacity[1]}";
    }

    public override ClickableObjectSave Save(ClickableObjectSave clickable = null)
    {

        if (clickable == null)
            clickable = new FluidProdBSave();
        (clickable as FluidProdBSave).fluidSave = networkAccess.SaveFluidData(transform.GetChild(2));
        return base.Save(clickable);
    }
    public override void Load(ClickableObjectSave save)
    {

        networkAccess.fluid = (save as FluidProdBSave).fluidSave.fluid;
        networkAccess.Load(transform.GetChild(2), (save as FluidProdBSave).fluidSave.pipeSaves);
        base.Load(save);
    }
}
