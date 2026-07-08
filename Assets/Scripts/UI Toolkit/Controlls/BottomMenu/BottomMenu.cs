using BottomBar;
using BottomBar.Building;
using ResearchUI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

namespace BottomBar
{
    public class BottomMenu : InitilizablePanelRenderer, IAfterLoad
    {
        BottomButtonBar bottomButtonBar;
        VisualElement questGroup;
        BuildMenu buildMenu;
        protected override void OnUIReload()
        {
            base.OnUIReload();
            bottomButtonBar = Root.Q("BottomButtonBar") as BottomButtonBar;

            questGroup = Root.Q("QuestGroup");

            buildMenu = (BuildMenu)UIRefs.BottomBarRoot.Q<VisualElement>(className: "build-menu");
        }

        public void AfterInit()
        {
            RegisterLoad();
        }

        protected override void OnDataLoadLogic()
        {
            bottomButtonBar.Init();
            ((IUIElement)questGroup).Open(SceneRefs.QuestController);
            buildMenu.Init(UIRefs.Research.GetResearchData());
        }
    }

}