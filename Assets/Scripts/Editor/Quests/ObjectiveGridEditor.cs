using NUnit.Framework;
using Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ObjectiveGridEditor : QuestCompositorList<Objective>
{
    public ObjectiveGridEditor() : base() 
    {
        columns.Add(new Column()
        {
            title = "Max Progress",
            width = 125,
            resizable = false,
            stretchable = true,
            makeCell = () =>
            {
                IntegerField intField = new IntegerField();
                return intField;
            },
            bindCell = (el, i) =>
            {
                
                IntegerField intField = el as IntegerField;
                if (itemsSource[i] is ExcavationObjective excavation)
                {
                    intField.SetValueWithoutNotify((itemsSource[i] as ExcavationObjective).needToRemove.Count);
                    intField.isReadOnly = true;
                }
                else
                {
                    intField.SetValueWithoutNotify((itemsSource[i] as Objective).MaxProgress);
                    intField.isReadOnly = false;
                    intField.UnregisterValueChangedCallback(MaxProgressChange);
                    intField.RegisterValueChangedCallback(MaxProgressChange);
                }
            },
            unbindCell = (el, i) =>
            {
                IntegerField intField = el as IntegerField;
                intField.UnregisterValueChangedCallback(MaxProgressChange);
            }
        });

        columns.Add(new Column()
        {
            title = "Positions",
            width = 150,
            resizable = false,
            stretchable = true,
            makeCell = () => new Button() { text = "Set Positions" },
            bindCell = (el, i) =>
            {
                if (itemsSource[i] is ExcavationObjective objective)
                {
                    Button button = el as Button;
                    button.style.display = DisplayStyle.Flex;
                    GridPosList list = new GridPosList();
                    button.clicked += () => ButtonClick(i, list);
                    list.Bind(holder, ref objective.needToRemove, (x) => RefreshItem(i));

                }
                else
                    el.style.display = DisplayStyle.None;
            },
            unbindCell = (el, i) =>
            {
                Button button = el as Button;
                GridPosList list = new GridPosList();
                button.clicked -= () => ButtonClick(i, list);
            }

        });

        onAdd = (list) =>
        {
            data.objectives.Add(new Objective());
            EditorUtility.SetDirty(holder);
            RefreshItems();
        };

        onRemove = (list) =>
        {
            data.objectives.Remove(list.selectedItem as Objective);
            EditorUtility.SetDirty(holder);
            RefreshItems();
        };
    }

    public override void Bind(QuestHolder _holder, Quest _data, List<Type> _types)
    {
        base.Bind(_holder, _data, _types);
        itemsSource = _data.objectives;
    }
    #region Changes
    void MaxProgressChange(ChangeEvent<int> ev)
    {
        int i = ev.target.GetRowIndex();
        if(ev.newValue != ev.previousValue)
        {
            (itemsSource[i] as Objective).MaxProgress = ev.newValue;
            EditorUtility.SetDirty(holder);
        }
    }
    #endregion
}