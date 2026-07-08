using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace StartMenu
{
    public class MyMainMenu : PanelRendererRoot, IInitiableUI
    {
        VisualElement elements;
        VisualElement blocker;

        [SerializeReference] public List<MonoBehaviour> toolkitControllers;
        string lastStyle;

        bool instaLoad = false;

        protected override void OnUIReload()
        {
            elements = Root.Q<VisualElement>("Elements");
            blocker = Root.Q<VisualElement>("Main-Blocker");

            foreach (IToolkitController controller in toolkitControllers)
            {
                controller.Init(Root);
            }
#if UNITY_EDITOR
            if(instaLoad)
            {
                LoadGameMenu menu = toolkitControllers.Find(q => q is LoadGameMenu) as LoadGameMenu;
                menu.Continue(null);
                return;
            }
#endif
            // opening load menu
            Button button = Root.Q<Button>("Exit-Button");
            button.RegisterCallback<ClickEvent>((_) => Application.Quit());

            elements.RegisterCallback<TransitionEndEvent>(
                (eve) => { if (eve.target == elements) blocker.style.display = DisplayStyle.None; });
        }

        public void OpenWindow(string styleName)
        {
            blocker.style.display = DisplayStyle.Flex;
            elements.AddToClassList("menu-" + styleName);
            lastStyle = styleName;
        }


        public void CloseWindow()
        {
            if (lastStyle == "")
                return;
            blocker.style.display = DisplayStyle.Flex;
            elements.RemoveFromClassList("menu-" + lastStyle);
            lastStyle = "";
        }

        public void Init()
        {
            instaLoad = true;
        }
    }
}