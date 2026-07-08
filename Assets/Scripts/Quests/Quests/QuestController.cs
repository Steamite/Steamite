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

public class QuestController : IGameDataController<QuestControllerSave>, IQuestController, IUpdatable
{
    public GameObject ExcavationIcon;

    public QuestHolder data;
    public GameObject endMenu;
    public static int difficulty = 1;

    public OrderController orderController { get; set; }

    public List<Quest> finishedQuests;
    public ObservableCollection<Quest> activeQuests { get; set; }


    public List<ExcavationObjective> ExcavationObjectives = new();
    public List<AnyExcavationObjective> AnyExcavationObjectives = new();
    public List<BuildingObjective> buildingObjectives { get; set; } = new();

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    /// <summary>Company's trust in your leadership, if it falls to zero you lose.</summary>
    int trust;

    /// <inheritdoc cref="trust"/>
    [CreateProperty]
    public int Trust
    {
        get => trust;
        set
        {
            trust = value;
            UIUpdate(nameof(Trust));
            if (trust <= 0)
            {
                /// EndMenu.cs
                endMenu.GetComponent<IUIElement>().Open(false);
            }
        }
    }

    public bool IsLoaded { get; set; }

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

    public override async Task LoadState(QuestControllerSave saveData)
    {
        trust = saveData.trust;
        activeQuests = new();
        finishedQuests = new();

        data = Instantiate(await Addressables.LoadAssetAsync<QuestHolder>("QuestData").Task);
        List<Quest> quests = data.Categories.SelectMany(q => q.Objects).ToList();
        foreach (Quest quest in quests)
        {
            QuestSave save;
            if ((save = saveData.activeQuests.FirstOrDefault(q => q.objectId == quest.id)) != null)
            {
                quest.Load(this, save);
            }
            else if ((save = saveData.finishedQuests.FirstOrDefault(q => q.objectId == quest.id)) != null)
            {
                quest.Load(this, save);
            }
        }
        orderController = new(
                this,
                saveData);
        SceneRefs.Tick.SubscribeToEvent(UpdateTimers, Tick.TimeEventType.Ticks);


        IsLoaded = true;
        GetComponent<PanelRendererRoot>().Renderer.enabled = true;
    }

    public void UpdateTimers()
    {
        for (int i = activeQuests.Count - 1; i > -1; i--)
        {
            activeQuests[i].DecreaseTimeToFail(this);
        }
        orderController.UpdateTimers();
    }

    public void AddDummy()
    {
        AnyExcavationObjective objective = new AnyExcavationObjective(3);
        Quest quest = new Quest("Dummy", "persistent quest", objective, -5, 30);
        quest.Load(this);
    }

    public void UIUpdate(string property = "")
    {
        propertyChanged?.Invoke(this, new(property));
    }

    public override QuestControllerSave SaveState()
    {
        QuestControllerSave save = new()
        {
            finishedQuests = finishedQuests.Select(q => new QuestSave(q)).ToList(),
            activeQuests = activeQuests.Select(q => new QuestSave(q)).ToList(),
            order = orderController.CurrentOrder == null ? null : new(orderController.CurrentOrder),
            trust = Trust,
            finishedOrdersCount = orderController.finishedOrdersCount,
            orderChoiceSaves = orderController.orderChoice.Select(q => new OrderChoiceSave(q)).ToList()
        };
        return save;
    }
}