using Objectives;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class QuestElement : VisualElement
{
    Label nameLabel;
    Label descriptionLabel;
/*    List<Label> currentLabels;
    List<Label> maxLabels;*/

    public QuestElement()
    {
        Add(nameLabel = new("<u>Name"));
        nameLabel.AddToClassList("quest-name");
        Add(descriptionLabel = new("Lorem ipsum dolor sit amet"));
        descriptionLabel.AddToClassList("quest-description");
    }

    public QuestElement(Quest quest)
    {
        Add(nameLabel = new($"<u>{quest.Name}"));
        nameLabel.AddToClassList("quest-name");
        Add(descriptionLabel = new($"{quest.description}"));
        descriptionLabel.AddToClassList("quest-description");
/*        currentLabels = new();
        maxLabels = new();*/
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            VisualElement elemGroup = new();
            elemGroup.AddToClassList("quest-objective-group");

            Label label = new();// $"{quest.objectives[i].CurrentProgress}/{quest.objectives[i].maxProgress}");
            label.AddToClassList("quest-objective");
            ConverterGroup group = new("strToInt");
            group.AddConverter((ref int x) => $"{x}/");
            label.SetBinding(
                nameof(Objective.CurrentProgress),
                "text",
                group,
                quest.objectives[i]);
            elemGroup.Add(label);

            label = new();// $"{quest.objectives[i].CurrentProgress}/{quest.objectives[i].maxProgress}");
            label.AddToClassList("quest-objective");
            group = new("strToInt");
            group.AddConverter((ref int x) => $"{x}");
            label.SetBinding(
                nameof(Objective.MaxProgress),
                "text",
                group,
                quest.objectives[i]);
            elemGroup.Add(label);
            Add(elemGroup);
        }
    }
}
