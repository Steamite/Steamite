using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;


namespace InfoWindowElements
{
    public class UIResource
    {
        public int ammount;
        public ResourceType type;

        public UIResource(int _ammount, ResourceType _type)
        {
            ammount = _ammount;
            type = _type;
        }
    }

    [UxmlElement("Resource-List")]
    public partial class ResourceList : ListView, IUIElement
    {
        [CreateProperty]
        protected List<UIResource> resources
        {
            get { return (List<UIResource>)itemsSource; }
            set
            {
                itemsSource = value;
                RefreshItems();
            }
        }
        bool showEmpty = false;

        public ResourceList()
        {
            itemTemplate = Resources.Load<VisualTreeAsset>("UI Toolkit/Resource-Text-Icon");
            itemsSource = new List<UIResource>();

            makeItem = () =>
            {
                Debug.Log($"Making item{name}, {itemsSource.Count}");
                return itemTemplate.CloneTree();
            };

            bindItem = (el, i) =>
            {
                Color c = SceneRefs.infoWindow.resourceSkins.GetResourceColor(((UIResource)itemsSource[i]).type);
                if (!showEmpty)
                    el.Q<Label>("Value").style.color = c;

                el.Q<Label>("Value").text = ConvertString((UIResource)itemsSource[i]);
                el.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = c;
            };

            makeNoneElement = () =>
            {
                Label l = new Label("Nothing");
                l.AddToClassList("unity-list-view__empty-label");
                return l;
            };

            style.flexGrow = 1;
            selectionType = SelectionType.None;
            reorderable = false;
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            focusable = false;
        }

        public virtual void Fill(object data)
        {
            DataBinding binding;
            switch (data)
            {
                case StorageObject:
                    binding = Util.CreateBinding(nameof(StorageObject.LocalRes));
                    binding.sourceToUiConverters.AddConverter((ref StorageResource stored) => ToUIRes(stored.stored));
                    SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, data);
                    break;
                case Rock:
                    binding = Util.CreateBinding(nameof(Rock.rockYield));
                    binding.sourceToUiConverters.AddConverter((ref Resource yeild) => ToUIRes(yeild));
                    SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, data);
                    break;
                case Human:
                    binding = Util.CreateBinding(nameof(Human.Inventory));
                    binding.sourceToUiConverters.AddConverter((ref Resource inventory) => ToUIRes(inventory));
                    SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, data);
                    break;
                case ResourceDisplay:
                    binding = Util.CreateBinding(nameof(ResourceDisplay.GlobalResources));
                    binding.sourceToUiConverters.AddConverter((ref Resource globalRes) => ToUIRes(globalRes));
                    SetBinding("resources", binding);
                    dataSource = data;
                    ((IUpdatable)data).UIUpdate(binding.dataSourcePath.ToString());
                    (hierarchy.ElementAt(0) as ScrollView).verticalScrollerVisibility = ScrollerVisibility.Hidden;
                    showEmpty = true;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual List<UIResource> ToUIRes(Resource storage)
        {
            List<UIResource> res = new();
            for (int i = 0; i < storage.type.Count; i++)
            {
                if (showEmpty || storage.ammount[i] > 0)
                    res.Add(new(storage.ammount[i], storage.type[i]));
            }
            return res;
        }

        protected virtual string ConvertString(UIResource resource)
        {
            return $"{resource.ammount}";
        }
    }
}
