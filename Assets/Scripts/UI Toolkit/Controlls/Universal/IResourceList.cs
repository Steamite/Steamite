using InfoWindowElements;
using System.Collections.Generic;

namespace Assets.Scripts.UI_Toolkit.Controlls.Universal
{
    public interface IResourceList : IUIElement
    {
        public List<UIResource> resources
        {
            get;
            set;
        }
        public float scale { get; set; }

    }
}