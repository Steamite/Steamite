using System.Collections.Generic;
using AbstractControls;
using UnityEngine.UIElements;

namespace RadioGroups
{
    [UxmlElement]
    public partial class WorldRadioList : CustomRadioButtonList
    {
        public WorldRadioList() : base()
        {
            _itemsSource = new List<RadioButtonData>{ new("Predefined"), new("Random") };
        }

        public void Reset()
        {
            ((CustomRadioButton)ElementAt(0)).Select();
        }

        protected override CustomRadioButton DefaultMakeItem()
        {
            CustomRadioButton button = new CustomRadioButton("main-button", -1, true);
            button.style.fontSize = 65;
            button.style.marginTop = 0;
            return button;
        }
    }
}