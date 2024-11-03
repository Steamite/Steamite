using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[Serializable]
public class TradeExpedition
{
    [Header("Trade")]
    public bool goingToTrade = true;
    public int tradeLocation = -1;
    public int sliderID = -1;

    public float currentProgress = 0;
    public float maxProgress = 0;

    private Resource buying;
    private int reward;
    public TradeExpedition(Resource _buying, int _reward)
    {
        buying = _buying;
        reward = _reward;
    }

    public bool FinishExpedition()
    {
        if (goingToTrade)
        {
            currentProgress = maxProgress;
            goingToTrade = false;
            return false;
            //MyGrid.canvasManager.tradeWindow.transform.GetChild(0).GetChild(0).GetChild(tradeLocation).GetComponent<Slider>().value =;
        }
        else
        {
            MyRes.DeliverToElevator(buying);
            MyRes.ManageMoney(reward);
            MyGrid.canvasManager.tradeWindow.window.transform.GetChild(0).GetChild(1).GetChild(sliderID).gameObject.SetActive(false);
            return true;
        }
    }

    public override string ToString()
    {
        string s = (goingToTrade ? "Going there" : "Returning") + "\n";

        string x = buying.ToString();
        if (x != "")
            s += $"resources:{x}";
        s += $"reward:{reward}";
        return s;
    }
}
