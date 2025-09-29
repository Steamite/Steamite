using InfoWindowElements;
using UnityEngine;
using UnityEngine.UIElements;
using Outposts;
using TradeWindowElements;
using System.Linq;
using AbstractControls;

[UxmlElement]
public partial class OutpostView : TradeMapViewBase
{
    [UxmlAttribute] Texture2D resetIcon;
    Outpost outpost;

    int outpostIndex;
    public TradeMap map;
    public OutpostView()
    {
    }

    void CreateNewView()
    {
        VisualElement element = new();
        element.AddToClassList("view");
        CreateUpgradeOutpost(element);
        Add(element);
    }

    void CreateConstructionView()
    {
        VisualElement constructionView = new();
        Label labelLevel;
        ProgressBar progress;
        constructionView.Add(labelLevel = new($"{outpost.level} >>> {outpost.level+1}"));
        labelLevel.AddToClassList("level-label");
        
        constructionView.Add(progress = new());
        progress.AddToClassList("construction-bar");
        progress.lowValue = 0;
        int time = Outpost.upgradeCosts[outpost.level].timeInTicks;
        progress.highValue = time;
        time -= outpost.timeToFinish;
        progress.value = time;
        progress.title = Tick.RemainingTimeWithoutBrackets(outpost.timeToFinish);

        Add(constructionView);
    }

    void CreateTradeView()
    {
        VisualElement container;
        VisualElement row;
        Label levelTitle;
        Label levelLabel;
        Label costListLabel;
        ResList resourceList;

        // level Label
        Add(container = new());
        container.style.flexGrow = 0;
        container.AddToClassList("view");
        container.Add(row = new());
        row.AddToClassList("row");
        row.Add(levelTitle = new("Level"));
        row.Add(levelLabel = new(outpost.level.ToString()));

        TabView view = new();
        Tab tab = new("Production");
        view.Add(tab);
        container = tab.contentContainer;
        container.AddToClassList("view");
        container.Add(costListLabel = new("Production per week"));
        costListLabel.AddToClassList("level-label");
        container.Add(resourceList = new() { name = "cost", cost = true});
        resourceList.Open(outpost);
        CreateUpgradeOutpost(container);


        view.Add(tab = new("Stored"));
        container = tab.contentContainer;
        container.AddToClassList("view");
        container.Add(new OutpostTradeElem(outpost, resetIcon));
        
        Add(view);
    }

    void CreateUpgradeOutpost(VisualElement root)
    {
        EnumField enumField;
        DoubleResList costList;
        Button unlockButton;

        VisualElement newView = new() 
        {
            style =
            {
                minWidth = new Length(100, LengthUnit.Percent),
                flexGrow = 1,
                alignItems = Align.Center
            }
        };
        root.Add(newView);
        newView.Add(enumField = new(ResourceType.None));


        newView.Add(costList = new(true, "cost") { showMoney = true });
        costList.Open(Outpost.upgradeCosts[outpost.level].resource);

        newView.Add(unlockButton = new()
        {
            style =
            {
                minWidth = new Length(75, LengthUnit.Percent),
                maxWidth = new Length(75, LengthUnit.Percent),
                marginBottom = 20,
                height = new Length(100, LengthUnit.Pixel),
                fontSize = 45,
                position = Position.Absolute,
                bottom = 0
            },
            text = "Create Outpost"
        });

        unlockButton.enabledSelf = false;
        unlockButton.AddToClassList("disabled-button");
        if (outpost.CanAffordUpgrade())
        {
            enumField.RegisterValueChangedCallback((ev) =>
            {
                if ((ResourceType)(object)ev.newValue == 0)
                {
                    unlockButton.RemoveFromClassList("main-button");
                    unlockButton.AddToClassList("disabled-button");
                    unlockButton.enabledSelf = false;
                }
                else
                {
                    unlockButton.AddToClassList("main-button");
                    unlockButton.RemoveFromClassList("disabled-button");
                    unlockButton.enabledSelf = true;
                }
            });
            unlockButton.clicked += () => CreateOutpost(enumField);
            unlockButton.text = "Construct";
        }
        else
        {
            unlockButton.text = "Not enough resources!";
        }

        newView.style.display = DisplayStyle.Flex;
    }

    public override object Open(int i)
    {
        base.Open();
        outpost = UIRefs.TradingWindow.outposts[i];
        outpostIndex = i;

        style.display = DisplayStyle.Flex;
        Clear();
        if (outpost.buildInProgress == true)
        {
            CreateConstructionView();
        }
        else if (outpost.level == 0)
        {
            CreateNewView();
        }
        else
        {
            Debug.Log("Trade");
            CreateTradeView();
        }
        return outpost;
    }

    void CreateOutpost(EnumField field)
    {
        outpost.StartUpgrade((ResourceType)field.value);
        Open(outpostIndex);
    }
}
