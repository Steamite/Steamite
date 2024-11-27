using TMPro;
using UnityEngine;

public class Research_Production : ProductionBuilding
{
    bool b = true;

    public override void UniqueID()
    {
        base.UniqueID();
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
                CanvasManager.researchAdapter.DisplayResearch(info.cTransform.GetChild(3).GetComponent<TMP_Text>());
            }
            return info;
        }
        return null;
    }

    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
        //researchBackend = CanvasManager.research.GetComponent<ResearchBackend>();
    }

    public override void Produce(float speed)
    {
        if (b)
        {
            gameObject.GetComponent<Animator>().SetBool("Rotate", true);
            b = false;
        }
        CanvasManager.researchAdapter.DoProduction(speed);
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
