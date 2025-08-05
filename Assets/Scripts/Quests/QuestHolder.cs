using Objectives;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    Hidden,
    Active,
    Failed,
    Completed
}


[Serializable]
public class Quest : DataObject
{
    [SerializeField] public string description;
    [SerializeField] public QuestState state = QuestState.Hidden;
    [SerializeReference] public List<QuestReward> rewards = new();
    [SerializeReference] public List<QuestPenalty> penalties = new();
    [SerializeReference] public List<Objective> objectives = new();

    public virtual void Complete(bool success)
    {
        state = success ? QuestState.Completed: QuestState.Failed;
        if (success)
        {
            foreach (var reward in rewards)
                reward.ObtainReward();
        }
        else
        {
            foreach (var penalty in penalties)
                penalty.GetPenalty();
        }
    }

    public Quest() { }
    public Quest(int id) : base(id)
    {

    }
    public Quest(Quest quest)
    {
        id = quest.id;
        Name = quest.Name;
        description = quest.description;
        state = quest.state;
        rewards = quest.rewards;
        penalties = quest.penalties;

        objectives = quest.objectives;
    }
}

[Serializable]
public class StoryQuest : Quest
{
    public List<Quest> nextQuests;

    public override void Complete(bool success)
    {
        base.Complete(success);
        foreach (var quest in nextQuests)
        {
            quest.state = QuestState.Active;
        }
    }
}

[Serializable]
public class QuestCategory : DataCategory<Quest>
{

}

[CreateAssetMenu(fileName = "QuestData", menuName = "UI Data/Quests", order = 2)]
public class QuestHolder : DataHolder<QuestCategory, Quest>
{

}
