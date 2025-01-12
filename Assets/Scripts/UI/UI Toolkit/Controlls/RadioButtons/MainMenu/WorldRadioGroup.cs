using UnityEngine.UIElements;
using AbstractControls;

namespace RadioGroups
{
    [UxmlElement]
    public partial class WorldRadioGroup : CustomRadioButtonGroup
    {
        public WorldRadioGroup() : base()
        {
            choices = new string[] { "Predefined", "Random" };
        }

        public void Reset()
        {
            ((CustomRadioButton)ElementAt(0)).Select();
        }
        protected override CustomRadioButton CreateButton(int i)
        {
            CustomRadioButton button = new CustomRadioButton(choices[i], "main-button", i, true);
            button.style.fontSize = 65;
            button.style.marginTop = 0;
            return button;
        }
    }
}