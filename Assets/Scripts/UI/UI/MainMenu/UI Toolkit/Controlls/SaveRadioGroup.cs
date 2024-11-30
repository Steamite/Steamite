using System;
using UnityEngine.UIElements;

using AbstractControls;

namespace RadioGroups
{
    public class SaveRadioGroup : CustomRadioButtonGroup
    {
        [Obsolete]
        public new class UxmlFactory : UxmlFactory<SaveRadioGroup, UxmlTraits> { }
        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            (element as CustomRadioButton).text = _itemsSource[index].data;
            (element as CustomRadioButton).style.fontSize = new(new Length(40, LengthUnit.Percent));
            (element as CustomRadioButton).style.height = new(new Length(98.3f, LengthUnit.Pixel));
            (element as CustomRadioButton).RegisterCallback<ClickEvent>((element as CustomRadioButton).Select);
        }
        protected override CustomRadioButton DefaultMakeItem()
        {
            return new("string", "save-radio-button", -1);
        }
        public override void Init(Action<int> onChange)
        {
            base.Init(onChange);
            for (int i = 0; i < 9; i++)
            {
                AddItem(new($"Save {i}#", "save-radio-button", i));
            }
        }
        public override void Select(CustomRadioButton customRadioButton)
        {
            base.Select(customRadioButton);
            // handele logic
        }
    }

}