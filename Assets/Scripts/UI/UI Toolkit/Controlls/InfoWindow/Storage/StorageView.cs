using InfoWindowElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfoWindowViews
{
    [UxmlElement]
    public partial class StorageView : TabView, IUIElement
    {
#if UNITY_EDITOR
        bool active;
        [UxmlAttribute]
        bool storageTabActive
        {
            get => active;
            set
            {
                active = value;
                if (active)
                    activeTab = storageTab;
                else
                    activeTab = levelTab;
            }
        }
#endif

        [UxmlAttribute] public VisualTreeAsset elemPref;

        StorageTab storageTab;
        LevelsTab levelTab;
        Storage storage;
        public StorageView()
        {
            #region Tabs
            style.flexGrow = 1;
            storageTab = new StorageTab();
            Add(storageTab);

            levelTab = new LevelsTab();
            Add(levelTab);

            //activeTab = levelTab;
            #endregion
        }

        public void Fill(object data)
        {
            switch (data)
            {
                case Elevator:
                    hierarchy.ElementAt(0).style.display = DisplayStyle.Flex;
                    break;
                case Storage:
                    activeTab = contentContainer.Q<Tab>("Storage");
                    hierarchy.ElementAt(0).style.display = DisplayStyle.None;
                    break;
                default:
                    throw new NotImplementedException();
            }
            storage = (Storage)data;
            storageTab.Open(storage, elemPref);
            levelTab.Open(storage);
        }
    }
}