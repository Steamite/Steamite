using Objectives;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

[UxmlElement]
public partial class QuestElement : VisualElement
{
    Label nameLabel;
    Label descriptionLabel;
    Label timeLabel;
    VisualElement objectives;

    public QuestElement()
    {
        VisualElement header = new VisualElement();
        header.AddToClassList("quest-header");
        Add(header);
        
        header.Add(nameLabel = new("<u>Name"));
        nameLabel.AddToClassList("quest-name");

        header.Add(timeLabel = new("###"));
        timeLabel.AddToClassList("quest-time");

        
        Add(descriptionLabel = new("Lorem ipsum dolor sit amet"));
        descriptionLabel.AddToClassList("quest-description");
        
        Add(objectives = new());
        objectives.AddToClassList("quest-objective-view");
    }

    public void Open(Quest quest)
    {
        nameLabel.text = $"<u>{quest.Name}";
        descriptionLabel.text = MainShortcuts.ParseDescription(quest.description);
        timeLabel.ClearBindings();
        if (quest.TimeToFail != -1)
        {
            timeLabel.SetBinding(
                nameof(Quest.TimeToFail), 
                nameof(Label.text), 
                (ref int time) =>
                {
                    return Tick.RemainingTime(time);
                },
                quest);
        }
        else
        {
            timeLabel.text = "";
        }

        objectives.Clear();
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            var obj = quest.objectives[i];
            VisualElement elemGroup = new();
            elemGroup.AddToClassList("quest-objective-group");

            // Descr Label
            Label label = new(quest.objectives[i].Descr);
            label.AddToClassList("quest-objective-descr");
            elemGroup.Add(label);

            // Current Label
            label = new();
            label.AddToClassList("quest-objective");
            label.SetBinding(
                nameof(Objective.CurrentProgress),
                "text",
                (ref int x) =>
                {
                    if(x == obj.MaxProgress && elemGroup.parent.childCount > 1)
                    {
                        elemGroup.AddToClassList("completed");
                        elemGroup.RegisterCallbackOnce<TransitionEndEvent>((_ev) =>
                        {
                            elemGroup.parent.Remove(elemGroup);
                        });
                    }
                    return $"{x}/";
                },
                quest.objectives[i]);
            elemGroup.Add(label);

            // Max Label
            label = new();
            label.AddToClassList("quest-objective");
            label.SetBinding(
                nameof(Objective.MaxProgress),
                "text",
                (ref int x) => $"{x}",
                quest.objectives[i]);
            elemGroup.Add(label);
            objectives.Add(elemGroup);
        }
        RegisterCallback<MouseEnterEvent>((_) => ToolkitUtils.localMenu.UpdateContent(quest, this));
        RegisterCallback<MouseLeaveEvent>((_) => ToolkitUtils.localMenu.Close());
    }
}
