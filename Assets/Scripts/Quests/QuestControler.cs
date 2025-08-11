using Objectives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class QuestController : MonoBehaviour, IQuestController, IGameDataController<QuestsSave>
{
    [SerializeField] public GameObject ExcavationIcon;
    public QuestHolder data;

    public List<Quest> finishedQuests;
    public ObservableCollection<Quest> activeQuests;


    List<Objective> objectives;
    public List<ExcavationObjective> ExcavationObjectives = new();
    public List<AnyExcavationObjective> AnyExcavationObjectives = new();
    public List<BuildingObjective> buildingObjectives = new();

    public void BuildBuilding(object obj)
    {
        for (int i = buildingObjectives.Count - 1; i > -1; i--)
        {
            buildingObjectives[i].UpdateProgress(obj, this);
        }
    }

    public void DigRock(object obj)
    {
        for (int i = ExcavationObjectives.Count - 1; i > -1; i--)
        {
            ExcavationObjectives[i].UpdateProgress(obj, this);
        }
        for (int i = AnyExcavationObjectives.Count - 1; i > -1; i--)
        {
            AnyExcavationObjectives[i].UpdateProgress(obj, this);
        }
    }
    public void ResChange()
    {
        throw new System.NotImplementedException();
    }

    public async Task LoadState(QuestsSave saveData)
    {
        activeQuests = new();
        finishedQuests = new();

        data = Instantiate(await Addressables.LoadAssetAsync<QuestHolder>("Assets/Game Data/UI/QuestData.asset").Task);
        List<Quest> quests = data.Categories.SelectMany(q => q.Objects).ToList();
        foreach (Quest quest in quests)
        {
            QuestSave save;
            if((save = saveData.activeQuests.FirstOrDefault(q => q.questId == quest.id)) != null)
            {
                quest.Load(save, this);
                activeQuests.Add(quest);
            }
            else if((save = saveData.finishedQuests.FirstOrDefault(q => q.questId == quest.id)) != null)
            {
                quest.state = save.state;
                finishedQuests.Add(quest);
            }
        }
        ((IUIElement)GetComponent<UIDocument>().rootVisualElement[3]).Open(this);
        SceneRefs.Tick.SubscribeToEvent(UpdateTimers, Tick.TimeEventType.Ticks);
    }

    public void UpdateTimers()
    {
        for (int i = activeQuests.Count-1; i > -1; i--)
        {
            activeQuests[i].DecreaseTimeToFail(this);
        }
    }

    public void AddDummy()
    {
        Quest quest = new Quest();
        quest.Name = "Dummy";
        quest.description = "persistent quest";
        quest.objectives.Add(new AnyExcavationObjective(3));
        QuestSave save = new()
        {
            questId = -5,
            state = QuestState.Active,
            currentProgress = new() { 0 },
            timeToFail = 30
        };
        quest.Load(save, this);
        activeQuests.Add(quest);
    }
}