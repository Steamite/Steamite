using InfoWindowElements;
using System;
using UnityEngine.UIElements;

namespace InfoWindowViews
{
    /// <summary>
    /// Info window view mode for storage building buildings. <br/>
    /// Made up of two tabs: <bold>storage</bold> one and the <bold>level</bold> one(only for elevators).
    /// </summary>
    [UxmlElement]
    public partial class StorageView : TabView, IUIElement
    {
        #region Variables
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
        #endregion
        public StorageView()
        {
            #region Tabs
            style.flexGrow = 1;
            storageTab = new StorageTab(elemPref);
            Add(storageTab);

            levelTab = new LevelsTab();
            Add(levelTab);

            //activeTab = levelTab;
            #endregion
        }

        /// <inheritdoc/>
        public void Fill(object data)
        {
            storage = (Storage)data;
            switch (data)
            {
                case Elevator:
                    hierarchy.ElementAt(0).style.display = DisplayStyle.Flex;
                    levelTab.Open(storage);
                    break;
                case Storage:
                    activeTab = contentContainer.Q<Tab>("Storage");
                    hierarchy.ElementAt(0).style.display = DisplayStyle.None;
                    break;
                default:
                    throw new NotImplementedException();
            }
            storageTab.Open(storage);
        }
    }
}