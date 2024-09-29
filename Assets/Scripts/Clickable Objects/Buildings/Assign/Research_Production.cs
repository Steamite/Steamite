using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System;
public class Research_Production : ProductionBuilding
{
    //private int temp_workers_count = 0; Already exists: ProductionBuilding.working
    ResearchBackend Research_Script;

    public void Init()
    {
        Research_Script = MyGrid.canvasManager.research.GetComponent<ResearchBackend>();
    }

    public override void Load(ClickableObjectSave save)
    {
        Init();
        base.Load(save);
    }

    public override void Produce()
    {
        //base.Produce();
        Research_Script.DoResearch();
    }

    protected override void Product()
    {
        /*
        Transform research_transform  = GameObject.Find("Research Tree(Stays Active)").transform;
        research_transform.GetComponent<Research>().research_progress += 10; //add 10 research points
        */
        pTime.currentTime = 0;
    }
    protected override void AfterProduction()
    {
       // base.AfterProduction();
    }
    public override void Work(Human h)
    {
        base.Work(h);
    }
    public override void RefreshStatus()
    {
        //base.RefreshStatus();
    }

    
}
