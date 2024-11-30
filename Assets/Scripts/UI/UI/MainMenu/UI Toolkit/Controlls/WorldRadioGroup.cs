using System;
using UnityEngine.UIElements;
using AbstractControls;

namespace RadioGroups
{
    public class WorldRadioGroup : CustomRadioButtonGroup
    {
        string[] choices = new string[] { "Predefined", "Random" };
        [Obsolete]
        public new class UxmlFactory : UxmlFactory<WorldRadioGroup, UxmlTraits> { }
        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            element.style.marginTop = new(new Length(25, LengthUnit.Pixel));
            element.style.fontSize = new(new Length(40, LengthUnit.Percent));
            element.style.height = new(new Length(98.3f, LengthUnit.Pixel));
            element.RegisterCallback<ClickEvent>((element as CustomRadioButton).Select);

            (element as CustomRadioButton).text = _itemsSource[index].data;
        }
        protected override CustomRadioButton DefaultMakeItem()
        {
            return new("string", "world-radio-button", -1);
        }
        public override void Init(Action<int> onChange)
        {
            base.Init(onChange);
            if (_itemsSource.Count == 0)
            {
                for (int i = 0; i < choices.Length; i++)
                {
                    AddItem(new($"{choices[i]}", "world-radio-button", i));
                }
            }
            else
            {
                _selectedButton?.Deselect();
                _selectedButton = null;
            }
        }
        public override void Select(CustomRadioButton customRadioButton)
        {
            base.Select(customRadioButton);
        }
    }
}