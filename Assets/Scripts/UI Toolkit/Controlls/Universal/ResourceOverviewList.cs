using InfoWindowElements;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI_Toolkit.Controlls.Universal
{
    public partial class ResourceOverviewList : VisualElement, IUIElement
    {
        readonly List<ResourceTypeCategory> categories;
        List<ResourceType> types;
        float scale;
        Label title;

        List<ResourceTextIcon> icons;
        [CreateProperty] public List<UIRes> res { get; set; }

        VisualElement body;


        public ResourceOverviewList() : base() { }
        public ResourceOverviewList(List<ResourceTypeCategory> _categories, float _scale) : base()
        {
            body = new();
            body.AddToClassList("res-overview-group");

            scale = _scale;
            categories = _categories;
            body.Add(title = new()
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 22,
                    alignSelf = Align.Center,
                    minWidth = new Length(110, LengthUnit.Percent), 
                    whiteSpace = WhiteSpace.Normal
                }
            });
            VisualElement container = new();
            container.AddToClassList("res-list-container");
            body.Add(container);
            Add(body);
        }

        public void ChangeCategory(int i, ResourceDisplay display, IResolvedStyle resStyle)
        {
            types = categories[i].Objects.Select(q => q.data).ToList();
            int activeElems;
            InitToUI(display.GlobalResources, out activeElems);

            title.text = categories[i].Name;
            if (resStyle.left < parent.resolvedStyle.width)
            {
                style.left = resStyle.left;
                style.right = StyleKeyword.None;
            }
            else
            {
                style.left = StyleKeyword.None;
                style.right = 0;
            }
            style.display = DisplayStyle.Flex;
        }

        public void Close()
        {
            types = null;
            style.display = DisplayStyle.None;
        }
        protected ResourceTextIcon MakeItem(ResourceType type, int ammount)
        {
            ResourceTextIcon el2 = new ResourceTextIcon(scale);
            el2.SetTextIcon(ammount.ToString(), type);
            el2.style.marginTop = 5;
            el2.style.justifyContent = Justify.SpaceBetween;
            el2.style.flexGrow = 0;
            el2.style.minWidth = 100;
            el2.style.maxWidth = 100;
            return el2;
        }

        void InitToUI(Resource storage, out int count) 
        {
            count = 0;
            icons = new();
            body[1].Clear();
            for (int i = 0; i < storage.types.Count; i++)
            {
                if (types.Contains(storage.types[i]))
                {
                    count++;
                    icons.Add(MakeItem(storage.types[i], storage.ammounts[i]));
                    body[1].Add(icons[^1]);
                }
            }
        }

        protected List<UIRes> ToUIRes(Resource storage)
        {
            if (types == null)
                return null;
            int j = 0;
            for (int i = 0; i < storage.types.Count; i++)
            {
                if (types.Contains(storage.types[i]))
                {
                    icons[j].SetText(storage.ammounts[i].ToString());
                    j++;
                }
            }
            return null;
        }

        public void Open(object data)
        {
            DataBinding binding = BindingUtil.CreateBinding(nameof(ResourceDisplay.GlobalResources));
            binding.sourceToUiConverters.AddConverter((ref MoneyResource globalRes) => ToUIRes(globalRes));
            SetBinding(nameof(res), binding);
            dataSource = data;
            //((IUpdatable)data).UIUpdate(binding.dataSourcePath.ToString());
        }
    }
}
