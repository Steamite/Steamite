using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public struct TradeRoute
{
    public int cost;
    public int reward;
    public Resource buying;
    public Resource selling;

    public TradeRoute(int _cost)
    {
        cost = 0;
        reward = 0;
        selling = new();
        buying = new();
    }
}

public class TradeInfo : MonoBehaviour
{
    [Header("final money")]
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text moneyChangeText;
    [SerializeField] TMP_Text finalMoneyText;
    [SerializeField] Button confirmButton;
    [SerializeField] TMP_Text expeditonText;

    [Header("Buy summary")]
    [SerializeField] TMP_Text buyCapText;
    [SerializeField] TMP_Text buyResText;
    [SerializeField] TMP_Text buyCostText;

    [Header("Sell summary")]
    [SerializeField] TMP_Text sellCapText;
    [SerializeField] TMP_Text sellResText;
    [SerializeField] TMP_Text sellCostText;

    [Header("")]
    [SerializeField] Trade trade;

    List<TradeDeal> buy;
    List<TradeDeal> sell;

    TradeRoute activeTrade;

    public void Hide(int lastIndex)
    {
        gameObject.SetActive(false);
        Transform t = trade.window.transform.GetChild(1).GetChild(lastIndex);
        trade.window.transform.GetChild(0).GetChild(0).GetChild(lastIndex).GetComponent<Image>().color = trade.unavailableColor;
        t.GetChild(0).GetComponent<Animator>().SetTrigger("unselected");
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public string ChangeTradeLocation(TradeLocation tradeLocation)
    {
        activeTrade = new();
        ChangeDeals(0, tradeLocation.wantToSell);
        buy = tradeLocation.wantToSell;
        ChangeDeals(1, tradeLocation.wantToBuy);
        sell = tradeLocation.wantToBuy;

        UpdateTradeText();
        return tradeLocation.name;
    }

    void ChangeDeals(int i, List<TradeDeal> deals)
    {
        for (int j = 0; j < 3; j++)
        {
            Transform dealTran = transform.GetChild(i).GetChild(j + 2);
            if (j < deals.Count)
            {
                dealTran.gameObject.SetActive(true);
                dealTran.GetChild(0).GetComponent<TMP_Text>().text = deals[j].type.ToString();
                dealTran.GetChild(1).GetComponent<TMP_Text>().text = deals[j].cost.ToString();
                dealTran.GetChild(2).GetComponent<TMP_InputField>().text = "";
                dealTran.GetChild(3).GetComponent<TMP_Text>().text = MyRes.resources.ammount[MyRes.resources.type.IndexOf(deals[j].type)].ToString();
            }
            else
                dealTran.gameObject.SetActive(false);
        }
    }

    public void UpdateTradeText()
    {
        if (gameObject.activeSelf)
        {
            activeTrade = CalculateTradeCost();
            int capacity = trade.colonyLocation.stats[0].currentProduction * 50;
            string buttonText = "confirm";
            if (trade.colonyLocation.stats[0].currentProduction <= trade.expeditions.Count)
                buttonText = "no available expeditions"; // error 0

            //-------------money summary------------\\
            int moneyC = activeTrade.reward - activeTrade.cost;
            MakeSummary(MyRes.money, moneyC, moneyC + MyRes.money,
                moneyChangeText, moneyText, finalMoneyText, ref buttonText, false);

            //-------------buy------------\\
            MakeSummary(capacity, activeTrade.buying.ammount.Sum(), activeTrade.cost,
                buyResText, buyCapText, buyCostText, ref buttonText, true);

            //-------------sell-----------\\
            MakeSummary(capacity, activeTrade.selling.ammount.Sum(), activeTrade.reward,
                sellResText, sellCapText, sellCostText, ref buttonText, true);
            List<Resource> availableResources = MyGrid.buildings.Where(q => q.GetComponent<Storage>() != null).Select(q => q.localRes.Future(true)).ToList();
            for (int i = 0; i < activeTrade.selling.ammount.Count; i++)
            {
                if (activeTrade.selling.ammount[i] > availableResources.Sum(q => q.ammount[q.type.IndexOf(activeTrade.selling.type[i])]))
                {
                    transform.GetChild(1).GetChild(i + 2).GetChild(2).GetComponent<TMP_InputField>().textComponent.color = Color.red;
                    if (buttonText == "confirm")
                        buttonText = "not enough in store"; // error 3
                }
                else
                {
                    transform.GetChild(1).GetChild(i + 2).GetChild(2).GetComponent<TMP_InputField>().textComponent.color = Color.white;
                }
            }

            confirmButton.interactable = buttonText == "confirm";
            confirmButton.transform.GetChild(0).GetComponent<TMP_Text>().text = buttonText;
            capacity /= 50;
            expeditonText.text = $"{capacity - trade.expeditions.Count} / {capacity}";
        }
    }

    void MakeSummary(int capacity, int ammount, int revenue, TMP_Text res, TMP_Text cap, TMP_Text mon, ref string message, bool isRes)
    {
        if (isRes)
        {
            if (ammount > capacity)
            {
                res.color = Color.red;
                if (message == "confirm")
                    message = "over capacity"; // error 4
            }
            else if (activeTrade.buying.ammount.Sum() + activeTrade.selling.ammount.Sum() == 0)
            {
                res.color = Color.white;
                if (message == "confirm")
                    message = "no resources selected"; // error 1
            }
            else
            {
                res.color = Color.white;
            }
        }
        else
        {
            if(revenue < 0)
            {
                res.color = Color.red; 
                if (message == "confirm")
                    message = "insufficient funds"; // error 2
            }
            else
                res.color = Color.white;
        }
        res.text = ammount.ToString();
        cap.text = capacity.ToString();
        mon.text = revenue.ToString();
    }

    TradeRoute CalculateTradeCost()
    {
        activeTrade = new(0);
        Transform buyTran = transform.GetChild(0);
        Transform sellTran = transform.GetChild(1);
        for (int i = 0; i < 3; i++)
        {
            int midlleCount = 0;
            int.TryParse(buyTran.GetChild(i + 2).GetChild(2).GetComponent<TMP_InputField>().text, out midlleCount);
            if (buy.Count > i)
            {
                activeTrade.cost += midlleCount * buy[i].cost;
                activeTrade.buying.type.Add(buy[i].type);
                activeTrade.buying.ammount.Add(midlleCount);
            }
            midlleCount = 0;
            int.TryParse(sellTran.GetChild(i + 2).GetChild(2).GetComponent<TMP_InputField>().text, out midlleCount);
            if (sell.Count > i)
            {
                activeTrade.reward += midlleCount * sell[i].cost;
                activeTrade.selling.type.Add(sell[i].type);
                activeTrade.selling.ammount.Add(midlleCount);
            }
        }
        return activeTrade;
    }

    public void CommitTrade()
    {
        activeTrade = CalculateTradeCost();
        if (MyRes.money + activeTrade.reward - activeTrade.cost >= 0)
        {
            Resource diff = MyRes.DiffRes(activeTrade.selling, MyRes.resources);
            if (diff.ammount.Count > 0 && diff.ammount.Min() < 0)
                return;
            // REMOVES resources
            MyRes.TakeFromGlobalStorage(activeTrade.selling);
            
            trade.StartExpediton(new(activeTrade.buying, activeTrade.reward));
            MyRes.ManageMoney(-activeTrade.cost);

            // UPDATE UI
            ChangeDeals(0, buy);
            ChangeDeals(1, sell);
            UpdateTradeText();
        }
        else
        {
            CanvasManager.ShowMessage("No money");
        }
    }
}
