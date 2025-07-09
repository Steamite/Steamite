using InfoWindowElements;
using UnityEngine.UIElements;
namespace InfoWindowElements
{
    [UxmlElement]
    public partial class DoubleResList : DoubleResourceList<Resource, ResourceType>
    {
        public DoubleResList() : base() { }
        public DoubleResList(bool _cost, string _name, bool _useBindings = false)
            : base(_cost, _name, _useBindings)
        { }
    }
}