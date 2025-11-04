using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

public class QuestPenaltyEditor : QuestCompositorList<QuestPenalty>
{
    public QuestPenaltyEditor() : base()
    {
        columns.Add(new Column()
        {
            name = "ammount",
            title = "Ammount",
            makeCell = () => new IntegerField(),
            bindCell = (el, i) =>
            {
                IntegerField field = el as IntegerField;
                if (itemsSource[i] is QuestPenalty penalty)
                {
                    field.style.display = DisplayStyle.Flex;
                    field.value = penalty.penaltyAmmount;
                    field.RegisterValueChangedCallback(AmmountChange);
                }
                else
                    field.style.display = DisplayStyle.None;
            },
            unbindCell = (el, i) =>
            {
                IntegerField field = el as IntegerField;
                field.UnregisterValueChangedCallback(AmmountChange);
            },
            resizable = false,
            width = 100,
        });

        onAdd = (list) =>
        {
            data.penalties.Add(new QuestPenalty());
            EditorUtility.SetDirty(holder);
            RefreshItems();
        };

        onRemove = (list) =>
        {
            data.penalties.Remove(list.selectedItem as QuestPenalty);
            EditorUtility.SetDirty(holder);
            RefreshItems();
        };
    }

    public override void Bind(QuestHolder _holder, Quest _data, List<Type> _types)
    {
        base.Bind(_holder, _data, _types);
        itemsSource = _data.penalties;
    }

    void AmmountChange(ChangeEvent<int> ev)
    {
        int i = ev.target.GetRowIndex();
        if (ev.previousValue != ev.newValue)
        {
            QuestPenalty penalty = itemsSource[i] as QuestPenalty;
            penalty.penaltyAmmount = ev.newValue;
            EditorUtility.SetDirty(holder);
        }
    }
}
