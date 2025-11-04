using Objectives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class QuestInfo : ScrollView
{
    Label header;
    Label description;

    VisualElement dataGroup;
    TitleListView objectiveList;

    TitleListView penaltyList;
    TitleListView rewardList;

    public QuestInfo()
    {
        Add(header = new Label("Header"));
        header.AddToClassList("quest-header");
        Add(description = new Label("Description - Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque vel semper velit. Nulla luctus ac mauris ac molestie. In cursus, justo rhoncus lacinia placerat, sapien risus mattis quam, vel fermentum "));
        description.AddToClassList("quest-description");

        Add(dataGroup = new VisualElement());
        dataGroup.AddToClassList("end-group");
        dataGroup.Add(objectiveList = new TitleListView("Objectives", "objective-list"));


        VisualElement temp = new();
        temp.AddToClassList("bottom-row");

        temp.Add(penaltyList = new TitleListView("Penalty", "bottom-list"));
        temp.Add(rewardList = new TitleListView("Reward", "bottom-list"));

        dataGroup.Add(temp);
    }

    public void Open(Quest quest)
    {
        if (quest == null)
            style.display = DisplayStyle.None;
        else
        {
            style.display = DisplayStyle.Flex;
            header.text = quest.Name;
            description.text = MainShortcuts.ParseDescription(quest.description);

            objectiveList.Open(quest.objectives, quest.state == QuestState.Failed ? Color.red : null);
            penaltyList.Open(quest.penalties, quest.state == QuestState.Failed ? Color.red : null);
            rewardList.Open(quest.rewards, quest.state == QuestState.Completed ? Color.green : null);
        }
    }

    class TitleListView : VisualElement
    {
        public Label title;
        ListView listView;
        Color? color;
        Label none;
        public IList itemSource { set => listView.itemsSource = value; }
        public TitleListView() : base()
        {
            hierarchy.Add(title = new Label("Title"));
        }
        public TitleListView(string titleText, string className)
        {
            Add(title = new Label(titleText));
            title.AddToClassList("list-title");
            AddToClassList(className);
            Add(listView = new()
            {
                makeItem = () =>
                {
                    Label label = new Label();
                    label.AddToClassList("list-label");
                    return label;
                },
                bindItem = (el, i) =>
                {
                    (el as Label).text = listView.itemsSource[i].ToString();
                    if (listView.itemsSource[i] is Objective objective)
                    {
                        if (objective.CurrentProgress == objective.MaxProgress)
                        {
                            el.style.color = Color.green;
                            return;
                        }
                    }

                    if (color == null)
                        el.style.color = StyleKeyword.Null;
                    else
                        el.style.color = (Color)color;
                },
                makeNoneElement = () =>
                {
                    none = new("Empty") { style = { color = color != null ? (Color)color : StyleKeyword.Null } };
                    return none;
                },
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                selectionType = SelectionType.None
            });

        }

        public void Open<T>(List<T> list, Color? _color)
        {
            color = _color;
            itemSource = null;
            itemSource = list;

            if (color == null)
            {
                title.style.color = StyleKeyword.Null;
            }
            else
            {
                title.style.color = (Color)color;
            }
        }
    }
}
