using AbstractControls;
using UnityEngine.UIElements;

namespace RadioGroups
{
    [UxmlElement]
    public partial class WorldRadioGroup : TextRadioButtonGroup
    {
        public WorldRadioGroup() : base()
        {
            choices = new string[] { "Predefined", "Random" };
        }

        public void Reset()
        {
            ((CustomRadioButton)ElementAt(0)).Select();
        }
        protected override TextRadioButton CreateButton(int i)
        {
            TextRadioButton button = new TextRadioButton("main-button", i, true, choices[i]);
            button.style.fontSize = 65;
            button.style.marginTop = 0;
            return button;
        }
    }
}