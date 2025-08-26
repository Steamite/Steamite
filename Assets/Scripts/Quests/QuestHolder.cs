using Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
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
public class QuestLink
{
    [SerializeField] public int categIndex;
    [SerializeField] public int questId;
    public QuestLink(int _categIndex, int _questId)
    {
        categIndex = _categIndex;
        questId = _questId;
    }

}

[Serializable]
public class Quest : DataObject, IUpdatable
{
    [SerializeField] public string description;
    [SerializeField] public QuestState state = QuestState.Hidden;
    [SerializeField] protected int timeToFail;
    [CreateProperty] public int TimeToFail { get => timeToFail; set => timeToFail = value; }
    [SerializeReference] public List<QuestReward> rewards = new();
    [SerializeReference] public List<QuestPenalty> penalties = new();
    [SerializeReference] public List<Objective> objectives = new();
    [SerializeField] public List<QuestLink> nextQuests = new();


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

        int i = 0;
        for (; i < nextQuests.Count; i++)
        {
            controller.data.Categories[nextQuests[i].categIndex]
                .Objects.FirstOrDefault(q => q.id == nextQuests[i].questId)?.Load(controller);
        }
        if(i == 0)
            controller.AddDummy();
    }


    public virtual void Load(QuestController controller, QuestSave save = null)
    {
        if(save != null)
            timeToFail = save.timeToFail;
        for (int i = 0; i < objectives.Count; i++)
        {
            objectives[i].Load(save != null ? save.currentProgress[i] : 0, this, controller);
        }
        for (int i = 0; i < rewards.Count; i++)
        {
            rewards[i].Init();
        }

        if (save == null || save.state == QuestState.Active)
        {
            controller.activeQuests.Add(this);
            state = QuestState.Active;
        }
        else
        {
            controller.finishedQuests.Add(this);
            state = save.state;
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
        s += "\n";
        foreach (QuestPenalty item in penalties)
            s += item.ToString();
        return s;
    }

    public override bool Equals(object obj)
    {
        if(obj is Quest quest)
        {
            return quest.id == id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(Name);
        hash.Add(id);
        hash.Add(description);
        hash.Add(state);
        hash.Add(timeToFail);
        hash.Add(TimeToFail);
        hash.Add(rewards);
        hash.Add(penalties);
        hash.Add(objectives);
        hash.Add(nextQuests);
        return hash.ToHashCode();
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

    public Quest(string _name, string _description, Objective _objective, int _id, int _timeToFail)
    {
        Name = _name;
        description = _description;
        objectives.Add(_objective);//; = new() { objective },
        id = -5;
        TimeToFail = 30;
    }
}

public class Order : Quest
{
    public int originalTimeToFail;
    public ResourceObjective orderObjective;

    public override void Load(QuestController controller, QuestSave save = null)
    {
        originalTimeToFail = TimeToFail;
        if (save != null)
            timeToFail = save.timeToFail;
        objectives[0].Load(save != null ? save.currentProgress[0] : 0, this, controller);
        orderObjective = objectives[0] as ResourceObjective;

        for (int i = 0; i < rewards.Count; i++)
        {
            rewards[i].Init();
        }
    }

    public override void Complete(bool success, QuestController controller)
    {
        state = success ? QuestState.Completed : QuestState.Failed;
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
        controller.order = null;
    }
    public Order() : base() { }

    public Order(Quest quest) : base(quest)
    {

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
