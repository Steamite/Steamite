using AbstractControls;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RadioGroups
{
    /// <summary>
    /// Selecting world types at before starting the game
    /// </summary>
    [UxmlElement]
    public partial class WorldRadioGroup : CustomRadioButtonGroup
    {

        public WorldRadioGroup() : base()
        {
            style.justifyContent = Justify.SpaceAround;
            CustomRadioButton button = new("main-button", 0, this) { text = "Predefined" };
            button = new("main-button", 1, this) { text = "Generated" };
        }
        public override void AddButton(CustomRadioButton button)
        {
            Add(button);
            button.style.fontSize = 65;
            button.style.height = new Length(14, LengthUnit.Percent);
            base.AddButton(button);
        }
        public void Open()
        {
            buttons[0].Select();
        }
    }
}