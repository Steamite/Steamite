using InfoWindowElements;
using Objectives;
using UnityEngine.UIElements;

[UxmlElement]
public partial class OrderInterface : Tab, IUIElement
{
    readonly VisualElement container;
    readonly Label header;
    readonly ProgressBar bar;
    
    readonly Label rewardLabel;
    readonly Label penaltyLabel;
    
    readonly DoubleResList cost;
    readonly Button button;
    

    readonly OrderSelectionList orderSelection;


    readonly Label noneLabel;

    Order order;
    OrderController orderController;
    public OrderInterface() : base("Order")
    {
        #region Order Info
        style.flexGrow = 1;
        container = new();
        container.AddToClassList("order-container");

        container.Add(header = new("Header"));
        header.AddToClassList("order-title");

        container.Add(bar = new() { title = "xx D xx H"});
        bar.AddToClassList("order-bar");
        bar.value = 50;
        bar[0][0].AddToClassList("order-bar-background");
        bar[0][0][0].AddToClassList("order-bar-progress");

        VisualElement element = new();
        element.Add(rewardLabel = new("+ ### Trust"));
        element.Add(penaltyLabel = new("- ### Trust"));
        element.AddToClassList("order-row-between");
        container.Add(element);

        container.Add(cost = new());
        cost.AddToClassList("order-cost");

        container.Add(button = new() { name = "Button"});
        button.AddToClassList("order-button");
        button.text = "Button";

        Add(container);
        #endregion

        #region Order Selection
        Add(orderSelection = new());
        orderSelection.style.display = DisplayStyle.None;
        #endregion


        Add(noneLabel = new("No order selected"));
        noneLabel.style.display = DisplayStyle.None;
        noneLabel.AddToClassList("none-label");
    }

    public void Open(object data)
    {
        button.clicked -= OrderFinish;
        orderController = data as OrderController;
        order = orderController.CurrentOrder;
        if(order != null)
        {
            if(order.state == QuestState.Active)
            {
                container.style.display = DisplayStyle.Flex;
                orderSelection.style.display = DisplayStyle.None;
                noneLabel.style.display = DisplayStyle.None;

                header.text = order.Name;

                bar.highValue = order.originalTimeToFail;
                bar.value = order.originalTimeToFail - order.TimeToFail;
                bar.title = Tick.RemainingTime(order.TimeToFail);

                rewardLabel.text = $"{order.rewards[0]}";
                penaltyLabel.text = $"{order.penalties[0]}";

                cost.Open(order);
                if (MyRes.CanAfford(
                    (order.objectives[0] as ResourceObjective).resource))
                {
                    button.text = "Finish Order";
                    button.enabledSelf = true;
                    button.clicked += OrderFinish;
                }
                else
                {
                    button.text = "Not enough resources";
                    button.enabledSelf = false;
                }
            }
            else
            {
                container.style.display = DisplayStyle.None;
                orderSelection.style.display = DisplayStyle.Flex;
                noneLabel.style.display = DisplayStyle.None;
                orderSelection.Open(data);
            }
        }
        else
        {
            container.style.display = DisplayStyle.None;
            orderSelection.style.display = DisplayStyle.None;
            noneLabel.style.display = DisplayStyle.Flex;
        }
    }

    void OrderFinish()
    {
        MyRes.PayCostGlobal(order.orderObjective.resource);
        order.Complete(true, orderController);
        Open(orderController);
    }
}
