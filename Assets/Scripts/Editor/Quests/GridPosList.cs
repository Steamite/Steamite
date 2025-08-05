using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EmptyEditorWindow : EditorWindow
{
    
}
public class GridPosList : ListView
{
    QuestHolder holder;
    
    public GridPosList() : base()
    {
        style.flexGrow = 1;
        selectionType = SelectionType.Single;
        reorderable = false;
        virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

        makeItem = () =>
        {
            VisualElement visualElement = new();
            visualElement.style.flexDirection = FlexDirection.Row;
            visualElement.style.flexGrow = 1;
            visualElement.focusable = true;

            IntegerField field = new IntegerField("x");
            field.style.flexGrow = 1;
            field[0].style.minWidth = 10;
            field[0].style.maxWidth = 10;
            visualElement.Add(field);

            SliderInt slider = new SliderInt("y", 0, 5);
            slider.style.flexGrow = 1;
            slider[0].style.minWidth = 10;
            slider[0].style.maxWidth = 10;
            visualElement.Add(slider);

            field = new IntegerField("z");
            field.style.flexGrow = 1;
            field[0].style.minWidth = 10;
            field[0].style.maxWidth = 10;
            visualElement.Add(field);

            return visualElement;
        };

        bindItem = (el, i) =>
        {
            GridPos gridPos = itemsSource[i] as GridPos;
            IntegerField integerField = el[0] as IntegerField;
            integerField.value = (int)gridPos.x;
            integerField.RegisterValueChangedCallback(ChangeValue);

            SliderInt slider = el[1] as SliderInt;
            slider.showInputField = true;
            slider.value = gridPos.y;
            slider.RegisterValueChangedCallback(ChangeValue);

            integerField = el[2] as IntegerField;
            integerField.value = (int)gridPos.z;
            integerField.RegisterValueChangedCallback(ChangeValue);
        };

        unbindItem = (el, i) =>
        {
            (el[0] as IntegerField).RegisterValueChangedCallback(ChangeValue);
            (el[1] as SliderInt).RegisterValueChangedCallback(ChangeValue);
            (el[2] as IntegerField).RegisterValueChangedCallback(ChangeValue);
        };

        onAdd = (list) =>
        {
            itemsSource.Add(new GridPos());
            EditorUtility.SetDirty(holder);
            RefreshItems();
        };

        onRemove = (list) =>
        {
            if (list.selectedItem != null)
                itemsSource.Remove(list.selectedItem);
            else if(itemsSource.Count == 0)
                itemsSource.RemoveAt(list.itemsSource.Count - 1);
            EditorUtility.SetDirty(holder);
            RefreshItems();
        };
        allowAdd = true;
        allowRemove = true;
        showAddRemoveFooter = true;

    }
    
    public void Bind(QuestHolder _holder, ref List<GridPos> _itemsSource)
    {
        holder = _holder;
        itemsSource = _itemsSource;
    }

    void ChangeValue(ChangeEvent<int> ev)
    {
        int i = ev.target.GetRowIndex(false);
        int x = (ev.target as VisualElement).parent.IndexOf(ev.target as VisualElement);
        if (ev.previousValue != ev.newValue)
        {
            if(x == 0)
                (itemsSource[i] as GridPos).x = ev.newValue;
            else if (x == 1)
                (itemsSource[i] as GridPos).y = ev.newValue;
            else
                (itemsSource[i] as GridPos).z = ev.newValue;

            EditorUtility.SetDirty(holder);
        }
    }
}
