using InfoWindowElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI_Toolkit.Controlls.Universal
{
    public partial class ResourceOverviewList : ResourceList
    {
        [UxmlAttribute] int selectedCategory;
        readonly public float width;
        readonly public float height;
        readonly List<ResourceTypeCategory> categories;

        public ResourceOverviewList() : base() { }
        public ResourceOverviewList(List<ResourceTypeCategory> _categories, float _scale) : base() 
        {
            scale = _scale;
            width = 160 * scale + 20;
            categories = _categories;
            style.minHeight = 1000;
            style.maxWidth = width;
            fixedItemHeight = height;
        }


        public void ChangeCategory(int i, ResourceDisplay display)
        {
            selectedCategory = i;
            itemsSource = ToUIRes(display.GlobalResources);
        }
        protected override VisualElement MakeItem()
        {
            VisualElement element = base.MakeItem();
            element.style.justifyContent = Justify.SpaceBetween;
            /*element.style.height = height;
            element.style.minWidth = width;
            element.style.maxWidth = width;
            element[0].style.fontSize = height / 60 * 30;
            float size = height / 60 * 50;
            element[1].style.maxHeight = size;
            element[1].style.maxWidth = size;
            element[1].style.minHeight = size;
            element[1].style.minWidth = size;*/
            return element;
        }

        protected override List<UIResource> ToUIRes(Resource storage)
        {
            return base.ToUIRes(storage.FilterByType(categories[selectedCategory].Objects));
        }
    }
}
