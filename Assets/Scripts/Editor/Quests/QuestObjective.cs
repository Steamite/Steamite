using NUnit.Framework;
using Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class QuestObjective : MultiColumnListView
{
    QuestHolder holder;
    Quest data;
    List<Type> types;
    public QuestObjective() : base() 
    {
        showAddRemoveFooter = true;
        allowAdd = true;
        allowRemove = true;

        columns.Add(new Column()
        {
            title = "Type",
            makeCell = () =>
            {
                return new DropdownField()
                {
                    name = "type",
                    choices = types.Select(q => q.Name).ToList(),
                };
            },
            bindCell = (el, i) =>
            {
                DropdownField dropdownField = el as DropdownField;
                dropdownField.value = itemsSource[i].GetType().Name;
                dropdownField.RegisterValueChangedCallback(TypeChange);
            },
            unbindCell = (el, i) =>
            {
                DropdownField dropdownField = el as DropdownField;
                dropdownField.UnregisterValueChangedCallback(TypeChange);
            }
        });

        columns.Add(new Column()
        {
            title = "Max Progress",
            makeCell = () =>
            {
                IntegerField intField = new IntegerField();
                return intField;
            },
            bindCell = (el, i) =>
            {
                if (itemsSource[i] is not ExcavationObjective)
                {
                    IntegerField intField = el as IntegerField;
                    intField.style.display = DisplayStyle.Flex;
                    intField.value = (itemsSource[i] as Objective).MaxProgress;
                    intField.RegisterValueChangedCallback(MaxProgressChange);
                }
                else
                {
                    el.style.display = DisplayStyle.None;
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
            makeCell = () => new Button() {text = "Set Positions"},
            bindCell = (el, i) =>
            {
                if (itemsSource[i] is ExcavationObjective)
                {
                    Button button = el as Button;
                    button.clicked += () => ButtonClick(i);

                }
                else
                    el.style.display = DisplayStyle.None;
            },
            unbindCell = (el, i) =>
            {
                Button button = el as Button;
                button.clicked -= () => ButtonClick(i);
            }

        });


        onAdd = (list) =>
        {
            data.objectives.Add(new Objective());
            RefreshItems();
        };

        onRemove = (list) =>
        {
            data.objectives.Remove(list.selectedItem as Objective);
            RefreshItems();
        };
    }

    public void Bind(QuestHolder _holder, Quest _data, List<Type> _types)
    {
        holder = _holder;
        data = _data;
        types = _types;
        itemsSource = _data.objectives;
    }

    #region Changes
    void TypeChange(ChangeEvent<string> ev)
    {
        int i = ev.target.GetRowIndex();
        Type t = types.FirstOrDefault(q => q.Name == ev.newValue);
        if (t != null && data.GetType() != t)
        {
            data.objectives[i] = Activator.CreateInstance(t) as Objective;
            EditorUtility.SetDirty(holder);
            RefreshItem(i);
        }
    }
    void MaxProgressChange(ChangeEvent<int> ev)
    {
        int i = ev.target.GetRowIndex();
        if(ev.newValue != ev.previousValue)
        {
            (itemsSource[i] as Objective).MaxProgress = ev.newValue;
            EditorUtility.SetDirty(holder);
        }
    }
    void ButtonClick(int i)
    {
        EditorWindow win = EditorWindow.GetWindow(typeof(EmptyEditorWindow));
        GridPosList list = new GridPosList();
        win.rootVisualElement.Clear();
        win.rootVisualElement.Add(list);
        list.Bind(holder, ref (itemsSource[i] as ExcavationObjective).needToRemove);
    }
    #endregion
}