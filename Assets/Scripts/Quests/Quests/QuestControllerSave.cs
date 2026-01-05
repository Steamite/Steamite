using Objectives;
using System.Collections.Generic;
using System.Linq;

public class QuestControllerSave
{
    public List<QuestSave> finishedQuests;
    public List<QuestSave> activeQuests;
    public QuestSave order;
    public int trust;
    public int finishedOrdersCount;
    public List<OrderChoiceSave> orderChoiceSaves;

    public QuestControllerSave() { }

    public QuestControllerSave(QuestController controller)
    {
        finishedQuests = controller.finishedQuests.Select(q => new QuestSave(q)).ToList();
        activeQuests = controller.activeQuests.Select(q => new QuestSave(q)).ToList();
        order = controller.orderController.CurrentOrder == null ? null: new(controller.orderController.CurrentOrder);
        trust = controller.Trust;
        finishedOrdersCount = controller.orderController.finishedOrdersCount;
        orderChoiceSaves = controller.orderController.orderChoice.Select(q => new OrderChoiceSave(q)).ToList();
    }
}

public class OrderChoiceSave
{
    public int timeToFail;
    public ResourceSave resources;
    public int gain;
    public int penalty;
    public int money;

    public OrderChoiceSave() { }
    public OrderChoiceSave(Order order)
    {
        timeToFail = order.TimeToFail;
        resources = new((order.objectives[0] as ResourceObjective).resource);
        money = +(order.objectives[0] as ResourceObjective).resource.Money;
        gain = (order.rewards[0] as TrustReward).gainAmmount;
        penalty = (order.penalties[0] as TrustPenalty).penaltyAmmount;
    }
}

public class QuestSave
{
    public int objectId;
    public List<int> currentProgress;
    public QuestState state;
    public int timeToFail;

    public QuestSave() { }
    public QuestSave(Quest quest)
    {
        objectId = quest.id;
        currentProgress = quest.objectives.Select(q => q.CurrentProgress).ToList();
        state = quest.state;
        timeToFail = quest.TimeToFail;
    }
}