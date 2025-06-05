using InfoWindowElements;
using ResearchUI;
using TradeData.Stats;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalMenu : MonoBehaviour, IAfterLoad
{
    VisualElement menu;
    Label header;
    Label secondHeader;
    DoubleResourceList costList;
    Label description;
    bool isOpen;
    public void Init()
    {
        menu = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Menu");
        ToolkitUtils.localMenu = this;
        header = menu.ElementAt(0) as Label;
        secondHeader = menu.ElementAt(1) as Label;
        costList = menu.ElementAt(2) as DoubleResourceList;
        description = menu.ElementAt(3) as Label;
    }

    /// <summary>
    /// Fills the local menu using <paramref name="data"/> positions it near the <paramref name="element"/>.
    /// If <paramref name="onlyUpdate"/> is false then also opens it.
    /// </summary>
    /// <param name="data">Data object.</param>
    /// <param name="element">positioning element</param>
    /// <param name="onlyUpdate">If true then don't open the window(only update if it was already visible).</param>
    public void UpdateContent(object data, VisualElement element, bool onlyUpdate = false)
    {
        Rect vec = new();
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
                description.text = node.description;
                break;
            case BuildingWrapper:
                BuildingWrapper wrapper = data as BuildingWrapper;
                Building building = wrapper.building;
                header.text = building.objectName;

                if (wrapper.unlocked)
                {
                    secondHeader.style.display = DisplayStyle.None;

                    costList.style.display = DisplayStyle.Flex;
                    costList.Open(building.cost);
                }
                else
                {
                    secondHeader.style.display = DisplayStyle.Flex;
                    secondHeader.text = "needs to be researched";

                    costList.style.display = DisplayStyle.None;
                }
                description.text = "";
                break;
        }

        vec = element.worldBound;
        menu.style.width = 300;
        menu.style.left = vec.x + vec.width + 25;
        menu.style.bottom = (Screen.height - element.worldBound.y) - element.resolvedStyle.height / 2;
        if(onlyUpdate == false)
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
        isOpen = false;
        menu.RegisterCallbackOnce<TransitionEndEvent>(
            (q) =>
            {
                if (isOpen == false) menu.style.display = DisplayStyle.None;
            });
        menu.RemoveFromClassList("show");
    }
}