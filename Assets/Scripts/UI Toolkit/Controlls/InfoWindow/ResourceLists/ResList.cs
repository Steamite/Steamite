using InfoWindowElements;
using Outposts;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    [UxmlElement]
    public partial class ResList : ResourceList<Resource, ResourceType> 
    { 
        public override void Open(object data)
        {
            switch (data)
            {
                case Outpost outpost:
                    if (cost)
                        SetResWithoutBinding(outpost.production);
                    else
                        SetResWithoutBinding(outpost.storedResources);
                    return;
            }
            base.Open(data);
        }
    }
}