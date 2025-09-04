using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
public class OrderController
{
    QuestController questController;
    public IUIElement orderInterface;
    QuestHolder data;
    public List<Order> orderChoice;
    Order order;
    public Order Order { get => order; set => order = value; }

    public OrderController(QuestController _questController, UIDocument _questCatalog, Order _order, QuestControllerSave saveData)
    {
        questController = _questController;
        data = _questController.data;
        orderInterface = _questCatalog.rootVisualElement[0][0].Q("OrderInterface") as IUIElement;
        order = _order;
        _order.Load(this, saveData.order);
        orderChoice = new();
    }

    public void CreateOrderChoice(Order order)
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
            orderChoice.Clear();
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

    public void UpdateTimers()
    {
        order?.DecreaseTimeToFail(this);
    }

    public void OpenWindow()
    {
        orderInterface.Open(this);
    }
}
