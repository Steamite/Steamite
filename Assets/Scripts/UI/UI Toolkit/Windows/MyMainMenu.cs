using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace StartMenu
{
    public class MyMainMenu : MonoBehaviour, IToolkitController
    {
        public VisualElement root;
        VisualElement elements;
        VisualElement blocker;

        [SerializeReference] public List<MonoBehaviour> toolkitControllers;
        string lastStyle;

        public void Init(VisualElement root)
        {
            foreach (IToolkitController controller in toolkitControllers)
            {
                controller.Init(root);
            }
        }

        void Start()
        {
            root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            elements = root.Q<VisualElement>("Elements");
            blocker = root.Q<VisualElement>("Main-Blocker");
            Init(root);

            // opening load menu
            Button button = root.Q<Button>("Exit-Button");
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
    }
}