using AbstractControls;
using RadioGroups;
using System;
using UnityEngine.UIElements;

namespace InfoWindowElements
{
    [UxmlElement]
    public partial class LevelUnlocker : CustomRadioButton
    {
        public LevelUnlocker() : base()
        {
            
        }

        public LevelUnlocker(int i, LevelState _state) : base(i.ToString(), "", i, true)
        {
            ToggleButtonStyle(_state);
        }

        public override void Deselect(bool triggerTransition = true)
        {
            base.Deselect();
            LevelState state = ((LevelUnlockerRadioGroup)parent)[value];
            if (state == LevelState.Selected)
                ToggleButtonStyle(LevelState.Unlocked);
            else
                ToggleButtonStyle(state);
        }
        public override void Select(bool triggerTransition = true)
        {
            base.Select();
            ToggleButtonStyle(LevelState.Selected);
        }


        #region Styling

        public void ToggleButtonStyle(LevelState state)
        {
            ClearClassList();
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