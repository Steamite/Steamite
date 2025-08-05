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
    [SerializeField] GameObject extractionIcon;
    [SerializeField] Quest quest;
    public QuestHolder data;

    [CreateProperty] public IEnumerable<DataObject> ActiveQuests { get => activeQuests; }
    List<Quest> finishedQuests;
    List<Quest> activeQuests;

    List<Objective> objectives;
    List<ExcavationObjective> excavationObjectives = new();

    public void BuildBuilding(object obj)
    {
        throw new System.NotImplementedException();
    }

    public void DigRock(object obj)
    {
        for (int i = excavationObjectives.Count - 1; i > - 1; i--)
        {
            excavationObjectives[i].UpdateProgress(obj);
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
        foreach (Quest quest in questHolder.Categories.SelectMany(q=> q.Objects))
        {
            QuestSave save;
            if((save = saveData.activeQuests.FirstOrDefault(q => q.questId == quest.id)) != null)
            {
                for (int i = 0; i < quest.objectives.Count; i++)
                {
                    quest.objectives[i].CurrentProgress = save.currentProgress[i];
                    if (quest.objectives[i] is ExcavationObjective excavation)
                    {
                        foreach (var item in excavation.needToRemove)
                        {
                            if (MyGrid.GetGridItem(item) is Rock rock)
                            {
                                Instantiate(extractionIcon, item.ToVec(3), Quaternion.identity, rock.transform);
                                rock.isQuest = true;
                            }
                        }
                    }
                }
                activeQuests.Add(quest);
            }
            else if((save = saveData.finishedQuests.FirstOrDefault(q => q.questId == quest.id)) != null)
            {
                quest.state = save.state;
                finishedQuests.Add(quest);
            }
        }
        /*
        activeQuests = new() { quest };
        ExcavationObjective objective = new ExcavationObjective(new() { new(9, 0, 5), new(10, 0, 5), new(9, 0, 6), new(10, 0, 6) }, quest);
        
        quest.objectives.Add(objective);
        excavationObjectives.Add(objective);

        ((IUIElement)GetComponent<UIDocument>().rootVisualElement[3]).Open(this);*/
    }
}