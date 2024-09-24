using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
public class Research_Production : ProductionBuilding
{
    //private int temp_workers_count = 0; Already exists: ProductionBuilding.working
    private ResearchBackend Research_Script;
    protected override void Awake()
    {
        base.Awake();
        Research_Script = MyGrid.sceneReferences.canvasManager.research.GetComponent<ResearchBackend>();
    }

    public override void Produce()
    {
        base.Produce();
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
        Research_Script.AddResearchers(1);
    }
    public override void RefreshStatus()
    {
        //base.RefreshStatus();
    }
}
