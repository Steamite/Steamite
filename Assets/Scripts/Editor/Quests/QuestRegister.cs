using BuildingStats;
using Objectives;
using ResearchUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestRegister : DataGridWindow<QuestCategory, Quest>
{
    #region Opening

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
        rewardTypes = TypeCache.GetTypesDerivedFrom(typeof(QuestReward)).ToList();
        penaltyTypes = TypeCache.GetTypesDerivedFrom(typeof(QuestPenalty)).ToList();

        holder = AssetDatabase.LoadAssetAtPath<QuestHolder>("Assets/Game Data/UI/QuestData.asset");
        base.CreateGUI();
        categorySelector.index = 0;
    }

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
    }
    #endregion


    protected override void AddEntry(BaseListView _)
    {
        selectedCategory.Objects.Add(new Quest(holder.UniqueID()));
        base.AddEntry(_);
    }

    protected override void CreateColumns()
    {
        base.CreateColumns();


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
            makeCell = () => new DropdownField(),
            bindCell = (el, i) =>
            {
                DropdownField field = el as DropdownField;
                field.choices = questTypes.Select(q => q.Name).ToList();
                field.value = ((Quest)dataGrid.itemsSource[i]).GetType().ToString();
                field.RegisterValueChangedCallback(QuestTypeChange);
            },
            unbindCell = (el, i) =>
            {
                DropdownField field = el as DropdownField;
                field.UnregisterValueChangedCallback(QuestTypeChange);
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
    void QuestTypeChange(ChangeEvent<string> ev)
    {
        int i = ev.target.GetRowIndex();
        Quest prev = dataGrid.itemsSource[i] as Quest;
        if (prev != null)
        {
            Type t = questTypes.FirstOrDefault(q => q.Name == ev.newValue);
            if (t != null && prev.GetType() != t)
            {
                dataGrid.itemsSource[i] = Activator.CreateInstance(t, prev);
                EditorUtility.SetDirty(holder);
                dataGrid.RefreshItem(i);
            }
        }
    }
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
