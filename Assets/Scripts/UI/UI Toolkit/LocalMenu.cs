using InfoWindowElements;
using System.Collections;
using System.Collections.Generic;
using TradeData.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalMenu : MonoBehaviour
{
    VisualElement menu;
    Label header;
    Label secondHeader;
	DoubleResourceList costList;
    Label description;
    private void Awake()
    {
        menu = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Menu");
        ToolkitUtils.localMenu = this;
        header = menu.ElementAt(0) as Label;
		secondHeader = menu.ElementAt(1) as Label;
		costList = menu.ElementAt(2) as DoubleResourceList;
		description = menu.ElementAt(3) as Label;

	}

    public void Open(object data, VisualElement element)
    {
        Rect vec;
		switch (data)
        {
            case ColonyStat:
                ColonyStat stat = data as ColonyStat;
                header.text = stat.name;
                secondHeader.style.display = DisplayStyle.None;
                if(element is Label)
                {
                    description.text = stat.GetText(true);
                    costList.style.display = DisplayStyle.None; 
                }
                else
                {
                    costList.style.display = DisplayStyle.Flex; 
                    costList.Open(stat.resourceUpgradeCost[element.parent.IndexOf(element)]);
                    description.text = stat.GetText(element.parent.IndexOf(element) + 1);
                }
                
                vec = element.worldBound;

                menu.style.width = 300;
                menu.style.left = vec.x + vec.width + 25;
                menu.style.bottom = (Screen.height - element.worldBound.y) - element.resolvedStyle.height / 2;
                menu.AddToClassList("show");
				break;
            case ResearchNode:
                ResearchNode node = data as ResearchNode;
				header.text = node.name;
				secondHeader.style.display = DisplayStyle.Flex;
                secondHeader.text = $"({node.currentTime}/{node.researchTime})";
				vec = element.worldBound;
				costList.style.display = DisplayStyle.Flex;
				costList.Open(node.reseachCost);

				description.text = node.description;
                

				menu.style.width = 300;
				menu.style.left = vec.x + vec.width + 25;
				menu.style.bottom = (Screen.height - element.worldBound.y) - element.resolvedStyle.height / 2;
				menu.AddToClassList("show");
				break;
        }
    }

    public void Close()
    {
		menu.RemoveFromClassList("show");
	}
}