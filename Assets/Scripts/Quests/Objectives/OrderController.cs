using Objectives;
using Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
public class OrderController
{
    public IUIElement orderInterface;
    QuestHolder data;
    public List<Order> orderChoice;
    Order currentOrder;

    OrderGenConfig orderGenConfig;

    public Order CurrentOrder { get => currentOrder; set => currentOrder = value; }

    public int finishedOrdersCount;

    public OrderController(QuestController _questController, UIDocument _questCatalog, Order _order, QuestControllerSave saveData)
    {
        data = _questController.data;
        orderInterface = _questCatalog.rootVisualElement[0][0].Q("OrderInterface") as IUIElement;
        currentOrder = _order;
        finishedOrdersCount = saveData.finishedOrdersCount;
        
        orderChoice = new();
        foreach (var item in saveData.orderChoiceSaves)
        {
            orderChoice.Add(new(item));
        }
        _order.Load(this, saveData.order);

        LoadOrderConfig();
    }

    public async void LoadOrderConfig()
    {
        orderGenConfig = await Addressables.LoadAssetAsync<OrderGenConfig>("Assets/Game Data/UI/OrderGenConfig.asset").Task;
        Order order = GenerateOrder();
    }

    public void CreateOrderChoice(Order order)
    {
        if(order.nextQuests.Count > 0)
        {
            int i = 0;
            List<QuestLink> nextQuests = order.nextQuests;
            if (nextQuests.Count == 1)
            {
                order = new(
                    data.Categories[nextQuests[i].categIndex]
                    .Objects.FirstOrDefault(q => q.id == nextQuests[i].questId));
            }
            else
            {
                orderChoice = new();
                for (; i < nextQuests.Count; i++)
                {
                    Order _order = new(
                            data.Categories[nextQuests[i].categIndex]
                            .Objects.FirstOrDefault(q => q.id == nextQuests[i].questId));
                    _order.objectives.ForEach(q => q.Load());
                    orderChoice.Add(_order);
                }
            }
        }
        else
        {
            orderChoice = new();
            orderChoice.Add(GenerateOrder());
            orderChoice.Add(GenerateOrder());
        }
    }

    public void UpdateTimers()
    {
        currentOrder?.DecreaseTimeToFail(this);
    }

    public void OpenWindow()
    {
        orderInterface.Open(this);
    }

    public Order GenerateOrder()
    {
        float mod = 1 + (finishedOrdersCount * 0.1f * QuestController.difficulty);

        float orderDifficulty = 0;

        int minAmmount = Mathf.RoundToInt(1 * mod);
        int maxAmmount = Mathf.RoundToInt(2 * mod);

        int resTypeCount = Random.Range(minAmmount, maxAmmount+1);

        ResourceObjective objective = new();
        List<ResourceGen> gen = orderGenConfig.resourceGens.ToList();
        

        for (int i = 0; i < resTypeCount; i++)
        {
            int max = gen.Sum(q => q.typeChance)+1;
            int random = Random.Range(0, max);
            ResourceGen selType = null;
            foreach (var item in gen)
            {
                if (random > item.typeChance)
                    random -= item.typeChance;
                else
                {
                    selType = item;
                    break;
                }
            }
            gen.Remove(selType);
            int resAmmount = selType.ammountRange.Random();
            objective.resource
                .ManageSimple(
                    selType.type,
                    resAmmount,
                    true,
                    mod);
            orderDifficulty += (float)resAmmount / (float)selType.typeChance;
        }
        
        Order order = new();
        order.orderDifficulty = (OrderDifficulty)Mathf.FloorToInt(orderDifficulty);
        order.TimeToFail = Mathf.RoundToInt(orderGenConfig.timeToFail.Random() * mod * orderDifficulty);
        order.objectives.Add(objective);
        order.rewards.Add(new TrustReward(
            Mathf.RoundToInt(
                orderGenConfig.trustGain.Random() * mod * orderDifficulty / 2)));
        order.penalties.Add(new TrustPenalty(
            Mathf.RoundToInt(
                orderGenConfig.trustLoss.Random() * mod * orderDifficulty / 2)));
        return order;
    }
}
