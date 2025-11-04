using InfoWindowElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI_Toolkit.Controlls.Universal
{
    public partial class ResourceOverviewList : ResourceList
    {
        [UxmlAttribute] int selectedCategory;
        readonly public float width;
        readonly List<ResourceTypeCategory> categories;
        int activeElems = 0;
        Label title;

        public ResourceOverviewList() : base() { }
        public ResourceOverviewList(List<ResourceTypeCategory> _categories, float _scale) : base()
        {
            scale = _scale;
            width = 160 * scale + 20;
            categories = _categories;
            style.maxWidth = width;
            hierarchy.Insert(0, title = new()
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

        }


        public void ChangeCategory(int i, ResourceDisplay display, IResolvedStyle resStyle)
        {
            selectedCategory = i;
            itemsSource = ToUIRes(display.GlobalResources);
            float size = 40 * activeElems;
            style.minHeight = size;
            title.text = categories[i].Name;
            if (resStyle.top + size < parent.resolvedStyle.height)
            {
                style.top = resStyle.top;
                style.bottom = StyleKeyword.None;
            }
            else
            {
                style.bottom = 0;
                style.top = StyleKeyword.None;
            }
        }
        protected override VisualElement MakeItem()
        {
            VisualElement element = new();
            VisualElement el2 = base.MakeItem();
            el2.style.marginTop = 5;
            el2.style.justifyContent = Justify.SpaceBetween;
            element.Add(el2);
            return element;
        }

        protected override void BindItem(VisualElement el, int i)
        {
            el.RemoveFromClassList("unity-collection-view__item");

            (el[0] as ResourceTextIcon).SetTextIcon(ConvertString((UIResource)itemsSource[i]), ((UIResource)itemsSource[i]).type);
        }

        protected override List<UIResource> ToUIRes(Resource storage)
        {
            return base.ToUIRes(storage.FilterByType(categories[selectedCategory].Objects, out activeElems));
        }
    }
}
