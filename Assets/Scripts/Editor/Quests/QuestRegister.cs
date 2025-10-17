using BuildingStats;
using Objectives;
using Orders;
using ResearchUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Analytics.IAnalytic;

public class QuestRegister : DataGridWindow<QuestCategory, Quest>
{
    #region Opening
    ObjectField orderConfigField;

    List<Type> questTypes;
    List<Type> objectiveTypes;
    List<Type> rewardTypes;
    List<Type> penaltyTypes;

    /// <summary>Opens the window, if it's already opened close it.</summary>
    [MenuItem("Custom Editors/Building Quest Register %j", priority = 16)]
    public static void Open()
    {
        QuestRegister wnd = GetWindow<QuestRegister>();
        wnd.titleContent = new GUIContent("Quest Register");

    }

    /// <summary>Fills the button style and recalculates head placement</summary>
    protected override void CreateGUI()
    {
        questTypes = TypeCache.GetTypesDerivedFrom(typeof(Quest)).ToList();
        objectiveTypes = TypeCache.GetTypesDerivedFrom(typeof(Objective)).ToList();
        objectiveTypes.Remove(typeof(DummyObjective));
        rewardTypes = TypeCache.GetTypesDerivedFrom(typeof(QuestReward)).ToList();
        penaltyTypes = TypeCache.GetTypesDerivedFrom(typeof(QuestPenalty)).ToList();

        holder = AssetDatabase.LoadAssetAtPath<QuestHolder>("Assets/Game Data/UI/QuestData.asset");
        RecalculateAvailableObjects();

        base.CreateGUI();
        categorySelector.index = 0;
    }

    protected override void TopBar(out ObjectField iconSelector)
    {
        base.TopBar(out iconSelector);
        orderConfigField = rootVisualElement.Q<ObjectField>("Order-Config");
        orderConfigField.value = AssetDatabase.LoadAssetAtPath<OrderGenConfig>("Assets/Game Data/UI/OrderGenConfig.asset");
        orderConfigField.enabledSelf = false;
    }
    void RecalculateAvailableObjects()
    {
        IEnumerable<Quest> _quests = holder.Categories.SelectMany(q => q.Objects);
        Dictionary<int, List<int>> takenQuests = new();
        foreach (var item in _quests)
        {
            foreach (var nextQuest in item.nextQuests)
            {
                if (takenQuests.ContainsKey(nextQuest.categIndex))
                    takenQuests[nextQuest.categIndex].Add(nextQuest.questId);
                else
                    takenQuests.Add(nextQuest.categIndex, new() { nextQuest.questId});
            }
        }
        for (int i = 0; i < holder.Categories.Count; i++)
        {
            QuestCategory category = holder.Categories[i];
            category.availableObjects = category.Objects.ToList();
            if (takenQuests.ContainsKey(i))
            {
                foreach (var item in category.Objects)
                {
                    if (takenQuests[i].Contains(item.id))
                        category.availableObjects.Remove(item);
                }
            }
            
        }
    }/*
    protected override bool LoadCategData(int index)
    {
        bool boo = base.LoadCategData(index);
        if (boo)
        {

        }
        else
        {
            selectedCategory = new QuestCategory();
        }
        return boo;
    }*/
    #endregion

    Action onNextQuestChange;
    protected override void CreateColumns()
    {
        base.CreateColumns();
        onNextQuestChange = null;
        dataGrid.columns.Add(new Column()
        {
            name = "timeToFail",
            title = "Duration (Ticks)",
            makeCell = () => new IntegerField(),
            bindCell = (el, i) =>
            {
                IntegerField field = el as IntegerField;
                field.value = ((Quest)dataGrid.itemsSource[i]).TimeToFail;
                field.RegisterValueChangedCallback(TimeToFailChange);
            },
            unbindCell = (el, i) =>
            {
                IntegerField field = el as IntegerField;
                field.UnregisterValueChangedCallback(TimeToFailChange);
            },
            resizable = false,
            width = 100,
        });
        dataGrid.columns.Add(new Column()
        {
            name = "type",
            title = "Type",
            makeCell = () => 
            {
                NextQuestList list = new NextQuestList(holder as QuestHolder);
                return list;
            },
            
            bindCell = (el, i) =>
            {
                NextQuestList list = el as NextQuestList;
                list.Bind(selectedCategory.Objects[i], ref onNextQuestChange, () => onNextQuestChange?.Invoke());
            },
            unbindCell = (el, i) =>
            {
                (el as NextQuestList).CustomUnbind(ref onNextQuestChange);
            },
            resizable = false,
            width = 200,
        });
        dataGrid.columns.Add(new Column()
        {
            name = "description",
            title = "Description",
            makeCell = () => new TextField() { multiline = true, style = { whiteSpace = WhiteSpace.Normal } },
            bindCell = (el, i) =>
            {
                TextField field = el as TextField;
                field.value = ((Quest)dataGrid.itemsSource[i]).description;
                field.RegisterValueChangedCallback(DescriptionChange);
            },
            unbindCell = (el, i) =>
            {
                TextField field = el as TextField;
                field.UnregisterValueChangedCallback(DescriptionChange);
            },
            resizable = false,
            width = 100,
        });
        dataGrid.columns.Add(new Column()
        {
            name = "objective",
            title = "Objective",
            makeCell = () => new ObjectiveGridEditor(),
            bindCell = (el, i) =>
            {
                (el as ObjectiveGridEditor).Bind(
                        holder as QuestHolder,
                        dataGrid.itemsSource[i] as Quest,
                        objectiveTypes);
            },
            resizable = true,
            width = 600,
            
        });
        dataGrid.columns.Add(new Column()
        {

            name = "reward",
            title = "Reward",
            makeCell = () => new QuestRewardEditor(),
            bindCell = (el, i) =>
            {
                (el as QuestRewardEditor).Bind(
                        holder as QuestHolder,
                        dataGrid.itemsSource[i] as Quest,
                        rewardTypes);
            },
            resizable = true,
            width = 600,

        });

        dataGrid.columns.Add(new Column()
        {

            name = "penalty",
            title = "Penalty",
            makeCell = () => new QuestPenaltyEditor(),
            bindCell = (el, i) =>
            {
                (el as QuestPenaltyEditor).Bind(
                        holder as QuestHolder,
                        dataGrid.itemsSource[i] as Quest,
                        penaltyTypes);
            },
            resizable = true,
            width = 600,

        });
    }

    #region Change

    void TimeToFailChange(ChangeEvent<int> ev)
    {
        int i = ev.target.GetRowIndex();
        if (ev.previousValue != ev.newValue)
        {
            Quest quest = dataGrid.itemsSource[i] as Quest;
            quest.TimeToFail = ev.newValue;
            EditorUtility.SetDirty(holder);
        }
    }
    void DescriptionChange(ChangeEvent<string> ev)
    {
        int i = ev.target.GetRowIndex();
        if (ev.previousValue != ev.newValue)
        {
            Quest quest = dataGrid.itemsSource[i] as Quest;
            quest.description = ev.newValue;
            EditorUtility.SetDirty(holder);
        }
    }
    #endregion
}
