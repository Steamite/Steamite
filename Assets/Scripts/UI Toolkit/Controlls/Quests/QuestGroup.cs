using Objectives;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class QuestGroup : ScrollView, IUIElement
{
    public int minutesPerTick;
    public QuestGroup()
    {
        Quest quest = new();
        quest.Name = "Testing";
        quest.description = "test";
        quest.objectives.Add(new ExcavationObjective(new() { new GridPos(1,2,3)}, quest));
        Add(new QuestElement(quest, 0));
    }

    public void Open(object data)
    {
        minutesPerTick = SceneRefs.Tick.MinutesPerTick;
        Clear();
        QuestController questController = data as QuestController;
        foreach(Quest quest in questController.activeQuests)
        {
            Add(new QuestElement(quest, minutesPerTick));
        }
    }
}
