using System;
using System.Collections.Generic;
using System.Linq;

public class QuestsSave
{
    public List<QuestSave> finishedQuests;
    public List<QuestSave> activeQuests;
    public QuestsSave() { }


    public QuestsSave(QuestController controller) 
    {
        
    }

}

public class QuestSave
{
    public int questId;
    public List<int> currentProgress;
    public QuestState state;

    public QuestSave() { }
    public QuestSave(Quest quest)
    {
        questId = quest.id;
        currentProgress = quest.objectives.Select(q => q.CurrentProgress).ToList();
        state = quest.state;
    }
}