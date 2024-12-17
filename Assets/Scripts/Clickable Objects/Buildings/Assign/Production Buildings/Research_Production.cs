
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Research_Production : ProductionBuilding
{
    bool b = true;

    public override void UniqueID()
    {
        base.UniqueID();
    }

    #region Window

    protected override void OpenWindowWithToggle(InfoWindow info, List<string> toEnable)
    {
        toEnable.Add("Research");
        base.OpenWindowWithToggle(info, toEnable);
    }
/*
    protected override void UpdateWindow(InfoWindow info)
    {
        base.UpdateWindow(info);
        ResearchDispayData data = SceneRefs.researchAdapter.DisplayResearch();
        VisualElement research = info.constructed.Q<VisualElement>("Research");

        research.Q<Label>("Name").text = data.name;
        research.Q<VisualElement>("Image").style.unityBackgroundImageTintColor = data.color;
        research.Q<Label>("Progress").text = data.progress;
    }*/

    #endregion
    public override void Load(ClickableObjectSave save)
    {
        base.Load(save);
        //researchBackend = SceneRefs.research.GetComponent<ResearchBackend>();
    }

    public override void Produce(float speed)
    {
        if (b)
        {
            gameObject.GetComponent<Animator>().SetBool("Rotate", true);
            b = false;
        }
        SceneRefs.researchAdapter.DoProduction(speed);
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
