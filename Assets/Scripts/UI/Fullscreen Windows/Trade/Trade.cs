using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Trade : FullscreenWindow
{
    [Header("References")]
    [SerializeField] public TradeInfo tradeInfo;
    [SerializeField] Transform pasProdTran;
    [SerializeField] TMP_Text header;

    [Header("Colors")]
    [SerializeField] Color unavailableColor;
    [SerializeField] Color productionColor;

    [Header("Data")]
    [SerializeField] StartingLocation colonyLocation;
    [SerializeField] List<TradeLocation> tradeLocations;

    public override void OpenWindow()
    {
        base.OpenWindow();
        SelectButton(-1);
    }

    public void Init()
    {
        MyGrid.sceneReferences.GetComponent<Tick>().timeController.weekEnd += DoPassiveProduction;
        TradeHolder tradeHolder = (TradeHolder)Resources.Load("Holders/Data/Trade Data");
        tradeLocations = tradeHolder.tradeLocations;
        header.text = colonyLocation.name;
        for (int i = 0; i < colonyLocation.passiveProductions.Count; i++)
        {
            Transform cat = pasProdTran.GetChild(i+1).GetChild(1);
            for (int j = 4; j > -1; j--)
            {
                if (j >= colonyLocation.passiveProductions[i].maxProduction)
                    Destroy(cat.GetChild(j).gameObject);
                else if(j >= colonyLocation.passiveProductions[i].currentProduction)
                    cat.GetChild(j).GetComponent<Image>().color = unavailableColor;
                else
                    cat.GetChild(j).GetComponent<Image>().color = productionColor;
            }
        }
    }

    public void DoPassiveProduction()
    {
        Resource r = new(-1);
        r.type.Add(ResourceType.Food);
        r.ammount.Add(colonyLocation.GetCurrentProduction("Food") * 5);
        r.type.Add(ResourceType.Wood);
        r.ammount.Add(colonyLocation.GetCurrentProduction("Wood") * 3);
        for(int i = 0; i < colonyLocation.GetCurrentProduction("Workforce"); i++)
        {
            MyGrid.sceneReferences.humans.CreateHuman();
        }
        MyRes.DeliverToElevator(r);
    }

    public void SelectButton(int index)
    {
        if(index == -1)
        {
            tradeInfo.transform.parent.GetChild(0).gameObject.SetActive(true);
            tradeInfo.gameObject.SetActive(false);
            header.text = colonyLocation.name;
        }
        else
        {
            tradeInfo.transform.parent.GetChild(0).gameObject.SetActive(false);
            tradeInfo.gameObject.SetActive(true);
            header.text = tradeInfo.ChangeTradeLocation(tradeLocations[index]);
        }
    }
}
