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
    private void Awake()
    {
        menu = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Menu");
        ToolkitUtils.localMenu = this;
    }

    public void Open(object data, VisualElement element)
    {
        switch (data)
        {
            case ColonyStat:
                ColonyStat stat = (ColonyStat)data;
                menu.style.opacity = 0;
                menu.style.display = DisplayStyle.Flex;
                menu.style.width = 300;
                ((Label)menu.ElementAt(0)).text = stat.name;
                ((DoubleResourceList)menu.ElementAt(1)).Open(stat.resourceUpgradeCost[element.parent.IndexOf(element)]);
                ((Label)menu.ElementAt(2)).text = stat.GetText(element.parent.IndexOf(element) + 1);
                
                Vector2 vec = element.worldBound.position;

                menu.style.left = vec.x + 75;
                menu.style.bottom = (ToolkitUtils.GetRoot(element).resolvedStyle.height - element.worldBound.y) - element.resolvedStyle.height / 2;
				menu.style.opacity = 100;
                break;
        }
    }

    public void Close()
    {
        menu.style.display = DisplayStyle.None;
    }
}