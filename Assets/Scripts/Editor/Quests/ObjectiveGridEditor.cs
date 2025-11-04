using Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ObjectiveGridEditor : QuestCompositorList<Objective>
{
    List<Type> buildingTypes = TypeCache.GetTypesDerivedFrom(typeof(Building)).ToList();
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
                else if (itemsSource[i] is ResourceObjective resource)
                    intField.isReadOnly = true;
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
            title = "Special",
            width = 150,
            resizable = false,
            stretchable = true,
            makeCell = () => new VisualElement() { style = { flexGrow = 1 } },
            bindCell = (el, i) =>
            {
                el.Clear();
                switch (itemsSource[i])
                {
                    case ExcavationObjective excavation:
                        Button button = new Button() { text = "set positions" };// style = { flexGrow = 1 } };
                        el.Add(button);
                        button.style.display = DisplayStyle.Flex;

                        GridPosList list = new GridPosList();
                        button.clicked += () => ButtonClick(i, list);
                        list.Bind(holder, ref excavation.needToRemove, (x) => RefreshItem(i));
                        break;
                    case ResourceObjective objective:
                        ResourceCell cell = new();
                        Button resButton;
                        el.Add(resButton = new Button() { text = "set Resource" });
                        resButton.clicked += () => ButtonClick(i, cell);
                        if (objective.resource == null)
                            objective.resource = new();
                        cell.Open(objective.resource, holder, true);
                        break;
                    case BuildingObjective building:
                        DropdownField field = new();
                        el.Add(field);
                        field.choices = buildingTypes.Select(q => q.Name).ToList();
                        field.value = building.BuildingTypeName;
                        field.RegisterValueChangedCallback(BuildingTypeChange);
                        break;
                }
            },
            unbindCell = (el, i) =>
            {
                el.Clear();
            }

        });


        onAdd = (list) =>
        {
            data.objectives.Add(new DummyObjective());
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
        if (_data.GetType() == typeof(Order))
        {
            _types = new() { typeof(ResourceObjective) };
        }

        base.Bind(_holder, _data, _types);
        itemsSource = _data.objectives;
    }
    #region Changes
    void MaxProgressChange(ChangeEvent<int> ev)
    {
        int i = ev.target.GetRowIndex();
        if (ev.newValue != ev.previousValue)
        {
            (itemsSource[i] as Objective).MaxProgress = ev.newValue;
            EditorUtility.SetDirty(holder);
        }
    }
    void BuildingTypeChange(ChangeEvent<string> ev)
    {
        int i = ev.target.GetRowIndex();
        if (ev.newValue != ev.previousValue)
        {
            (itemsSource[i] as BuildingObjective).BuildingTypeName = ev.newValue;
            EditorUtility.SetDirty(holder);
        }
    }
    #endregion
}