using Objectives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class QuestController : FullscreenWindow, IQuestController, IGameDataController<QuestControllerSave>
{
    [SerializeField] public GameObject ExcavationIcon;
    [SerializeField] UIDocument _questCatalog;
    IUIElement questCatalog;
    [SerializeField] UIDocument _questInteface;
    IUIElement questInteface;
    IUIElement orderInterface;
    public QuestHolder data;

    public List<Quest> finishedQuests;
    public ObservableCollection<Quest> activeQuests;


    public List<ExcavationObjective> ExcavationObjectives = new();
    public List<AnyExcavationObjective> AnyExcavationObjectives = new();
    public List<BuildingObjective> buildingObjectives = new();
    public Order order;

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

    public async Task LoadState(QuestControllerSave saveData)
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
                quest.Load(this, save);
            }
            else if((save = saveData.finishedQuests.FirstOrDefault(q => q.questId == quest.id)) != null)
            {
                quest.Load(this, save);
            }
        }
        order = new(data.Categories[2].Objects.FirstOrDefault(q => q.id == saveData.order.questId));
        order.Load(this, saveData.order);
        SceneRefs.Tick.SubscribeToEvent(UpdateTimers, Tick.TimeEventType.Ticks);

        questInteface = _questInteface.rootVisualElement.Q("QuestGroup") as IUIElement;
        questInteface.Open(this);

        questCatalog = _questCatalog.rootVisualElement[0][0].Q("QuestCatalog") as IUIElement;

        orderInterface = _questCatalog.rootVisualElement[0][0].Q("OrderInterface") as IUIElement;
        GetWindow();
    }

    public void UpdateTimers()
    {
        for (int i = activeQuests.Count-1; i > -1; i--)
        {
            activeQuests[i].DecreaseTimeToFail(this);
        }
        order.DecreaseTimeToFail(this);
    }

    public void AddDummy()
    {
        AnyExcavationObjective objective = new AnyExcavationObjective(3);
        Quest quest = new Quest("Dummy", "persistent quest", objective, -5, 30);
        quest.Load(this);
    }

    #region Window
    public override void GetWindow()
    {
        base.GetWindow();
    }
    public override void OpenWindow()
    {
        base.OpenWindow();
        questCatalog.Open(this);
        orderInterface.Open(this);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
    }
    #endregion
}