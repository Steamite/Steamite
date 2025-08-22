using InfoWindowElements;
using Objectives;
using UnityEngine.UIElements;

[UxmlElement]
public partial class OrderInterface : Tab, IUIElement
{
    VisualElement container;
    Label header;
    ProgressBar bar;
    DoubleResList cost;
    Button button;

    Label noneLabel;

    Order order;
    public OrderInterface() : base("Order")
    { 
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

        container.Add(cost = new());
        cost.AddToClassList("order-cost");

        container.Add(button = new() { name = "Button"});
        button.AddToClassList("order-button");
        button.text = "Button";

        Add(container);

        Add(noneLabel = new("No order selected"));
        noneLabel.style.display = DisplayStyle.None;
        noneLabel.AddToClassList("none-label");
    }

    public void Open(object data)
    {
        order = (data as QuestController).order;
        if(order != null)
        {
            container.style.display = DisplayStyle.Flex;
            noneLabel.style.display = DisplayStyle.None;

            header.text = order.Name;

            bar.highValue = order.originalTimeToFail;
            bar.value = order.originalTimeToFail - order.TimeToFail;
            bar.title = Tick.RemainingTime(order.TimeToFail);

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
                button.clicked -= OrderFinish;
            }
        }
        else
        {
            container.style.display = DisplayStyle.None;
            noneLabel.style.display = DisplayStyle.Flex;
        }
    }

    void OrderFinish()
    {
        MyRes.PayCostGlobal(order.orderObjective.resource);
        QuestController controller = SceneRefs.QuestController as QuestController;
        order.Complete(true, controller);
        Open(controller);
    }
}
