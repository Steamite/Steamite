using UnityEngine.UIElements;

namespace AbstractControls
{
    [UxmlElement]
    public partial class CustomRadioButton : Button
    {
        public bool IsSelected { get; protected set; }
        public string data;
        public int value;

        public string styleClass;

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

        /// <summary>
        /// Sets <see cref="IsSelected"/> to true, styles the button and sends an event to the button group.
        /// </summary>
        /// <param name="triggerTransition">If triggered by user, and should perform transition over duration, or instantly.</param>
        public virtual void Select(bool triggerTransition = true)
        {
            if (IsSelected)
                return;
            IsSelected = true;
            if (triggerTransition)
            {
                if (styleClass != "")
                {
                    RemoveFromClassList(styleClass);
                    AddToClassList(styleClass + "-selected");
                }
                    parent.parent.parent.Q<CustomRadioButtonGroup>().Select(this);
            }
            else if (styleClass != "")
            {
                    ToolkitUtils.ChangeClassWithoutTransition(styleClass, styleClass + "-selected", this);
            }
        }

        /// <summary>
        /// Sets <see cref="IsSelected"/> to false and resets styling.
        /// </summary>
        /// <param name="triggerTransition">If triggered by user, and should perform transition over duration, or instantly.</param>
        public virtual void Deselect(bool triggerTransition = true)
        {
            IsSelected = false;
            if (triggerTransition)
            {
                if (styleClass != "")
                {
                    RemoveFromClassList(styleClass + "-selected");
                    AddToClassList(styleClass);
                }
            }
            else
            {
                ToolkitUtils.ChangeClassWithoutTransition(styleClass + "-selected", styleClass, this);
            }
        }
    }

}
