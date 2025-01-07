using System;
using UnityEngine.UIElements;
using AbstractControls;
using UnityEngine;

namespace RadioGroups
{
    [UxmlElement]
    public partial class WorldRadioGroup : CustomRadioButtonGroup
    {
        string[] choices = new string[] { "Predefined", "Random" };

        #region List
        public WorldRadioGroup() : base()
        {
            for (int i = 0; i < choices.Length; i++)
            {
                AddItem(new($"{choices[i]}", "main-button", i));
            }
        }
        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            element.style.marginTop = new(new Length(25, LengthUnit.Pixel));
            element.style.fontSize = new(new Length(40, LengthUnit.Percent));
            element.style.height = new(new Length(98.3f, LengthUnit.Pixel));
        }
        protected override CustomRadioButton DefaultMakeItem()
        {
            return new("string", "main-button", -1);
        }
        public void Reset()
        {
            _selectedButton?.Deselect(false);
            _selectedButton = null;
        }
        #endregion
    }
}