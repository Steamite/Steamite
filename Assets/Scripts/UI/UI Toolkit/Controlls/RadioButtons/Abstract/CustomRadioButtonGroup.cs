using System;
using UnityEngine.UIElements;

namespace AbstractControls
{
    [UxmlElement]
    public partial class CustomRadioButtonGroup : VisualElement
    {
        #region Variables
        protected event Action<int> changeEvent;

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
        public void Select(int value)
        {
            if (SelectedChoice > -1 && SelectedChoice != value)
                ((CustomRadioButton)ElementAt(SelectedChoice)).Deselect();
            SelectedChoice = value;
            changeEvent?.Invoke(value);
        }
        #endregion
    }
}