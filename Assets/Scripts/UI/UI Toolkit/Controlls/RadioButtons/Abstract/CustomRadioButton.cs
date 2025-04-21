using UnityEngine.UIElements;

namespace AbstractControls
{
    [UxmlElement]
    public partial class CustomRadioButton : Button
    {
        public bool IsSelected { get; protected set; }
        public int value;

        public string styleClass;

        bool inGroup;
        public CustomRadioButton()
        {
            styleClass = "save-radio-button";
            AddToClassList("save-radio-button");
            value = -1;
        }


        public CustomRadioButton(string _styleClass, int i, bool _inGroup)
        {
            ClearClassList();
            styleClass = _styleClass;
            AddToClassList(_styleClass);
            value = i;
            inGroup = _inGroup;
            RegisterCallback<ClickEvent>((_) => Select());
        }

        /// <summary>
        /// Sets <see cref="IsSelected"/> to true, styles the button and sends an event to the button group.
        /// </summary>
        public void Select()
        {
            if (IsSelected)
                return;
            if (SelectChange(true) && styleClass != "")
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
            ToolkitUtils.ChangeClassWithoutTransition(styleClass, styleClass + "-selected", this);
        }

        protected virtual bool SelectChange(bool UpdateGroup)
        {
            IsSelected = true;
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
                    ((CustomRadioButtonList)el).Select(this);

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
