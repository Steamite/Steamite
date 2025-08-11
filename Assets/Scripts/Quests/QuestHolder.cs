using Objectives;
using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public enum QuestState
{
    Hidden,
    Active,
    Failed,
    Completed
}


[Serializable]
public class Quest : DataObject, IUpdatable
{
    [SerializeField] public string description;
    [SerializeField] public QuestState state = QuestState.Hidden;
    [SerializeField] int timeToFail;
    [CreateProperty] public int TimeToFail { get => timeToFail; set => timeToFail = value; }
    [SerializeReference] public List<QuestReward> rewards = new();
    [SerializeReference] public List<QuestPenalty> penalties = new();
    [SerializeReference] public List<Objective> objectives = new();

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    public virtual void Complete(bool success, QuestController controller)
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
            foreach (Objective objective in objectives)
                objective.Cancel(controller);
        }
        controller.activeQuests.Remove(this);
        controller.finishedQuests.Add(this);
        controller.AddDummy();
    }

    public void Load(QuestSave save, QuestController controller)
    {
        timeToFail = save.timeToFail;
        for (int i = 0; i < objectives.Count; i++)
        {
            objectives[i].Load(save.currentProgress[i], this, controller);
        }
    }

    public void DecreaseTimeToFail(QuestController controller)
    {
        if (timeToFail > -1)
        {
            timeToFail--;
            UIUpdate(nameof(TimeToFail));
            if (timeToFail == 0)
            {
                Complete(false, controller);
            }
        }
    }

    public void UIUpdate(string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    public string GetRewPenText()
    {
        string s = "";
        foreach (QuestReward item in rewards)
            s += item.ToString();
        foreach (QuestPenalty item in penalties)
            s += item.ToString();
        return s;
    }

    public Quest() { }
    public Quest(int id) : base(id)
    {

    }
    public Quest(Quest quest)
    {
        id = quest.id;
        Name = quest.Name;
        timeToFail = quest.timeToFail;
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

    public override void Complete(bool success, QuestController controller)
    {
        base.Complete(success, controller);
        foreach (var quest in nextQuests)
        {
            quest.state = QuestState.Active;
        }
    }
    public StoryQuest() { }
    public StoryQuest(Quest quest) : base(quest) { }
}

[Serializable]
public class QuestCategory : DataCategory<Quest>
{

}

[CreateAssetMenu(fileName = "QuestData", menuName = "UI Data/Quests", order = 2)]
public class QuestHolder : DataHolder<QuestCategory, Quest>
{

}
