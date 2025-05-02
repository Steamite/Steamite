using System;
using UnityEngine.UIElements;

namespace AbstractControls
{

    /// <summary>
    /// Group is for loose buttons with more layout needs. (eg. <see cref="Research.ResearchRadioButtonGroup"/>>)
    /// </summary>
    [UxmlElement]
    public partial class CustomRadioButtonGroup : VisualElement
    {
        #region Variables
        protected Action<int> changeEvent;

        /// <summary>Index of the currently selected button.</summary>
        public int SelectedChoice
        {
            get;
            protected set;
        }
        #endregion

        #region Constructors
        public CustomRadioButtonGroup()
        {
            SelectedChoice = -1;
            style.flexGrow = 1;
            style.justifyContent = Justify.SpaceAround;
        }
        #endregion

        #region Changing
        /// <summary>
        /// Sets the <see cref="changeEvent"/>.
        /// </summary>
        /// <param name="onChange"></param>
        public void SetChangeCallback(Action<int> onChange) => changeEvent = onChange;

        /// <summary>
        /// Deselects the previous button and triggers the <see cref="changeEvent"/>.
        /// </summary>
        /// <param name="value">Index of the new button</param>
        public virtual bool Select(int value)
        {
            if (SelectedChoice > -1)
                ((CustomRadioButton)ElementAt(SelectedChoice)).Deselect();
            if(SelectedChoice == value)
            {
                SelectedChoice = -1;
                changeEvent?.Invoke(-1);
                return false;
            }
            else
            {
                SelectedChoice = value;
                changeEvent?.Invoke(value);
                return true;
            }
        }
        #endregion
    }
}