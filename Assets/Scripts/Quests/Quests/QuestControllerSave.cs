using JetBrains.Annotations;
using Objectives;
using System;
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
        order = new (controller.orderController.CurrentOrder);
        trust = controller.Trust;
        finishedOrdersCount = controller.orderController.finishedOrdersCount;
        orderChoiceSaves = controller.orderController.orderChoice.Select(q => new OrderChoiceSave(q)).ToList();
    }
}

public class OrderChoiceSave
{
    public int timeToFail;
    public MoneyResource resources;
    public int gain;
    public int penalty;

    public OrderChoiceSave() { }
    public OrderChoiceSave(Order order)
    {
        timeToFail = order.TimeToFail;
        resources = (order.objectives[0] as ResourceObjective).resource;
        gain = (order.rewards[0] as TrustReward).gainAmmount;
        penalty = (order.penalties[0] as TrustPenalty).penaltyAmmount;
    }
}

public class QuestSave
{
    public int questId;
    public List<int> currentProgress;
    public QuestState state;
    public int timeToFail;

    public QuestSave() { }
    public QuestSave(Quest quest)
    {
        questId = quest.id;
        currentProgress = quest.objectives.Select(q => q.CurrentProgress).ToList();
        state = quest.state;
        timeToFail = quest.TimeToFail;
    }
}