using AbstractControls;
using System;
using UnityEngine.UIElements;

namespace InfoWindowElements
{

    [UxmlElement]
    public partial class LevelUnlocker : CustomRadioButton
    {
        public LevelState state;
        public LevelUnlocker() : base()
        {
            
        }

        public LevelUnlocker(int i, LevelState _state) : base(i.ToString(), "", i)
        {
            state = _state;
            ToggleButtonStyle(state);
        }

        public override void Deselect(bool triggerTransition = true)
        {
            base.Deselect();
            if (state == LevelState.Selected)
                ToggleButtonStyle(LevelState.Unlocked);
            else
                ToggleButtonStyle(state);
        }
        public override void Select(bool triggerTransition = true)
        {
            if(parent == null)
            {
                IsSelected = true;
                return;
            }
            base.Select();
            ToggleButtonStyle(LevelState.Selected);
        }


        #region Styling
        public void ToggleButtonStyle()
        {
            ToggleButtonStyle(state);
        }

        void ToggleButtonStyle(LevelState _state)
        {
            ClearClassList();
            switch (_state)
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