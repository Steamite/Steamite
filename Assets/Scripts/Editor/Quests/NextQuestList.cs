using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

public class NextQuestList : ListView
{
    Quest quest;
    QuestHolder holder;
    List<string> categoryList;
    Action triggerChange;
    public NextQuestList() : base()
    {

    }

    public NextQuestList(QuestHolder _holder) : base()
    {
        holder = _holder;
        categoryList = holder.Categories.Select(q => q.Name).ToList();
        categoryList.Insert(0, "Select");
        makeItem = () =>
        {
            VisualElement element = new();
            element.style.flexDirection = FlexDirection.Row;
            element.Add(new DropdownField());
            element.Add(new DropdownField());
            return element;
        };
        bindItem = (el, i) =>
        {
            DataAssign link = (DataAssign)itemsSource[i];
            DropdownField field = el[0] as DropdownField;
            field.choices = categoryList;
            field.SetValueWithoutNotify(categoryList[holder.GetCategIndexById(link.categoryId, true)]);
            field.userData = i;
            field.RegisterValueChangedCallback(NextQuestCategoryChange);

            field = el[1] as DropdownField;
            if (link.categoryId <= 0)
                field.enabledSelf = false;
            else
            {
                field.enabledSelf = true;
                List<string> choices = new() { "Select" };
                if (link.objectId != -1)
                {
                    string s = holder.GetObjectBySaveIndex(link).Name;//.Categories[link.categIndex].Objects.FirstOrDefault(q => q.id == link.objectId).Name;
                    choices.Add(s);
                    field.SetValueWithoutNotify(s);
                }
                else
                    field.SetValueWithoutNotify("Select");
                choices.AddRange(holder.GetCategByID(link.categoryId).availableObjects.Where(q => q.Name != quest.Name).Select(q => q.Name));
                field.choices = choices;

                field.userData = i;
                field.RegisterValueChangedCallback(NextQuestChange);
            }
        };
        unbindItem = (el, i) =>
        {
            DropdownField field = el[0] as DropdownField;
            field.UnregisterValueChangedCallback(NextQuestCategoryChange);
        };

        onAdd = (list) =>
        {
            list.itemsSource.Add(new DataAssign(-1, -1));
            EditorUtility.SetDirty(holder);
        };
        onRemove = (list) =>
        {
            DataAssign link = (DataAssign)selectedItem; // as DataAssign;
            list.itemsSource.Remove(selectedItem);
            if (link.objectId != -1)
                holder.GetObjectBySaveIndex(link);//.Categories[link.categIndex].availableObjects.Add(holder.Categories[link.categIndex].Objects.First(q => q.id == link.objectId));
            triggerChange?.Invoke();
            EditorUtility.SetDirty(holder);
        };
        allowAdd = true;
        allowRemove = true;
        showAddRemoveFooter = true;
    }

    public void Bind(Quest _q, ref Action _onChange, Action _triggerChange)
    {
        _onChange += RefreshItems;
        triggerChange = _triggerChange;
        quest = _q;
        itemsSource = _q.nextQuests;
    }

    void NextQuestCategoryChange(ChangeEvent<string> ev)
    {
        VisualElement element = ev.target as VisualElement;
        int i = (int)element.userData;
        DataAssign link = (DataAssign)itemsSource[i];
        if (link.categoryId > 0 && link.objectId != -1)
        {
            Quest _quest = holder.GetObjectBySaveIndex(link);//.Categories[link.categIndex].Objects.FirstOrDefault(q => q.id == link.objectId);
            holder.GetCategByID(link.categoryId).availableObjects.Add(_quest);
        }
        link.categoryId = holder.GetCategIdFromName(ev.newValue);
        link.objectId = -1;

        itemsSource[i] = link;
        triggerChange?.Invoke();
        EditorUtility.SetDirty(holder);
    }

    void NextQuestChange(ChangeEvent<string> ev)
    {
        VisualElement element = ev.target as VisualElement;
        int i = (int)element.userData;
        DataAssign link = (DataAssign)itemsSource[i];
        if (link.objectId != -1)
            holder.GetCategByID(link.categoryId).availableObjects.Add(holder.GetObjectBySaveIndex(link));

        if (ev.newValue == "Select")
        {
            link.objectId = -1;
        }
        else
        {
            link = holder.GetSaveIndexByName(ev.newValue);
            Quest _quest = holder.GetObjectBySaveIndex(link);
            holder.GetCategByID(link.categoryId).availableObjects.Remove(_quest);
            link.objectId = _quest.id;
            itemsSource[i] = link;
        }

        triggerChange?.Invoke();
        EditorUtility.SetDirty(holder);
    }

    public void CustomUnbind(ref Action _onChange)
    {
        _onChange -= RefreshItems;
    }
}
