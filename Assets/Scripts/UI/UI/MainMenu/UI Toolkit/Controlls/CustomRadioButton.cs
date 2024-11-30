using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractControls
{
    public class CustomRadioButton : Button
    {

        public bool IsSelected { get; private set; }
        public string data;
        public int value;

        public string styleClass;
        string classToCall;

        [System.Obsolete]
        public new class UxmlFactory : UxmlFactory<CustomRadioButton, UxmlTraits> { }

        public CustomRadioButton()
        {
            styleClass = "save-radio-button";
            AddToClassList("save-radio-button");
            text = "string";
            data = "string";
            value = -1;
        }


        public CustomRadioButton(string labelText, string _styleClass, int i)
        {
            styleClass = _styleClass;
            AddToClassList(_styleClass);
            style.borderTopWidth = new(v: 0);
            style.borderLeftWidth = new(v: 0);
            style.borderRightWidth = new(v: 0);
            style.borderBottomWidth = new(v: 0);
            text = labelText;
            data = labelText;
            value = i;
        }

        public void Select(ClickEvent _)
        {
            if (IsSelected)
                return;
            IsSelected = true;
            RemoveFromClassList(styleClass);
            AddToClassList(styleClass + "-selected");
            parent.parent.parent.parent.parent.Q<CustomRadioButtonGroup>()?.Select(this);
        }

        public void Deselect()
        {
            IsSelected = false;
            RemoveFromClassList(styleClass + "-selected");
            AddToClassList(styleClass);
            //style.backgroundColor = new(new Color(0.7372549f, 0.7372549f, 0.7372549f, 1));
        }
    }

}
