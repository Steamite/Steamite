using BottomBar.Building;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BottomBar
{
    [UxmlElement]
    public partial class BottomButtonBar : VisualElement, IInitiableUI
    {
        [UxmlAttribute] VectorImage buildSprite;
        [UxmlAttribute] VectorImage tradeSprite;
        [UxmlAttribute] VectorImage researchSprite;
        [UxmlAttribute] VectorImage questSprite;

        public BuildMenu buildMenu;
        public Button buildOpen;
        public Button researchOpen;
        public Button tradeOpen;
        public Button questOpen;
        public BottomButtonBar()
        {
            buildMenu = new();
            buildMenu.AddToClassList("build-menu");

            VisualElement element = new();
            element.AddToClassList("bottom-button-group");
            element.pickingMode = PickingMode.Ignore;
            CreateButton(element, out buildOpen, buildMenu.Toggle);
            CreateButton(element, out researchOpen);
            CreateButton(element, out tradeOpen);
            CreateButton(element, out questOpen);
            Add(element);
            Add(buildMenu);
        }

        void CreateButton(VisualElement parent, out Button button, Action action = null)
        {
            button = new(action);
            button.AddToClassList("bottom-button");
            button.Add(new VisualElement() { pickingMode = PickingMode.Ignore });
            parent.Add(button);
        }
        void SetImage(Button button, VectorImage image, Action action = null)
        {
            if(action != null)
                button.clicked += action;
            button[0].style.backgroundImage = Background.FromVectorImage(image);
            button[0].style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        }
        public void Init()
        {
            SetImage(buildOpen, buildSprite);
            SetImage(researchOpen, researchSprite, UIRefs.ResearchWindow.OpenWindow);
            SetImage(tradeOpen, tradeSprite, UIRefs.TradingWindow.OpenWindow);
            SetImage(questOpen, questSprite, UIRefs.Quests.OpenWindow);
        }
    }
}