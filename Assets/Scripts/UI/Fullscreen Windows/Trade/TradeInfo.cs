using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

struct TradeRoute
{
    public int cost;
    public Resource selling;
    public Resource buying;

    public TradeRoute(int _cost)
    {
        cost = _cost;
        selling = new();
        buying = new();
    }
}

public class TradeInfo : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    List<TradeDeal> selling;
    List<TradeDeal> buying;

    public string ChangeTradeLocation(TradeLocation tradeLocation)
    {
        ChangeDeals(0, tradeLocation.selling);
        selling = tradeLocation.selling;
        ChangeDeals(1, tradeLocation.buying);
        buying = tradeLocation.buying;
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
            }
            else
                dealTran.gameObject.SetActive(false);
        }
    }

    public void UpdateTradeText()
    {
        int cost = CalculateTradeCost().cost;
        text.color = cost > MyRes.money ? Color.red : Color.white;
        transform.GetChild(2).GetChild(2).GetComponent<Button>().interactable = cost > MyRes.money;
        text.text = cost.ToString();
    }
    
    TradeRoute CalculateTradeCost()
    {
        TradeRoute trade = new(0);
        Transform buy = transform.GetChild(0);
        Transform sell = transform.GetChild(1);
        for(int i = 0; i < 4; i++)
        {
            int midlleCount = 0;
            int.TryParse(buy.GetChild(i + 2).GetChild(2).GetComponent<TMP_InputField>().text, out midlleCount);
            if(selling.Count > i)
            {
                trade.cost -= midlleCount * selling[i].cost;
                trade.selling.type.Add(selling[i].type);
                trade.selling.ammount.Add(midlleCount);
            }
            midlleCount = 0;
            int.TryParse(sell.GetChild(i + 2).GetChild(2).GetComponent<TMP_InputField>().text, out midlleCount);
            if (buying.Count > i)
            {
                trade.cost -= midlleCount * buying[i].cost;
                trade.buying.type.Add(buying[i].type);
                trade.buying.ammount.Add(midlleCount);
            }
        }
        return trade;
    }

    public void CommitTrade()
    {
        TradeRoute route = CalculateTradeCost();
        Elevator el = MyGrid.buildings.Select(q => (Elevator)q).Where(q => q != null).First(q => q.main);
        MyRes.ManageRes(el.localRes.stored, route.selling, 1);
        MyRes.UpdateResource(route.selling, 1);
        MyRes.ManageRes(el.localRes.stored, route.buying, -1);
        MyRes.UpdateResource(route.buying, -1);
        MyRes.ManageMoney(route.cost);
    }
}
