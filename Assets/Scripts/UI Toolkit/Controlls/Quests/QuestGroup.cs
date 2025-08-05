using Objectives;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class QuestGroup : ScrollView, IUIElement
{
    public QuestGroup()
    {
        Quest quest = new();
        quest.Name = "Testing";
        quest.description = "test";
        quest.objectives.Add(new ExcavationObjective(new() { new GridPos(1,2,3)}, quest));
        Add(new QuestElement(quest));
    }

    public void Open(object data)
    {
        Clear();
        QuestController questController = data as QuestController;
        foreach(Quest quest in questController.ActiveQuests)
        {
            Add(new QuestElement(quest));
        }
    }
}
