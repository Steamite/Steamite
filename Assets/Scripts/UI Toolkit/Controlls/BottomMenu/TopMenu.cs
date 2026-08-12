

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
[UnityEngine.RequireComponent(typeof(TimeDisplay))]

[UnityEngine.RequireComponent(typeof(ResourceDisplay))]
public class TopMenu : InitilizablePanelRenderer, IAfterLoad
{
    TimeDisplay timeDisplay;
    ResourceDisplay resourceDisplay;

    /// <summary>Resource display on the top bar.</summary>
    IUIElement resourceList;
    /// <summary>Money bar in the top center.</summary>
    Label moneyLabel;

    protected override void OnUIReload()
    {
        base.OnUIReload();
        resourceDisplay = GetComponent<ResourceDisplay>();
        timeDisplay = GetComponent<TimeDisplay>();

        moneyLabel = Root.Q<Label>("Money-Value");
        resourceList = Root.Q<VisualElement>("Resources") as IUIElement;
        timeDisplay.Init(Root);
    }

    public void AfterLoad()
    {
        RegisterLoad();
    }

    protected override void OnDataLoadLogic()
    {   
        moneyLabel.SetBinding(
            nameof(ResourceDisplay.Money), 
            nameof(Label.text), 
            (ref int _Money) => $"{_Money} <color=#FFD700>" + (char)163 + "</color>", 
            resourceDisplay);

        resourceList.Open(resourceDisplay);
    }
}
