using AbstractControls;
using RadioGroups;
using System;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    /// <summary>Represents each level in <see cref="LevelsTab"/>.</summary>
    [UxmlElement]
    public partial class LevelUnlocker : CustomRadioButton
    {
        #region Constructors
        public LevelUnlocker() : base()
        {

        }

        public LevelUnlocker(int i, LevelState _state, CustomRadioButtonList list) : base("", i, list)
        {
            ToggleButtonStyle(_state);
        }
        #endregion

        #region Selection
        /// <inheritdoc/>
        public override void Deselect(bool triggerTransition = true)
        {
            base.Deselect();
            LevelState state = (ToolkitUtils.GetParentOfType<LevelUnlockerRadioList>(this))[selIndex];
            DeselectButton();
        }

        /// <inheritdoc/>
        protected override bool SelectChange(bool UpdateGroup)
        {
            base.SelectChange(UpdateGroup);
            SelectButton();
            return false;
        }
        #endregion

        #region Styling
        /// <summary>
        /// Changes the style classes and enabledSelf.
        /// </summary>
        /// <param name="state">New state.</param>
        public void ToggleButtonStyle(LevelState state)
        {
            switch (state)
                {
                    case LevelState.Unavailable:
                        enabledSelf = false;
                        break;
                    case LevelState.Available:
                        enabledSelf = true;
                        AddToClassList("Level-Available");
                        break;
                    case LevelState.CanUnlock:
                        enabledSelf = true;
                        AddToClassList("Level-Can-Unlock");
                        break;
                    case LevelState.Unlocked:
                        enabledSelf = true;
                        AddToClassList("Level-Opened");
                        break;
                    //case LevelState.Selected:
                    default:
                        throw new NotImplementedException();
                }
        }

        public void DeselectButton()
        {
            RemoveFromClassList("Level-Selected");
        }
        public void SelectButton() 
        {
            AddToClassList("Level-Selected");
        }

        #endregion
    }
}