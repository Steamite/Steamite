using AbstractControls;
using Outposts;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class TestList : ListView
{
    public TestList() : base()
    {
        //makeItem = () => new TextField() { style = { color = Color.red} };
        //bindItem = (el, i) => { (el as TextField).value = i.ToString(); };
        makeItem = () => {
            SliderInt slider = new SliderInt() { style = { color = Color.red } };
            slider.showInputField = true;
            return slider;
        };
        bindItem = (el, i) => { (el as SliderInt).value = i; };

        schedule.Execute(
            () =>
            {
                itemsSource = new List<int>() { 1, 2, 3, 4, 5 };
                Rebuild();
            }).ExecuteLater(1000);
    }
}

[UxmlElement]
public partial class OutpostTradeElem : VisualElement
{
    public Outpost InspectedOutpost => outpost;
    Outpost outpost;

    CustomRadioButtonGroup group;
    Button commitButton;
    SliderResList sliderRes;
    Label storageTitle;
    public OutpostTradeElem() { }

    public OutpostTradeElem(Outpost _outpost, Texture2D resetIcon)
    {
        outpost = _outpost;
        CustomRadioButton rButton;

        Add(storageTitle = new($"Storage ({outpost.storedResources.Sum()}/{outpost.storedResources.capacity})"));
        storageTitle.AddToClassList("level-label");

        group = new() { name = "button-group", style = {flexGrow = 0}};
        group.SetChangeCallback(StorageControlChange);
        rButton = new("choice-button", 0, group) { text = "Sell" };
        rButton = new("choice-button", 1, group) { text = "Collect" };

        Button button = new Button(ResetSliders) 
        { 
            style = 
            {
                maxWidth = 50,
                backgroundImage = resetIcon
            }
        };
        button.AddToClassList("choice-button");
        group.Add(button);
        Add(group);
        Add(sliderRes = new(false, "stored") { showEmpty = true});
        Add(commitButton = new(Commit) {
            style =
            {
                minWidth = new Length(75, LengthUnit.Percent),
                maxWidth = new Length(75, LengthUnit.Percent),
                marginBottom = 20,
                height = new Length(100, LengthUnit.Pixel),
                fontSize = 45,
                position = Position.Absolute,
                bottom = 0
            }
        });
        sliderRes.Open(this);
        //Add(new TestList());
        group.buttons[0].SelectWithoutTransition(true);
        
    }


    void ResetSliders()
    {
        if (sliderRes != null)
            sliderRes.Reset();
    }

    void StorageControlChange(int choice)
    {
        if (choice == 0)
        {
            commitButton.text = "sell";
            group.AddToClassList("sell");
            group.RemoveFromClassList("collect");
            sliderRes.ChangeType(true);
        }
        else
        {
            commitButton.text = "collect";
            group.AddToClassList("collect");
            group.RemoveFromClassList("sell");
            sliderRes.ChangeType(false);
        }
    }

    public void StoredSliderMove(List<int> vals)
    {
        int sum = vals.Sum();
        Debug.Log(sum);
        if(sum > 0)
        {
            commitButton.AddToClassList("main-button");
            commitButton.RemoveFromClassList("disabled-button");
        }
        else
        {
            commitButton.RemoveFromClassList("main-button");
            commitButton.AddToClassList("disabled-button");
        }
    }

    void Commit()
    {
        List<int> selectedVals = sliderRes.GetVals();
        if(selectedVals.Sum() > 0)
        {
            if (group.SelectedChoice == 0)
            {
                int money = 0;
                for (int i = 0; i < outpost.storedResources.types.Count; i++)
                {
                    ResourceType resType = outpost.storedResources.types[i];
                    outpost.storedResources.ManageSimple(
                        resType,
                        selectedVals[i],
                        false);
                    money += TradingWindow.RESOURCE_COSTS[resType] * selectedVals[i];
                }
                MyRes.ManageMoneyGlobal(money);
            }
            else
            {
                Resource resource = new(outpost.storedResources.types, selectedVals);
                outpost.storedResources.Manage(resource, false);
                MyRes.DeliverToElevator(resource);
            }
            storageTitle.text = $"Storage ({outpost.storedResources.Sum()} / {outpost.storedResources.capacity})";
            sliderRes.Open(this);
        }
    }
}