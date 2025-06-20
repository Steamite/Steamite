using AbstractControls;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RadioGroups
{
    /// <summary>
    /// Selecting world types at before starting the game
    /// </summary>
    [UxmlElement]
    public partial class WorldRadioList : CustomRadioButtonList
    {

        public WorldRadioList() : base()
        {
        }

        public void Open()
        {
            if (itemsSource == null)
            {
                contentContainer.style.minHeight = new Length(100, LengthUnit.Percent);
                contentContainer.style.justifyContent = Justify.SpaceAround;
                contentContainer.style.overflow = Overflow.Visible;
                contentContainer.parent.style.overflow = Overflow.Visible;
                ((ScrollView)hierarchy.ElementAt(0)).horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                ((ScrollView)hierarchy.ElementAt(0)).verticalScrollerVisibility = ScrollerVisibility.Hidden;
                itemsSource = new List<RadioButtonData> { new("Predefined"), new("Random") };
                ((CustomRadioButton)contentContainer[0]).Select();
            }
        }

        protected override CustomRadioButton DefaultMakeItem()
        {
            CustomRadioButton button = new CustomRadioButton("main-button", -1, false);
            button.style.fontSize = 65;
            button.style.marginTop = 0;
            return button;
        }
    }
}