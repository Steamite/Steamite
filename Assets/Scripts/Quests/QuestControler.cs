using Objectives;
using System.Collections.Generic;
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

    //[CreateProperty] public IEnumerable<DataObject> ActiveQuests { get => activeQuests; }
    public List<Quest> finishedQuests;
    public List<Quest> activeQuests;

    List<Objective> objectives;
    public List<ExcavationObjective> ExcavationObjectives = new();
    public List<AnyExcavationObjective> AnyExcavationObjectives = new();

    public void BuildBuilding(object obj)
    {
        throw new System.NotImplementedException();
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

        QuestHolder questHolder = Instantiate(await Addressables.LoadAssetAsync<QuestHolder>("Assets/Game Data/UI/QuestData.asset").Task);
        List<Quest> quests = questHolder.Categories.SelectMany(q => q.Objects).ToList();
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
}