using System.Linq;
using TMPro;
using UnityEngine;
public class WaterPump : ProductionBuilding
{
    [Header("Fluid")]
    [SerializeField] NetworkAccesBuilding networkAccess = new();
    Building lastAccesNetworkElem = null;
    Water water;

    public override void UniqueID()
    {
        base.UniqueID();
        networkAccess.ID(transform.GetChild(2));
    }
    public override void ProgressProduction(float speed)
    {
        if(pStates.supplied)
            base.ProgressProduction(speed);
    }
    protected override void Product()
    {
        while (currentTime >= prodTime)
        {
            currentTime -= prodTime;
            if (networkAccess.fluid.capacity[0] > networkAccess.fluid.ammount[0])
            {
                ExtractWater(this);
            }
            else
            {
                if (lastAccesNetworkElem)
                {
                    /*if(lastAccesNetworkElem.GetFluid().ammount[0] < lastAccesNetworkElem.GetFluid().capacity[0]) 
                    {
                        ExtractWater(lastAccesNetworkElem);
                        continue;
                    }
                    lastAccesNetworkElem = null;*/
                }
                if ((lastAccesNetworkElem = networkAccess.FindStore(FluidType.Water, transform.GetChild(3), true)) != null)
                {
                    ExtractWater(lastAccesNetworkElem);
                }
                else
                {
                    pStates.space = false;
                    RefreshStatus();
                }
            }
        }
    }
    protected virtual void ExtractWater(Building building)
    {
        if (water.ammount > 0)
        {
            /*int i = building.GetFluid().type.IndexOf(FluidType.water);
            if(i > -1)
            {
                building.GetFluid().ammount[i]++;
                water.ammount--;
                water.OpenWindow();
                building.OpenWindow();
                if (building != this)
                    OpenWindow();
            }*/
        }
        else
        {
            pStates.supplied = false;
            currentTime = 0;
            RefreshStatus();
            //OpenWindow();
        }
    }
    public override bool CanPlace()
    {
        bool res = true;
        if (!base.CanPlace())
            res = false;
        if (!networkAccess.ConnectPipes(transform.GetChild(2)))
            res = false;
       /* GridPos gridPos = new(transform.GetChild(2).transform.position);
        if (MyGrid.GetGridItem(gridPos).PrintText() != "w")
            res = false;*/
        return res;
    }
    public override void PlaceBuilding(GridTiles gT)
    {
        base.PlaceBuilding(gT);
        networkAccess.PlacePipes(transform.GetChild(3));
    }
    public override void FinishBuild()
    {
        base.FinishBuild();
        /*GridPos waterPos = new(transform.GetChild(2).gameObject);
        water = MyGrid.GetGridItem(waterPos) as Water;*/
        networkAccess.ConnectToNetwork(transform.GetChild(3));
    }
    protected override void AfterProduction()
    {
        return;
    }
    public override Chunk Deconstruct(GridPos instantPos)
    {
        return base.Deconstruct(instantPos);
    }
    public override void DestoyBuilding()
    {
        base.DestoyBuilding();
        networkAccess.DisconnectFromNetwork(transform.GetChild(3));
    }
    /*public override Fluid GetFluid()
    {
        return networkAccess.fluid;
    }*/
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
