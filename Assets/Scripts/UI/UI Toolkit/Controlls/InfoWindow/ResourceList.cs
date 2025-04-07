using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;


namespace InfoWindowElements
{
    /// <summary>Parsed data used directly for displaying.<br/>
    /// Parsed using binding convertors.</summary>
    public class UIResource
    {
        /// <summary>Resource ammount.</summary>
        public int ammount;
        /// <summary>Resource type.</summary>
        public Enum type;

        /// <summary>
        /// For resources.
        /// </summary>
        /// <param name="_ammount"></param>
        /// <param name="_type"></param>
        public UIResource(int _ammount, ResourceType _type)
        {
            ammount = _ammount;
            type = _type;
        }

        /// <summary>
        /// For money.
        /// </summary>
        /// <param name="_ammount"></param>
        public UIResource(int _ammount)
        {
            ammount = _ammount;
            type = null;
        }
    }

    /// <summary>
    /// Creates a simple list with one item for each resource type. <br/>
    /// Can hide empty ones.
    /// </summary>
    [UxmlElement("Resource-List")]
    public partial class ResourceList : ListView, IUIElement
    {
        #region Properties
        /// <summary>Binding link(_itemSource)</summary>
        [CreateProperty] protected List<UIResource> resources
        {
            get { return (List<UIResource>)itemsSource; }
            set
            {
                itemsSource = value;
                RefreshItems();
            }
        }

        #endregion

        #region Variables
        /// <summary>If disabled hides resources with 0.</summary>
        protected bool showEmpty = false;

        public const int ICON_SIZE = 60;
        [UxmlAttribute] public int iconSize = 60;
        #endregion

        #region Constructors
        public ResourceList()
        {
            itemTemplate = Resources.Load<VisualTreeAsset>("UI Toolkit/Resource Text Icon");
            itemsSource = new List<UIResource>();
            focusable = false;

            makeItem = MakeItem;
            bindItem = BindItem;
            makeNoneElement = MakeNoneElement;

            style.flexGrow = 1;
            selectionType = SelectionType.None;
            reorderable = false;
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        }
        #endregion

        #region Item Actions
        /// <summary>
        /// Customizible function for instantiating items.
        /// </summary>
        /// <returns>The instantiated item.</returns>
        protected virtual VisualElement MakeItem()
        {
            Debug.Log($"Making item{name}, {itemsSource.Count}, {parent.name}/{name}");
            VisualElement element = itemTemplate.CloneTree();
            element.ElementAt(0).ElementAt(0).style.fontSize = 40 * iconSize / ICON_SIZE;
            element.ElementAt(0).ElementAt(1).style.width = iconSize;
            element.ElementAt(0).ElementAt(1).style.height = iconSize;
            return element;
        }

        /// <summary>
        /// Customizible function for binding data to items.
        /// </summary>
        /// <param name="el">Element to bind to.</param>
        /// <param name="i">Index of the element.</param>
        protected virtual void BindItem(VisualElement el, int i)
        {
            el.RemoveFromClassList("unity-collection-view__item");
            Color c = ToolkitUtils.resSkins.GetResourceColor(((UIResource)itemsSource[i]).type);
            el.Q<Label>("Value").style.color = ToolkitUtils.textColor;

            el.Q<Label>("Value").text = ConvertString((UIResource)itemsSource[i]);
            el.Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = c;
        }

        /// <summary>
        /// Customizible function for creating the empty element.
        /// </summary>
        /// <returns>Created none element.</returns>
        protected virtual VisualElement MakeNoneElement()
        {
            Label l = new Label("Nothing");
            l.style.marginBottom = 0;
            l.style.marginTop = 0;
            l.style.marginLeft = 0;
            l.style.marginRight = 0;


            l.style.paddingBottom = 0;
            l.style.paddingTop = 0;
            l.style.paddingLeft = 0;
            l.style.paddingRight = 0;


            l.AddToClassList("unity-list-view__empty-label");
            return l;
        }
        #endregion

        public virtual void Open(object data)
        {
            DataBinding binding;
            switch (data)
            {
                case StorageObject:
                    binding = BindingUtil.CreateBinding(nameof(StorageObject.LocalRes));
                    binding.sourceToUiConverters.AddConverter((ref StorageResource stored) => ToUIRes(stored.stored));
                    SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, data);
                    break;
                case Rock:
                    binding = BindingUtil.CreateBinding(nameof(Rock.rockYield));
                    binding.sourceToUiConverters.AddConverter((ref Resource yeild) => ToUIRes(yeild));
                    SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, data);
                    break;
                case Human:
                    binding = BindingUtil.CreateBinding(nameof(Human.Inventory));
                    binding.sourceToUiConverters.AddConverter((ref Resource inventory) => ToUIRes(inventory));
                    SceneRefs.infoWindow.RegisterTempBinding(new(this, "resources"), binding, data);
                    break;
                case ResourceDisplay:
                    ToolkitUtils.Init();

                    binding = BindingUtil.CreateBinding(nameof(ResourceDisplay.GlobalResources));
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

        #region Convertors
        /// <summary>
        /// Splits and parses <paramref name="storage"/> by each each <see cref="ResourceType"/>.
        /// </summary>
        /// <param name="storage">Resources from the datasource.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Basic string parser.
        /// </summary>
        /// <param name="resource">What to parse</param>
        /// <returns>Just the resource ammount.</returns>
        protected virtual string ConvertString(UIResource resource)
        {
            return $"{resource.ammount}";
        }
        #endregion
    }
}
