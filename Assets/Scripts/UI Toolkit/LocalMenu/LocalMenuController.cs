
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UIElements;

namespace LocalMenuUtility
{
    public class LocalMenuController : MonoBehaviour
    {
        [SerializeField] VisualTreeAsset window;
        [SerializeField] PanelSettings settings;
        [SerializeField] int sortingOrder = 1000;

        const int MENU_COUNT = 3;
        readonly UIMenu[] uiMenus = new UIMenu[MENU_COUNT];
        WorldMenu worldMenu;

        static LocalMenuController instance;

        Dictionary<VisualElement, object> registeredEvents;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Clear() => instance = null;


        void Awake()
        {
            instance = this;
            registeredEvents = new();

            for (int i = 0; i < MENU_COUNT+1; i++)
            {
                GameObject menuObject = new();
                PanelRendererRoot root = menuObject.AddComponent<PanelRendererRoot>();
                root.Renderer.panelSettings = settings;
                root.Renderer.visualTreeAsset = window;
                root.Renderer.sortingOrder = sortingOrder;

                if(i < MENU_COUNT)
                {
                    UIMenu uiMenu = menuObject.AddComponent<UIMenu>();
                    root.RegisterReload(uiMenu.AfterInit);
                    uiMenus[i] = uiMenu;
                    menuObject.name = $"UI Menu: {i}";
                }
                else
                {
                    worldMenu = menuObject.AddComponent<WorldMenu>();
                    root.RegisterReload(worldMenu.AfterInit);
                    menuObject.name = $"World Menu";
                }
                menuObject.transform.parent = transform;
            }
        }

        public static void OpenUI(object data, VisualElement element = null, bool onlyUpdate = false)
            => instance.UI(data, element, onlyUpdate);

        void UI(object data, VisualElement element = null, bool onlyUpdate = false)
        {
            if (onlyUpdate)
            {
                UIMenu menu = GetMenuByObject(data);
                if (menu != null)
                {
                    menu.UpdateContent(data, element, onlyUpdate);
                    return;
                }
            }

            for (int i = 0; i < MENU_COUNT; i++)
            {
                if (uiMenus[i].isOpen)
                    continue;
                uiMenus[i].UpdateContent(data, element, onlyUpdate);
                return;
            }
        }

        public static void OpenWorld(object data)
            => instance.worldMenu.Open(data);

        public static void RegisterMouseEvents(VisualElement element, object data)
            => instance.RegisterEvents(element, data);
        public static void UnregisterMouseEvents(VisualElement element)
            => instance.UnregiterEvents(element);

        void RegisterEvents(VisualElement element, object data)
        {
            if (registeredEvents.ContainsKey(element))
            {
                registeredEvents[element] = data;
                return;
            }

            registeredEvents.Add(element, data);
            element.RegisterCallback<MouseEnterEvent>(EventOpen);
            element.RegisterCallback<MouseLeaveEvent>(EventClose);
        }


        
        void EventOpen(MouseEnterEvent evt)
        {
            VisualElement element = evt.target as VisualElement;
            object data = registeredEvents[element];
            UI(data, element);
            Debug.Log($"Opening {element.name}");
        }

        void EventClose(MouseLeaveEvent evt)
        {
            VisualElement element = evt.target as VisualElement;
            UIMenu menu = GetMenuByElement(element);
            menu.Close();
            Debug.Log($"Closing {element.name}");
        }

        UIMenu GetMenuByObject(object obj)
        {
            for (int i = 0; i < MENU_COUNT; i++)
            {
                if (uiMenus[i].isOpen && uiMenus[i].ActiveObject.Equals(obj)) 
                {
                    return uiMenus[i];
                }
            }
            return null;
        }
        UIMenu GetMenuByElement(VisualElement element)
        {
            for (int i = 0; i < MENU_COUNT; i++)
            {
                if (uiMenus[i].isOpen && uiMenus[i].Anchor == element)
                {
                    return uiMenus[i];
                }
            }
            return null;
        }


        public static void Close(VisualElement element)
            => instance.GetMenuByElement(element).Close();
        
        void UnregiterEvents(VisualElement element)
        {
            registeredEvents.Remove(element);
            element.RegisterCallback<MouseEnterEvent>(EventOpen);
            element.RegisterCallback<MouseLeaveEvent>(EventClose);
        }

        public static void MoveElement(VisualElement visualElement)
            => instance.Move(visualElement);
        void Move(VisualElement visualElement)
        {
            GetMenuByElement(visualElement)?.Move();
        }
    }
}
