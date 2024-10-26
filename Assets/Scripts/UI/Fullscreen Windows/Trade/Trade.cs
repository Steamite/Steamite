using System;
using System.Collections;
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

    [Header("Customizables")]
    [SerializeField] Color unavailableColor;
    [SerializeField] Color productionColor;
    [SerializeField] float expeditionSpeed;

    [Header("Data")]
    [SerializeField] public StartingLocation colonyLocation;
    [SerializeField] List<TradeLocation> tradeLocations;

    public List<TradeExpedition> expeditions;

    int lastIndex = -2;
    public override void OpenWindow()
    {
        base.OpenWindow();
        lastIndex = -2;
        SelectButton(-1);
        if(expeditions.Count > 0)
        {
            StartCoroutine(MoveTradeRoute());
        }
    }
    public override void CloseWindow()
    {
        base.CloseWindow();
        StopCoroutine(MoveTradeRoute());
    }

    public void Init()
    {
        MyGrid.sceneReferences.GetComponent<Tick>().timeController.weekEnd += DoPassiveProduction;
        MyGrid.sceneReferences.GetComponent<Tick>().tickAction += MoveTradeRouteProgress;

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
        expeditions = new();
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
        if (index == lastIndex)
            return;
        if(lastIndex != -2)
            window.transform.GetChild(1).GetChild(lastIndex + 1).GetChild(0).GetComponent<Animator>().SetTrigger("unselected");
        lastIndex = index;
        window.transform.GetChild(1).GetChild(index + 1).GetChild(0).GetComponent<Animator>().SetTrigger("selected");
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

    public void StartExpediton(TradeExpedition expedition)
    {
        Slider slider = window.transform.GetChild(0).GetChild(0).GetChild(lastIndex).GetComponent<Slider>();
        slider.value = 0;
        slider.maxValue = Vector3.Distance(slider.transform.position, window.transform.GetChild(1).GetChild(lastIndex + 1).transform.position);
        slider.handleRect.GetComponent<ExpeditionInfo>().SetExpedition(expedition);
        expedition.tradeLocation = lastIndex;
        expedition.maxProgress = slider.maxValue;
        expeditions.Add(expedition);
        if (expeditions.Count == 1)
        {
            StartCoroutine(MoveTradeRoute());
        }
    }

    void MoveTradeRouteProgress()
    {
        foreach(TradeExpedition exp in expeditions)
        {
            exp.currentProgress += exp.goingToTrade ? 4 : -4;
        }
    }

    IEnumerator MoveTradeRoute()
    {
        while (true)
        {
            for(int i = expeditions.Count -1; i >= 0; i--)
            {
                TradeExpedition exp = expeditions[i];
                Slider slider = window.transform.GetChild(0).GetChild(0).GetChild(exp.tradeLocation).GetComponent<Slider>();
                slider.value = Mathf.Lerp(slider.value, exp.currentProgress, expeditionSpeed * Time.deltaTime * Time.timeScale);
                slider.handleRect.GetComponent<ExpeditionInfo>().MoveInfo();
                if(exp.goingToTrade ? slider.value >= slider.maxValue : slider.value <= 0)
                {
                    if (exp.FinishExpedition())
                    {
                        expeditions.RemoveAt(i);
                        if (expeditions.Count == 0)
                            yield break;
                    }
                }
            }
            yield return null;
        }
    }
}