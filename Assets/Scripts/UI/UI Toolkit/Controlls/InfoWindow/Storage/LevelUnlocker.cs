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

        public LevelUnlocker(int i, LevelState _state) : base("", i, false)
        {
            ToggleButtonStyle(_state);
        }
        #endregion

        #region Selection
        /// <inheritdoc/>
        public override void Deselect(bool triggerTransition = true)
        {
            base.Deselect();
            LevelState state = (ToolkitUtils.GetParentOfType<LevelUnlockerRadioList>(this))[value];
            if (state == LevelState.Selected)
                ToggleButtonStyle(LevelState.Unlocked);
            else
                ToggleButtonStyle(state);
        }

        /// <inheritdoc/>
        protected override bool SelectChange(bool UpdateGroup)
        {
            base.SelectChange(UpdateGroup);
            ToggleButtonStyle(LevelState.Selected);
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
            bool active = ClassListContains("Level-Active");
            ClearClassList();
            if (active)
                AddToClassList("Level-Active");
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
                case LevelState.Selected:
                    enabledSelf = true;
                    AddToClassList("Level-Selected");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion
    }
}