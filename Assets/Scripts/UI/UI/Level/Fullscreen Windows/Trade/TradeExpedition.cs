using System;
using UnityEngine;

[Serializable]
public class TradeExpedition
{
    [Header("Trade")]
    public bool goingToTrade = true;
    public int tradeLocation = -1;
    public int sliderID = -1;

    public float currentprogress = 0;
    public float maxprogress = 0;

    public Resource buying;
    public int reward;
    public TradeExpedition(Resource _buying, int _reward)
    {
        buying = _buying;
        reward = _reward;
    }

    public bool FinishExpedition()
    {
        if (goingToTrade)
        {
            currentprogress = maxprogress;
            goingToTrade = false;
            return false;
            //SceneRefs.tradeWindow.transform.GetChild(0).GetChild(0).GetChild(tradeLocation).GetComponent<Slider>().value =;
        }
        else
        {
            MyRes.DeliverToElevator(buying);
            MyRes.ManageMoney(reward);
            UIRefs.trade.window.transform.GetChild(0).GetChild(1).GetChild(sliderID).gameObject.SetActive(false);
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
