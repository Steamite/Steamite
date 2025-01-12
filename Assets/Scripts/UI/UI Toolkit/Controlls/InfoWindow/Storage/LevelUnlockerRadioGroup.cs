using AbstractControls;
using InfoWindowElements;
using UnityEngine.UIElements;

namespace RadioGroups
{

    [UxmlElement]
    public partial class LevelUnlockerRadioGroup : CustomRadioButtonGroup
    {
        LevelState[] states;

        new public LevelState this[int i]
        {
            get => states[i];
        }

        #region Base
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