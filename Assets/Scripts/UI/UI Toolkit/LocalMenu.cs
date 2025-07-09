using InfoWindowElements;
using ResearchUI;
using System.Collections.Generic;
using System.Linq;
using TradeData.Locations;
using TradeData.Stats;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalMenu : MonoBehaviour, IAfterLoad
{
    VisualElement anchor;
    object activeObject;

    VisualElement menu;
    Label header;
    Label secondHeader;
    DoubleResList costList;
    Label description;
    bool isOpen;


    public void Init()
    {
        menu = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Menu");
        menu.pickingMode = PickingMode.Ignore;
        ToolkitUtils.localMenu = this;
        header = menu.ElementAt(0) as Label;
        secondHeader = menu.ElementAt(1) as Label;
        costList = menu.ElementAt(2) as DoubleResList;
        description = menu.ElementAt(3) as Label;

        SceneRefs.infoWindow.buildingCostChange = (building) =>
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

        if (onlyUpdate == false)
        {
            Rect rect = element.worldBound;
            menu.style.width = 300;
            menu.style.left = rect.x + rect.width + 25;
            float f = (1080 - element.worldBound.y) - element.resolvedStyle.height / 2;
            menu.style.bottom = f;
            Debug.LogWarning("see this: " + Screen.height + ", " + f);
        }
        switch (data)
        {
            case ColonyStat:
                ColonyStat stat = data as ColonyStat;
                header.text = stat.name;
                secondHeader.style.display = DisplayStyle.None;
                if (element is Label)
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
                break;
            case ResearchNode:
                ResearchNode node = data as ResearchNode;
                header.text = node.Name;
                secondHeader.style.display = DisplayStyle.Flex;
                menu.style.width = 400;
                if (node.researched)
                {
                    secondHeader.text = "researched";
                    costList.style.display = DisplayStyle.None;
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
                        costList.style.display = DisplayStyle.None;
                        secondHeader.text =
                            $"({node.CurrentTime}/{node.researchTime})\n" +
                            $"paid";
                    }
                }
                description.text = node.description.Replace('$', ' ');
                break;
            case BuildingWrapper:
                BuildingWrapper wrapper = data as BuildingWrapper;
                Building building = wrapper.building;
                header.text = building.objectName;

                if (wrapper.unlocked)
                {
                    secondHeader.style.display = DisplayStyle.None;

                    costList.style.display = DisplayStyle.Flex;
                    costList.Open(building);
                }
                else
                {
                    secondHeader.style.display = DisplayStyle.Flex;
                    secondHeader.text = "needs to be researched";

                    costList.style.display = DisplayStyle.None;
                }
                description.text = "";
                break;
            case TradeLocation:
                TradeLocation location = data as TradeLocation;
                header.text = location.name;
                secondHeader.text = "trade location";
                costList.style.display = DisplayStyle.None;
                List<TradeConvoy> convoyList = UIRefs.trading.GetConvoys();
                TradeConvoy convoy = convoyList.FirstOrDefault(q => q.tradeLocation == UIRefs.trading.tradeLocations.IndexOf(location));
                if (convoy != null)
                    description.text = convoy.ToString();
                else
                    description.text = "";
                    break;
        }
        if (onlyUpdate == false)
            Show();

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
        anchor = null;
        costList.ClearBindings();
        isOpen = false;
        menu.RegisterCallbackOnce<TransitionEndEvent>(
            (q) =>
            {
                if (isOpen == false) menu.style.display = DisplayStyle.None;
            });
        menu.RemoveFromClassList("show");
    }
}