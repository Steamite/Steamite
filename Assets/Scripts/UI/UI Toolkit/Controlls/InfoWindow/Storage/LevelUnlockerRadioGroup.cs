using AbstractControls;
using InfoWindowElements;
using UnityEngine.UIElements;

namespace RadioGroups
{
    /// <summary>Radio group for managing switchin between level unlockers.</summary>
    [UxmlElement]
    public partial class LevelUnlockerRadioGroup : CustomRadioButtonGroup
    {
        #region Variables
        LevelState[] states;

        new public LevelState this[int i]
        {
            get => states[i];
        }
        #endregion

        #region Constructors
        public LevelUnlockerRadioGroup() : base()
        {
            states = new LevelState[5];
            choices = new string[] { "1", "2", "3", "4", "5"};
        }

        protected override CustomRadioButton CreateButton(int i)
        {
            return new LevelUnlocker(i, states[i]);
        }
        #endregion

        /// <summary>
        /// Resets the view and selects the current level.
        /// </summary>
        /// <param name="storage">Storage building that's being inspected.</param>
        /// <param name="levelData">Data containing costs for each of the button.</param>
        /// <returns>New level.</returns>
        public int SelectUpdate(Storage storage, LevelPresent levelData)
        {
            GridPos gridPos = storage.GetPos();

            if (SelectedChoice > -1)
            {
                ((LevelUnlocker)ElementAt(SelectedChoice)).Deselect();
            }
            SelectedChoice = gridPos.y;
            bool unlocked = true;
            // check up
            for (int i = SelectedChoice - 1; i > -1; i--)
            {
                gridPos.y = i;
                CheckLevel(gridPos, ref unlocked, levelData);
            }

            // check down
            unlocked = true;
            for (int i = SelectedChoice + 1; i < 5; i++)
            {
                gridPos.y = i;
                CheckLevel(gridPos, ref unlocked, levelData);
            }

            states[SelectedChoice] = LevelState.Selected;
            ((LevelUnlocker)ElementAt(SelectedChoice)).Select();

            return SelectedChoice;
        }


        /// <summary>
        /// Checks level supplyied by for loop.
        /// </summary>
        /// <param name="i">level number</param>
        /// <param name="gridPos">elevator position</param>
        /// <param name="unlocked">Is it contineous?</param>
        void CheckLevel(GridPos gridPos, ref bool unlocked, LevelPresent levelData)
        {
            if (unlocked)
            {
                if (MyGrid.GetGridItem(gridPos) is Elevator)
                    SetStates(gridPos.y, LevelState.Unlocked);
                else
                {
                    unlocked = false;
                    if (MyRes.CanAfford(levelData.costs[gridPos.y]))
                        SetStates(gridPos.y, LevelState.CanUnlock);
                    else
                        SetStates(gridPos.y, LevelState.Available);
                }
            }
            else
                SetStates(gridPos.y, LevelState.Unavailable);
        }

        /// <summary>
        /// Direct manipulation with button states, used to switch between affordable and too expenisve.
        /// </summary>
        /// <param name="i">Which state to set for.</param>
        /// <param name="state">Value to set.</param>
        /// <returns></returns>
        public bool SetStates(int i, LevelState state)
        {
            if(states[i] != state)
            {
                states[i] = state;
                ((LevelUnlocker)ElementAt(i)).ToggleButtonStyle(state);
                return true;
            }
            return false;
        }
    }
}