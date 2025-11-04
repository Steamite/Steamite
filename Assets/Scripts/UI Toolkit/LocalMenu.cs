using InfoWindowElements;
using Outposts;
using ResearchUI;
using System.Collections.Generic;
using System.Linq;
using TradeData.Locations;
using TradeData.Stats;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalMenu : MonoBehaviour, IAfterLoad
{
    const int LOFFSET = 20; // Left offset
    const int ROFFSET = 25; // Right offset
    VisualElement anchor;
    object activeObject;

    VisualElement menu;
    Label header;
    Label secondHeader;
    DoubleResList costList;
    Label description;
    bool isOpen;

    int width = 300;
    public void AfterInit()
    {
        menu = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Menu");
        menu.pickingMode = PickingMode.Ignore;
        menu.style.display = DisplayStyle.None;
        ToolkitUtils.localMenu = this;
        header = menu.ElementAt(0) as Label;
        secondHeader = menu.ElementAt(1) as Label;
        costList = menu.ElementAt(2) as DoubleResList;
        description = menu.ElementAt(3) as Label;

        SceneRefs.InfoWindow.buildingCostChange = (building) =>
        {
            if (activeObject == null)
                return;
            if (building.Equals(((BuildingWrapper)activeObject).building))
                UpdateContent(activeObject, onlyUpdate: true);
        };
    }

    /// <summary>
    /// Fills the local menu using <paramref name="data"/> positions it near the <paramref name="element"/>.
    /// If <paramref name="onlyUpdate"/> is false then also opens it.
    /// </summary>
    /// <param name="data">Data object.</param>
    /// <param name="element">positioning element</param>
    /// <param name="onlyUpdate">If true then don't open the window(only update if it was already visible).</param>
    public void UpdateContent(object data, VisualElement element = null, bool onlyUpdate = false)
    {
        activeObject = data;
        if (element == null)
        {
            element = anchor;
            if (anchor == null)
            {
                Debug.LogWarning("No anchor to attach to.");
                return;
            }
        }
        else
            anchor = element;

        secondHeader.style.display = DisplayStyle.None;
        costList.style.display = DisplayStyle.None;
        width = 300;
        switch (data)
        {
            case ColonyStat stat:
                header.text = stat.name;
                if (element is Label)
                {
                    description.text = stat.GetText(true);
                }
                else
                {
                    costList.style.display = DisplayStyle.Flex;
                    costList.Open(stat.resourceUpgradeCost[element.parent.IndexOf(element)]);
                    description.text = stat.GetText(element.parent.IndexOf(element) + 1);
                }
                break;
            case ResearchNode node:
                header.text = node.Name;
                secondHeader.style.display = DisplayStyle.Flex;
                menu.style.width = 400;
                if (node.researched)
                {
                    secondHeader.text = "researched";
                }
                else
                {
                    if (node.CurrentTime < 0)
                    {
                        secondHeader.text = $"({0}/{node.researchTime})";
                        costList.style.display = DisplayStyle.Flex;
                        costList.Open(node.reseachCost);
                    }
                    else
                    {
                        secondHeader.text =
                            $"({node.CurrentTime}/{node.researchTime})\n" +
                            $"paid";
                    }
                }
                description.text = node.description.Replace('$', ' ');
                break;
            case BuildingWrapper wrapper:
                Building building = wrapper.building;
                header.text = building.objectName;

                if (wrapper.unlocked)
                {
                    costList.style.display = DisplayStyle.Flex;
                    costList.Open(building);
                }
                else
                {
                    secondHeader.style.display = DisplayStyle.Flex;
                    secondHeader.text = "needs to be researched";
                }
                description.text = "";
                break;
            case TradeLocation tradeLocation:
                header.text = tradeLocation.Name;
                secondHeader.text = "trade location";
                List<TradeConvoy> convoyList = UIRefs.TradingWindow.GetConvoys();
                TradeConvoy convoy = convoyList.FirstOrDefault(q => q.tradeLocation == UIRefs.TradingWindow.tradeLocations.IndexOf(tradeLocation));
                if (convoy != null)
                    description.text = convoy.ToString();
                else
                    description.text = "";
                break;
            case ColonyLocation colonyLocation:
                header.text = colonyLocation.Name;
                secondHeader.text = "colony";
                secondHeader.style.display = DisplayStyle.Flex;
                break;
            case Outpost outpost:
                width = 200;
                header.text = outpost.Name;
                secondHeader.text = "outpost";
                secondHeader.style.display = DisplayStyle.Flex;
                break;
            case Quest quest:
                header.text = quest.Name;
                description.text = quest.GetRewPenText();
                break;
            case ResourceType type:
                header.text = type.Name;
                description.text = "";
                break;
        }
        if (onlyUpdate == false)
        {
            Move();
            Show();
        }
    }

    void Show()
    {
        isOpen = true;
        menu.style.display = DisplayStyle.Flex;
        menu.AddToClassList("show");
    }

    public void Close()
    {
        activeObject = null;
        costList.ClearBindings();
        isOpen = false;
        menu.RegisterCallbackOnce<TransitionEndEvent>(
            (q) =>
            {
                if (isOpen == false)
                {
                    menu.style.display = DisplayStyle.None;
                    anchor = null;
                }
            });
        menu.RegisterCallbackOnce<TransitionCancelEvent>(
            (q) =>
            {
                if (isOpen == false)
                {
                    menu.style.display = DisplayStyle.None;
                    anchor = null;
                }
            });
        menu.RemoveFromClassList("show");
    }

    public void Move()
    {
        if (anchor != null)
        {
            Rect rect = anchor.worldBound;
            menu.style.width = width;
            float pos = rect.x + rect.width + LOFFSET;
            if (pos < 1620) // = 1920 - 300
            {
                menu.style.right = StyleKeyword.Auto;
                menu.style.left = pos;
            }
            else
            {
                menu.style.left = StyleKeyword.Auto;
                menu.style.right = 1920 - rect.x + ROFFSET; // = 1920 + OFFSET
            }
            float f = (1080 - rect.y) - anchor.resolvedStyle.height / 2;
            if (f < 1000)
            {
                menu.style.bottom = f;
                menu.style.top = StyleKeyword.Auto;
            }
            else
            {
                menu.style.top = 0;
                menu.style.bottom = StyleKeyword.Auto;
            }
        }
    }
}