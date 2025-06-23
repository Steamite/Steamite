using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    /// <summary>Tab for resource storing managment.</summary>
    [UxmlElement]
    public partial class StorageInfo : InfoWindowControl
    {
        /// <summary>Stores references for one resource type in view.</summary>
        struct StorageElem
        {
            /// <summary>Icon reference</summary>
            public VisualElement icon;
            /// <summary>Ammount label reference</summary>
            public Label label;
            /// <summary>Toggle buttons reference</summary>
            public ToggleButtonGroup canStore;

            public StorageElem(VisualElement element)
            {
                icon = element.Q<VisualElement>("Icon");
                label = element.Q<Label>("Ammount");
                canStore = element.Q<ToggleButtonGroup>("Can-Store");
            }
        }

        #region Variables
        /// <summary>Prefab for resource elements.</summary>
        [UxmlAttribute] public VisualTreeAsset elemPref;
        /// <summary>Reference to label displaying capacity state.</summary>
        Label capacityLabel;
        /// <summary>Refence to scroller.</summary>
        ScrollView storageScroll;
        /// <summary>Last row group for adding new elems.</summary>
        VisualElement lastRow;

        /// <summary>Data.</summary>
        [CreateProperty] List<UIResource> resources;
        /// <summary>List of all storage elems.</summary>
        List<StorageElem> storageElems;

        /// <summary>Datasource.</summary>
        IStorage storage;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor that adds and sets basic elements.
        /// </summary>
        public StorageInfo()
        {
            style.flexGrow = 1;
            name = "Storage";

            resources = new();
            storageElems = new();

            VisualElement element = new();
            element.AddToClassList("Capacity");
            capacityLabel = new("Capacity: ####/####");
            capacityLabel.AddToClassList("Capacity-Level");
            element.style.flexShrink = 0;
            element.Add(capacityLabel);
            Add(element);

            storageScroll = new ScrollView(ScrollViewMode.Vertical);
            storageScroll.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            storageScroll.style.minHeight = new(new Length(100, LengthUnit.Percent));
            Add(storageScroll);
        }
        #endregion

        /// <summary>
        /// Fills the elemlist
        /// </summary>
        /// <param name="_storage">Data source</param>
        public override void Open(object _storage)
        {
            storage = (IStorage)_storage;
            DataBinding binding = BindingUtil.CreateBinding(nameof(Building.LocalRes));
            binding.sourceToUiConverters.AddConverter((ref StorageResource store) => ToUIRes(store));
            SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, storage);

            binding = BindingUtil.CreateBinding(nameof(Building.LocalRes));
            binding.sourceToUiConverters.AddConverter((ref StorageResource store) => $"Capacity: {store.Sum()}/{store.capacity}");
            SceneRefs.infoWindow.RegisterTempBinding(new(capacityLabel, "text"), binding, storage);

            if (storageElems.Count > 0)
            {
                for (int i = 0; i < storage.CanStore.Count; i++)
                {
                    UpdateGroup(i);
                }
            }
        }

        #region Storage Managment
        /// <summary>
        /// Parses the data from <paramref name="storage"/>.
        /// </summary>
        /// <param name="storage">New data.</param>
        /// <returns></returns>
        List<UIResource> ToUIRes(StorageResource storage)
        {
            for (int i = 0; i < storage.type.Count; i++)
            {
                if (i >= resources.Count)
                {
                    if (i % 3 == 0)
                    {
                        lastRow = new();
                        lastRow.AddToClassList("storage-row");
                        storageScroll.Add(lastRow);
                    }
                    AddNewElem(new(storage.ammount[i], storage.type[i]));
                }
                else if (resources[i].ammount != storage.ammount[i])
                {
                    resources[i].ammount = storage.ammount[i];
                    storageElems[i].label.text = resources[i].ammount.ToString();
                }
            }
            return resources;
        }

        /// <summary>
        /// Changes canstore for one resource.
        /// </summary>
        /// <param name="b">New state</param>
        /// <param name="i">Resource index</param>
        void ToggleCanStore(bool b, int i)
        {
            storage.CanStore[i] = b;
        }

        /// <summary>
        /// Updates the canstore buttons on <paramref name="storageElem"/> elem.
        /// </summary>
        /// <param name="x">Index of the resource elem.</param>
        void UpdateGroup(int x)
        {
            ToggleButtonGroupState state = storageElems[x].canStore.value;
            state.ResetAllOptions();
            storageElems[x].canStore.value = state;
            state[0] = storage.CanStore[x];
            state[1] = !storage.CanStore[x];
            storageElems[x].canStore.value = state;
        }
        #endregion

        #region New elements
        /// <summary>
        /// Creates and links a new element.
        /// </summary>
        /// <param name="uiResource"></param>
        void AddNewElem(UIResource uiResource)
        {
            resources.Add(uiResource);
            VisualElement element = elemPref.CloneTree();
            StorageElem storageElem = new(element);
            var x = resources.Count - 1;
            storageElem.canStore.Q<Button>("Store").RegisterCallback<ClickEvent>((_) => ToggleCanStore(true, x));
            storageElem.canStore.Q<Button>("Not-Store").RegisterCallback<ClickEvent>((_) => ToggleCanStore(false, x));
            try
            {
                storageElem.icon.style.unityBackgroundImageTintColor = ToolkitUtils.resSkins.GetResourceColor(uiResource.type);
            }
            catch
            {
                Debug.LogError("Editor");
            }
            storageElem.label.text = uiResource.ammount.ToString();

            storageElems.Add(storageElem);
            UpdateGroup(x);
            lastRow.Add(element);

        }
        #endregion

    }
}
