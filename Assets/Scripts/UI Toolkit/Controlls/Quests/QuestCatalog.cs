using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class QuestCatalog : TreeView, IUIElement
{
    public QuestCatalog()
    {
        makeItem = () => new Label();
        bindItem = (el, i) =>
        {
            Quest quest = GetItemDataForIndex<Quest>(i);
            if (quest != null)
            {
                (el as Label).text = quest.Name;
            }
            else
            {
                int id = GetIdForIndex(i);
                string s = "";
                switch (id)
                {
                    case -1:
                        s = "Active Quests";
                        break;
                    case -2:
                        s = "Completed Quests";
                        break;
                    case -3:
                        s = "Failed Quests";
                        break;
                }
                (el as Label).text = s;
            }
        };

        selectionChanged += (items) => 
        {
            foreach (var item in items)
            {
                if(item != null)
                    (parent[1] as QuestInfo).Open(item as Quest);
                break;
            }
        };
    }

    public void Open(object data)
    {
        QuestController controller = data as QuestController;
        var items = new List<TreeViewItemData<Quest>>(10);
        
        var activeQuests = new List<TreeViewItemData<Quest>>();
        foreach (Quest item in controller.activeQuests)
        {
            activeQuests.Add(new(item.id, item));
        }
        items.Add(new(-1, null, activeQuests));
        SetRootItems(items);
        Rebuild();
    }
}