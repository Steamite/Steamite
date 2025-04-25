using UnityEngine.UIElements;

namespace AbstractControls
{
    [UxmlElement]
    public partial class CustomRadioButton : Button
    {
        public bool IsSelected { get; protected set; }
        bool toggle;
        public int value;

        public string styleClass;

        bool inGroup;
        public CustomRadioButton()
        {
            styleClass = "save-radio-button";
            AddToClassList("save-radio-button");
            value = -1;
            toggle = false;
        }


        public CustomRadioButton(string _styleClass, int i, bool _inGroup, bool _toggle = false)
        {
            ClearClassList();
            styleClass = _styleClass;
            AddToClassList(_styleClass);
            value = i;
            inGroup = _inGroup;
            toggle = _toggle;
            RegisterCallback<ClickEvent>((_) => Select());
        }

        /// <summary>
        /// Sets <see cref="IsSelected"/> to true, styles the button and sends an event to the button group.
        /// </summary>
        public void Select()
        {
            if (IsSelected && !toggle)
                return;
            IsSelected = SelectChange(true);
            if (IsSelected && styleClass != "")
            {
                RemoveFromClassList(styleClass);
                AddToClassList(styleClass + "-selected");
            }
        }

        /// <summary>
        /// Sets <see cref="IsSelected"/> to true, styles the button and sends an event to the button group.
        /// </summary>
        /// <param name="UpdateGroup">Should the parent group be updated.</param>
        public void SelectWithoutTransition(bool UpdateGroup)
        {
            SelectChange(UpdateGroup);
            IsSelected = true;
            ToolkitUtils.ChangeClassWithoutTransition(styleClass, styleClass + "-selected", this);
        }

        /// <summary>
        /// Returns if the change is valid or not. (not enough resources, ...)
        /// </summary>
        /// <param name="UpdateGroup"></param>
        /// <returns></returns>
        protected virtual bool SelectChange(bool UpdateGroup)
        {
            if (UpdateGroup)
            {

                VisualElement el = this;
                if (inGroup)
                {
                    do
                    {
                        el = el.parent;
                    } while (el.parent != null && el is not CustomRadioButtonGroup);
                    ((CustomRadioButtonGroup)el).Select(value);
                }
                else
                {
                    do
                    {
                        el = el.parent;
                    } while (el.parent != null && el is not CustomRadioButtonList);
                    ((CustomRadioButtonList)el).Select(value);

                }
            }
            return true;
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
