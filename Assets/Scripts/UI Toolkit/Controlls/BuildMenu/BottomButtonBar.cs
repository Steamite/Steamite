using BottomBar.Building;
using UnityEngine;
using UnityEngine.UIElements;

namespace BottomBar
{
    [UxmlElement]
    public partial class BottomButtonBar : VisualElement, IInitiableUI
    {
        [UxmlAttribute] Texture2D buildSprite;
        [UxmlAttribute] Texture2D tradeSprite;
        [UxmlAttribute] Texture2D researchSprite;

        public BuildMenu buildMenu;
        public Button buildOpen;
        public Button researchOpen;
        public Button tradeOpen;
        public BottomButtonBar()
        {
            buildMenu = new();
            buildMenu.AddToClassList("build-menu");

            VisualElement element = new();
            element.AddToClassList("bottom-button-group");
            element.pickingMode = PickingMode.Ignore;
            buildOpen = new(buildMenu.Open);
            researchOpen = new();
            tradeOpen = new();

            Add(buildMenu);
            Add(element);
            element.Add(buildOpen);
            element.Add(researchOpen);
            element.Add(tradeOpen);
        }

        public void Init()
        {
            buildOpen.iconImage = buildSprite;
            researchOpen.clicked += UIRefs.ResearchWindow.OpenWindow;
            researchOpen.iconImage = researchSprite;
            tradeOpen.clicked += UIRefs.TradingWindow.OpenWindow;
            tradeOpen.iconImage = tradeSprite;
        }
    }
}