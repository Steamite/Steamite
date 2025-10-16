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
            makeCell = () => new(),
            bindCell = (el, i) =>
            {
                if (itemsSource[i] is QuestResourceReward resourceReward)
                {
                    el.style.display = DisplayStyle.Flex;
                    ResourceCell cell = new ResourceCell();
                    Button button;
                    el.Add(button = new() { text = "Set Reward" });
                    button.clicked += () => ButtonClick(i, cell);

                    cell.Open(resourceReward.resource, holder, true);
                }
                else if (itemsSource[i] is TrustReward trustReward)
                {
                    el.style.display = DisplayStyle.Flex;
                    IntegerField integerField;
                    el.Add(integerField = new());
                    integerField.value = trustReward.gainAmmount;
                    integerField.RegisterValueChangedCallback<int>(ev =>
                    {
                        trustReward.gainAmmount = ev.newValue;
                        EditorUtility.SetDirty(holder);
                    });
                }
                else
                    el.style.display = DisplayStyle.None;
            },
            unbindCell = (el, i) =>
            {
                el.Clear();
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