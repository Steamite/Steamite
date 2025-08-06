using Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

public class QuestRewardEditor : QuestCompositorList<QuestReward>
{
    public QuestRewardEditor() : base()
    {
        columns.Add(new()
        {
            name = "resource",
            title = "Resource",
            width = 200,
            makeCell = () => new Button() { text = "Set Reward"},
            bindCell = (el, i) =>
            {
                if (itemsSource[i] is QuestResourceReward resourceReward)
                {
                    el.style.display = DisplayStyle.Flex;
                    ResCell cell = new ResCell();
                    (el as Button).clicked += () => ButtonClick(i, cell);
                    cell.Open(resourceReward.resource, holder, true);
                }
                else
                    el.style.display = DisplayStyle.None;
            }
        });
        onAdd = (list) =>
        {
            data.rewards.Add(new QuestReward());
            RefreshItems();
        };

        onRemove = (list) =>
        {
            data.rewards.Remove(list.selectedItem as QuestReward);
            RefreshItems();
        };
    }

    public override void Bind(QuestHolder _holder, Quest _data, List<Type> _types)
    {
        base.Bind(_holder, _data, _types);
        itemsSource = _data.rewards;
    }

    #region Changes
    
    #endregion
}