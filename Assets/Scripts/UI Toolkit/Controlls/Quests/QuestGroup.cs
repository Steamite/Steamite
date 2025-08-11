using Objectives;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine.UIElements;

[UxmlElement]
public partial class QuestGroup : ScrollView, IUIElement
{
    public int minutesPerTick;
    List<VisualElement> elements;
    List<Quest> quests;


    public QuestGroup()
    {
        elements = new();
        quests = new();
        pickingMode = PickingMode.Ignore;
        visible = true;
        Quest quest = new();
        quest.Name = "Testing";
        quest.description = "test";
        quest.objectives.Add(new ExcavationObjective(new() { new GridPos(1, 2, 3) }, quest));
    }

    public void Open(object data)
    {
        QuestController questController = data as QuestController;
        questController.activeQuests.CollectionChanged += 
            (obj, ev) => 
            {
                if (ev.Action == NotifyCollectionChangedAction.Add)
                {
                    Quest quest = ev.NewItems[0] as Quest;
                    CreateItem(quest);
                }
                else if (ev.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (Quest quest in ev.OldItems)
                    {
                        int i = quests.IndexOf(quest);
                        if (quest.state == QuestState.Completed)
                            elements[i].AddToClassList("completed");
                        else if (quest.state == QuestState.Failed)
                            elements[i].AddToClassList("failed");

                        var q = quest;
                        elements[i].RegisterCallbackOnce<TransitionEndEvent>((_ev) =>
                        {
                            RemoveItem(i);
                        });
                    }
                }
            };
        foreach(Quest quest in questController.activeQuests)
            CreateItem(quest);
    }

    public void CreateItem(Quest quest)
    {
        QuestElement element = new QuestElement();
        elements.Add(element);
        quests.Add(quest);

        Add(element);
        element.Open(quest);
    }

    public void RemoveItem(int i)
    {
        quests.RemoveAt(i);
        elements[i].parent.Remove(elements[i]);
        elements.RemoveAt(i);
    }
}
