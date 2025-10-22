using InfoWindowElements;
using Outposts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class SliderResList : DoubleResList, IUpdatable
{
    bool showFinalMoney = true;
    public SliderResList() { }
    public SliderResList(bool _cost, string _name, bool _useBindings = false) : base(_cost, _name, _useBindings)
    {
        style.minWidth = new Length(110, LengthUnit.Percent);
        fixedItemHeight = 88;
        delegatesFocus = true;
        itemTemplate = Resources.Load<VisualTreeAsset>("UI Toolkit/Deal");
    }
    /// <summary>
    /// first is index, second is val
    /// </summary>
    Action<List<int>> onSliderMove;

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    public override void Open(object data)
    {
        onSliderMove = null;
        switch (data)
        {
            case OutpostTradeElem tradeElem:
                onSliderMove = tradeElem.StoredSliderMove;
                Outpost outpost = tradeElem.InspectedOutpost;
                SetResWithoutBinding(outpost.storedResources);
                onSliderMove(GetVals());
                Rebuild();
                break;
        }
    }
    protected override void BindItem(VisualElement el, int i)
    {
        el.style.height = 88;
        el[0].style.height = 88;

        VisualElement deal = el.Q("Deal");
        if (deal.Q<Label>("Value") == null)
            deal.Insert(2, new Label("##") { name = "Value" });

        base.BindItem(el, i);
        SliderInt slider = el.Q<SliderInt>("Slider");
        slider.Q<TextField>().maxLength = 3;
        slider.Q("unity-tracker").style.top = 18;
        slider.Q("unity-dragger").style.top = 27;

        TextField field = slider.Q<TextField>();
        field.pickingMode = PickingMode.Position;
        field[0].pickingMode = PickingMode.Ignore;
        field[0][0].pickingMode = PickingMode.Ignore;

        slider.lowValue = 0;
        slider.value = (resources[i] as DoubleUIResource).secondAmmount;
        slider.highValue = resources[i].ammount;
        slider.RegisterValueChangedCallback(ev => OnSliderChange(ev, i));

        Label label = el.Q<Label>("Result");
        if (showFinalMoney)
        {
            label.style.display = DisplayStyle.Flex;

            var x = i;
            label.SetBinding(
                nameof(resources), nameof(Label.text),
                (ref List<UIResource> res) =>
                {
                    int cost = TradingWindow.RESOURCE_COSTS[resources[i].type];
                    return $"* {cost} = {(res[x] as DoubleUIResource).secondAmmount * cost}";
                },
                this);
        }
        else
        {
            label.style.display = DisplayStyle.None;
        }
    }

    protected override string ConvertString(UIResource resource)
    {
        return $" / {resource.ammount}";
    }

    protected override void SetResWithoutBinding(Resource res)
    {
        List<UIResource> temp = new List<UIResource>();
        if (res is MoneyResource money && showMoney && money.Money > 0)
            temp.Add(new DoubleUIResource(MyRes.Money, +money.Money));
        for (int i = 0; i < res.types.Count; i++)
        {
            temp.Add(new DoubleUIResource(
                res.ammounts[i], 0, res.types[i]));
        }
        resources = temp;
    }

    void OnSliderChange(ChangeEvent<int> ev, int index)
    {
        (resources[index] as DoubleUIResource).secondAmmount = ev.newValue;
        onSliderMove(GetVals());
        UIUpdate(nameof(resources));
    }

    public void Reset()
    {
        for (int i = 0; i < resources.Count; i++)
        {
            (resources[i] as DoubleUIResource).secondAmmount = 0;
        }
        resources = resources;
        onSliderMove(GetVals());
    }

    public void ChangeType(bool showCost)
    {
        showFinalMoney = showCost;
        RefreshItems();
    }
   
    public List<int> GetVals()
        => resources.Select(q => (q as DoubleUIResource).secondAmmount).ToList();

    public void UIUpdate(string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}