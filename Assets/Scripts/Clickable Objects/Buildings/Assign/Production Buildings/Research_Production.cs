using TMPro;

public class Research_Production : ProductionBuilding
{
    //private int temp_workers_count = 0; Already exists: ProductionBuilding.working
    ResearchBackend researchBackend;

    public override void UniqueID()
    {
        base.UniqueID();
        researchBackend = MyGrid.canvasManager.research.GetComponent<ResearchBackend>();
    }

    public override InfoWindow OpenWindow(bool setUp = false)
    {
        InfoWindow info;
        // if selected
        if ((info = base.OpenWindow(setUp)) != null)
        {
            // if to be setup
            if (setUp)
            {
                // disable production button
                info.cTransform.GetChild(2).gameObject.SetActive(false);
                info.cTransform.GetChild(3).gameObject.SetActive(true);
                if (researchBackend.currentResearch)
                    info.cTransform.GetChild(3).GetComponent<TMP_Text>().text = researchBackend.currentResearch.name;
                else
                    info.cTransform.GetChild(3).GetComponent<TMP_Text>().text = "None";
            }
            return info;
        }
        return null;
    }

    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
        researchBackend = MyGrid.canvasManager.research.GetComponent<ResearchBackend>();
    }

    public override void Produce(float speed)
    {
        //base.Produce();
        researchBackend.DoResearch(speed);
    }


    /*public override void Take(Human h, int transferPerTick)
    {
        base.Take(h, transferPerTick);
    }
    public override void Store(Human h, int transferPerTick)
    {
        if (!build.constructed)
            base.Store(h, transferPerTick);
        else
        {
            //MyRes.MoveRes(researchBackend.researchResourceInput, h.inventory, , transferPerTick);
        }
    }
    public override Resource GetDiff(Resource r)
    {
        if(!build.constructed)
            return base.GetDiff(r);
        else
        {
            Resource x = new();
            MyRes.ManageRes(x, researchBackend.currentResearch.node.reseachCost, 2);
            return MyRes.DiffRes(x, researchBackend.researchResourceInput.Future(), r);
        }
    }*/

    protected override void AfterProduction()
    {
        //base.AfterProduction();
    }

    public override void RefreshStatus()
    {
        //base.RefreshStatus();
    }
    protected override void Product()
    {
        //
    }
}
