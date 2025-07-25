using System;
using UnityEngine;
using UnityEngine.UIElements;
[Serializable]
public class TradeConvoy
{
    [Header("Trade")]
    public bool firstPhase = true;
    public int tradeLocation = -1;

    public float currentprogress = 0;
    public float maxprogress = -1;

    public Resource buying;
    public int reward;

    public TradeConvoy(Resource _buying, int _reward, int _tradeLocation, float _distance)
    {
        buying = _buying;
        reward = _reward;
        tradeLocation = _tradeLocation;
        maxprogress = _distance;
    }

    void FinishFirstPart()
    {
        currentprogress = 2 * maxprogress - currentprogress;
        firstPhase = false;
        VisualElement slider = ((VisualElement)UIRefs.TradingWindow.map).ElementAt(0).ElementAt(0).ElementAt(tradeLocation).ElementAt(0);
        slider.RemoveFromClassList("trading");
        slider.AddToClassList("retreat");
    }

    void FinishConvoy()
    {
        MyRes.DeliverToElevator(buying);
        MyRes.ManageMoneyGlobal(reward);
        UIRefs.TradingWindow.RemoveConvoy(this);
        Slider slider = (Slider)((VisualElement)UIRefs.TradingWindow.map).ElementAt(0).ElementAt(0).ElementAt(tradeLocation).ElementAt(0);
        slider.RemoveFromClassList("retreat");
        slider.AddToClassList("free");
        slider.value = 0;
    }

    public void Move(float speed)
    {
        if (firstPhase)
        {
            currentprogress += speed;
            if (currentprogress >= maxprogress)
                FinishFirstPart();
        }
        else
        {
            currentprogress -= speed;
            if (currentprogress <= 0)
                FinishConvoy();
        }
    }

    public override string ToString()
    {
        string s = (firstPhase ? "Going there" : "Returning") + "\n";

        string x = buying.ToString();
        if (x != "")
            s += $"resources:{x}";
        //s += $"reward:{reward}";
        return s;
    }
}
