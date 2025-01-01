using RadioGroups;
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
            text = labelText;
            data = labelText;
            value = i;
        }

        public virtual void Select(bool triggerTransition = true)
        {
            if (IsSelected)
                return;
            IsSelected = true;
            if (triggerTransition)
            {
                RemoveFromClassList(styleClass);
                AddToClassList(styleClass + "-selected");
                parent.parent.parent.Q<CustomRadioButtonGroup>()?.Select(this);
            }
            else
            {
                ToolkitUtils.ChangeClassWithoutTransition(styleClass, styleClass + "-selected", this);
            }
        }

        public virtual void Deselect(bool triggerTransition = true)
        {
            IsSelected = false;
            if (triggerTransition)
            {
                RemoveFromClassList(styleClass + "-selected");
                AddToClassList(styleClass);
            }
            else
            {
                ToolkitUtils.ChangeClassWithoutTransition(styleClass + "-selected", styleClass, this);
            }
        }
    }

}
