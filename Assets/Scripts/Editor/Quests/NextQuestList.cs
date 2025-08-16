using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
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

    public NextQuestList( QuestHolder _holder) : base()
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
            QuestLink link = (QuestLink)itemsSource[i];
            DropdownField field = el[0] as DropdownField;
            field.choices = categoryList;
            field.SetValueWithoutNotify(categoryList[link.categIndex+1]);
            field.userData = i;
            field.RegisterValueChangedCallback(NextQuestChangeCategory);

            field = el[1] as DropdownField;
            if (link.categIndex == -1)
                field.enabledSelf = false;
            else
            {
                field.enabledSelf = true;
                List<string> choices = new() { "Select" };
                if (link.questId != -1)
                {
                    string s = holder.Categories[link.categIndex].Objects.FirstOrDefault(q => q.id == link.questId).Name;
                    choices.Add(s);
                    field.SetValueWithoutNotify(s);
                }
                else
                    field.SetValueWithoutNotify("Select");
                choices.AddRange(holder.Categories[link.categIndex].availableObjects.Select(q => q.Name).Where(q => q != quest.Name));
                field.choices = choices;

                field.userData = i;
                field.RegisterValueChangedCallback(NextQuestChange);
            }
        };
        unbindItem = (el, i) =>
        {
            DropdownField field = el[0] as DropdownField;
            field.UnregisterValueChangedCallback(NextQuestChangeCategory);
        };

        onAdd = (list) =>
        {
            list.itemsSource.Add(new QuestLink(-1, -1));
            EditorUtility.SetDirty(holder);
        };
        onRemove = (list) =>
        {
            QuestLink link = selectedItem as QuestLink;
            list.itemsSource.Remove(selectedItem);
            if (link.questId != -1)
                holder.Categories[link.categIndex].availableObjects.Add(holder.Categories[link.categIndex].Objects.First(q => q.id == link.questId));
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

    void NextQuestChangeCategory(ChangeEvent<string> ev)
    {
        VisualElement element = ev.target as VisualElement;
        int i = (int)element.userData;
        QuestLink link = (QuestLink)itemsSource[i];
        if(link.questId != -1 && link.questId != -1)
        {
            Quest _quest = holder.Categories[link.categIndex].Objects.FirstOrDefault(q => q.id == link.questId);
            holder.Categories[link.categIndex].availableObjects.Add(_quest);
        }
        link.categIndex = categoryList.IndexOf(ev.newValue) - 1;
        link.questId = -1;

        triggerChange?.Invoke();
        EditorUtility.SetDirty(holder);
    }

    void NextQuestChange(ChangeEvent<string> ev)
    {
        VisualElement element = ev.target as VisualElement;
        int i = (int)element.userData;
        QuestLink link = (QuestLink)itemsSource[i];
        if(link.questId != -1)
            holder.Categories[link.categIndex].availableObjects.Add(holder.Categories[link.categIndex].Objects.FirstOrDefault(q => q.id == link.questId));

        if (ev.newValue == "Select")
        {
            link.questId = -1;
        }
        else
        {
            Quest _quest = holder.Categories[link.categIndex].Objects.FirstOrDefault(q => q.Name == ev.newValue);
            holder.Categories[link.categIndex].availableObjects.Remove(_quest);
            link.questId = _quest.id;
        }

        triggerChange?.Invoke();
        EditorUtility.SetDirty(holder);
    }

    public void CustomUnbind(ref Action _onChange)
    {
        _onChange -= RefreshItems;
    }
}
