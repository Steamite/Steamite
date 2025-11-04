using System.Collections.Generic;
using UnityEngine.UIElements;

[UxmlElement]
public partial class QuestCatalog : TreeView, IUIElement
{
    int oldSelectionId;
    public QuestCatalog()
    {
        makeItem = () => new Label();
        bindItem = (el, i) =>
        {
            Quest quest = GetItemDataForIndex<Quest>(i);
            if (quest != null)
            {
                (el as Label).text = quest.Name;
                el.parent.parent.AddToClassList("tree-content");

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
                el.parent.parent.AddToClassList("category");

                el.userData = id;
                el.UnregisterCallback<ClickEvent>(ExpandEvent);
                el.RegisterCallback<ClickEvent>(ExpandEvent);
            }
        };

        unbindItem = (el, i) =>
        {
            int id = GetIdForIndex(i);
            el.parent.parent.RemoveFromClassList("category");
            el.parent.parent.RemoveFromClassList("tree-content");
        };

        selectionChanged += (items) =>
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    oldSelectionId = (item as Quest).id;
                    (parent[1] as QuestInfo).Open(item as Quest);
                }
                else
                    SetSelectionByIdWithoutNotify(new List<int>() { oldSelectionId });
                break;
            }
        };
        autoExpand = true;
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

        var finishedQuests = new List<TreeViewItemData<Quest>>();
        foreach (Quest item in controller.finishedQuests)
        {
            finishedQuests.Add(new(item.id, item));
        }
        items.Add(new(-2, null, finishedQuests));

        SetRootItems(items);
        Rebuild();

        selectedIndex = -1;
        (parent[1] as QuestInfo).Open(null);
    }

    public void ExpandEvent(ClickEvent ev)
    {
        if (ev.clickCount == 2)
        {
            int id = (int)(ev.target as VisualElement).userData;
            if (IsExpanded(id))
                CollapseItem(id);
            else
                ExpandItem(id);
            ev.StopPropagation();
        }
        ev.StopImmediatePropagation();
    }
}