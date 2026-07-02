using InfoWindowElements;
using Outposts;
using ResearchUI;
using System.Collections.Generic;
using System.Linq;
using TradeData.Locations;
using TradeData.Stats;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UIElements;

namespace LocalMenuUtility
{
    public abstract class LocalMenu : MonoBehaviour
    {
        protected const int LOFFSET = 20; // Left offset
        protected const int ROFFSET = 25; // Right offset
        protected object activeObject;

        protected VisualElement menu;
        protected Label header;
        protected Label secondHeader;
        protected DoubleResList costList;
        protected Label description;
        
        public bool isOpen;

        protected int width = 300;

        public virtual void AfterInit(VisualElement rootElem)
        {
            //ToolkitUtils.SetLocalMenu(this);

            menu = rootElem.Q<VisualElement>("Menu");
            menu.pickingMode = PickingMode.Ignore;
            menu.style.display = DisplayStyle.None;
            header = menu.ElementAt(0) as Label;
            secondHeader = menu.ElementAt(1) as Label;
            costList = menu.ElementAt(2) as DoubleResList;
            description = menu.ElementAt(3) as Label;
        }

        

        protected virtual void HandleData()
        {
            switch (activeObject)
            {
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
                /*case TradeConvoy convoy:
                    header.text = convoy.firstPhase ? "Going There" : "Coming back";
                    secondHeader.text = "";
                    List<TradeConvoy> convoyList = UIRefs.TradingWindow.GetConvoys();
                    TradeConvoy convoy = convoyList.FirstOrDefault(q => q.tradeLocation == UIRefs.TradingWindow.tradeLocations.IndexOf(tradeLocation));
                    if (convoy != null)
                        description.text = convoy.ToString();
                    else
                        description.text = "";
                    break;*/
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
        }


        protected void Show()
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
                        OnClose();
                });
            menu.RegisterCallbackOnce<TransitionCancelEvent>(
                (q) =>
                {
                    if (isOpen == false)
                        OnClose();
                });
            menu.RemoveFromClassList("show");
        }

        public virtual void OnClose()
        {
            menu.style.display = DisplayStyle.None;
            //anchor = null;
        }

        public abstract void Move();

        protected void MoveX(float xPos, float fallbackXPos)
        {
            if (xPos < 1620) // = 1920 - 300
            {
                menu.style.right = StyleKeyword.Auto;
                menu.style.left = xPos;
            }
            else
            {
                menu.style.left = StyleKeyword.Auto;
                menu.style.right = 1920 - fallbackXPos + ROFFSET; // = 1920 + OFFSET
            }
        }

        protected void MoveY(float yPos)
        {
            if (yPos < 1000)
            {
                menu.style.bottom = yPos;
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