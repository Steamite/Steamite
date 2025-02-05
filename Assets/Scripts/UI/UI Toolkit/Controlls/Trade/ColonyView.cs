using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace TradeWindowElements
{
    [UxmlElement]
    public partial class ColonyView : VisualElement
    {
        public ColonyView() { }

        public string Open()
        {
            return UIRefs.trading.colonyLocation.name;
        }

        public void Hide()
        {
            throw new NotImplementedException();
        }
    }
}
