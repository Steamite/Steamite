

using TradeData.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LocalMenuUtility
{
    public class UIMenu : LocalMenu
    {
        VisualElement anchor;
        public object ActiveObject => activeObject;

        public VisualElement Anchor => anchor;

        protected override void OnUIReload()
        {
            base.OnUIReload();

            InfoWindow.Window.buildingCostChange = (building) =>
            {
                if (activeObject == null)
                    return;
                if (building.Equals(((BuildingWrapper)activeObject).Building))
                    UpdateContent(activeObject, onlyUpdate: true);
            };
        }

        /*
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
        */
        public override void Move()
        {
            if (anchor != null)
            {
                Rect rect = anchor.worldBound;
                menu.style.width = width;
                float xPosL = rect.x + rect.width + LOFFSET;
                MoveX(xPosL, rect.x);

                float yPos = (1080 - rect.y) - anchor.resolvedStyle.height / 2;
                MoveY(yPos);
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            anchor = null;
        }

        protected override void HandleData()
        {
            base.HandleData();
            switch (activeObject)
            {
                case ColonyStat stat:
                    header.text = stat.name;
                    if (anchor is Label)
                    {
                        description.text = stat.GetText(true);
                    }
                    else
                    {
                        costList.Open(stat.resourceUpgradeCost[anchor.parent.IndexOf(anchor)]);
                        description.text = stat.GetText(anchor.parent.IndexOf(anchor) + 1);
                    }
                    break;
                case GradientMouseData gradientMouse:
                    header.text = "test";
                    secondHeader.text = gradientMouse.data.ToString();
                    description.text = "";

                    secondHeader.style.display = DisplayStyle.Flex;
                    costList.style.display = DisplayStyle.None;
                    break;
            }
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

            HandleData();

            width = 300;

            if (onlyUpdate == false)
            {
                Move();
                Show();
            }
        }
    }
}
