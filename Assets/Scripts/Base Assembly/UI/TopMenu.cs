using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

public class TopMenu : PanelRendererRoot, IAfterLoad
{
    ResourceDisplay resourceDisplay;

    /// <summary>Resource display on the top bar.</summary>
    IUIElement resourceList;
    /// <summary>Money bar in the top center.</summary>
    Label moneyLabel;

    protected override void OnUIReload()
    {
        base.OnUIReload();
        resourceDisplay = GetComponent<ResourceDisplay>();
        moneyLabel = Root.Q<Label>("Money-Value");
        resourceList = Root.Q<VisualElement>("Resources") as IUIElement;
    }

    public void AfterInit()
    {
        RegisterReload(Init);
    }

    void Init(PanelRenderer renderer, VisualElement element)
    {        
        moneyLabel.SetBinding(
            nameof(ResourceDisplay.Money), 
            nameof(Label.text), 
            (ref int _Money) => $"{_Money} <color=#FFD700>" + (char)163 + "</color>", 
            resourceDisplay);

        resourceList.Open(resourceDisplay);
    }
}
