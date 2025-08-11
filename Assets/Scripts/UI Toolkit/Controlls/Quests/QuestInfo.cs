using Objectives;
using System.Collections;
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
        header.text = quest.Name;
        description.text = quest.description;
        objectiveList.itemSource = quest.objectives;
        penaltyList.itemSource = quest.penalties;
        rewardList.itemSource = quest.rewards;
    }

    class TitleListView : VisualElement
    {
        public Label title;
        ListView listView;
        public IList itemSource { set => listView.itemsSource = value; }
        public TitleListView() : base() 
        {
            hierarchy.Add(title = new Label("Title"));
        }
        public TitleListView(string titleText, string className)
        {
            Add(title = new Label(titleText));
            title.AddToClassList("temp-label");
            AddToClassList(className);
            Add(listView = new()
            {
                makeItem = () => new Label(),
                bindItem = (el, i) => (el as Label).text = listView.itemsSource[i].ToString(),
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            });
        }


        void Bind(VisualElement element, int i)
        {
            /*element.Clear();
            switch (itemsSource[i])
            {
                case Objective objective:
                    element.Add(new Label(objective.ToString()));
                    break;
                case QuestPenalty penalty:
                    element.Add(new Label(penalty.ToString()));
                    break;
                case QuestReward reward:
                    element.Add(new Label(reward.ToString()));
                    break;
            }*/
        }
        void BindObjective(VisualElement element, Objective objective)
        {
        }

        //void BindPenalty
    }
}
