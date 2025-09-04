using InfoWindowElements;
using Objectives;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class OrderSelectionList : VisualElement, IUIElement
{
    OrderController controller;
    IUIElement orderInterface;
    public OrderSelectionList()
    {
        Add(new Label("Order selection"));
        hierarchy[0].AddToClassList("header");
        Add(new VisualElement());
        hierarchy[1].AddToClassList("selections-container");
    }

    public void Open(object data)
    {
        controller = data as OrderController;
        orderInterface = controller.orderInterface;
        hierarchy[1].Clear();
        int i = 0;
        foreach (Order item in controller.orderChoice)
        {
            hierarchy[1].Add(CreateCard(item, i));
            i++;
        }
    }

    VisualElement CreateCard(Order order, int i)
    {
        VisualElement card = new();
        card.AddToClassList("order-card");
        Label label;
        card.Add(label = new Label(order.Name));
        label.AddToClassList("order-card-header");

        card.Add(label = new Label($"Time: {Tick.RemainingTimeWithoutBrackets(order.TimeToFail)}"));
        label.AddToClassList("order-card-time");
        DoubleResList list;
        card.Add(list = new DoubleResList(true, ""));
        list.Open(order);
        Button button;
        card.Add(button = new Button(() => SelectOrder(i)) { text = "Select" });
        button.AddToClassList("main-button");
        return card;
    }


    void SelectOrder(int i)
    {
        Order order = controller.orderChoice[i];
        order.state = QuestState.Active;
        order.orderObjective = order.objectives[0] as ResourceObjective;

        controller.Order = order;
        
        controller.orderChoice.Clear();
        orderInterface.Open(controller);
    }
}
