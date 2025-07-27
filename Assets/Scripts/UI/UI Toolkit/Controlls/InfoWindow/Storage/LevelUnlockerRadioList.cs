using AbstractControls;
using InfoWindowElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RadioGroups
{
    class RadioLevelButtonData : RadioButtonData
    {
        public LevelState state;
        public bool active;
        public RadioLevelButtonData(string _text) : base(_text)
        {
            state = LevelState.Unavailable;
        }
    }

    /// <summary>Radio group for managing switchin between level unlockers.</summary>
    [UxmlElement]
    public partial class LevelUnlockerRadioList : CustomRadioButtonList
    {
        #region Variables
        new public LevelState this[int i]
        {
            get
            {
                if (i < -1)
                {
                    Debug.LogWarning("index out of range");
                    return LevelState.Unavailable;
                }
                return ((RadioLevelButtonData)itemsSource[i]).state;
            }
        }
        #endregion

        #region Constructors
        public LevelUnlockerRadioList() : base()
        {
            itemsSource = new List<RadioLevelButtonData> { new("1"), new("2"), new("3"), new("4"), new("5") };
            contentContainer.AddToClassList("Level-Group");
            contentContainer.parent.style.overflow = Overflow.Visible;
        }

        protected override CustomRadioButton DefaultMakeItem()
        {
            return new LevelUnlocker(-1, LevelState.Unavailable, this);
        }

        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            (element as LevelUnlocker).ToggleButtonStyle(this[index]);
            if (((RadioLevelButtonData)itemsSource[index]).active)
                (element as LevelUnlocker).AddToClassList("Level-Active");
            else
                (element as LevelUnlocker).RemoveFromClassList("Level-Active");
        }

        #endregion



        /// <summary>
        /// Resets the view and selects the current level.
        /// </summary>
        /// <param name="storage">Storage building that's being inspected.</param>
        /// <param name="levelData">Data containing costs for each of the button.</param>
        /// <returns>New level.</returns>
        public int Open(Elevator storage, LevelPresent levelData)
        {
            GridPos gridPos = storage.GetPos();

            if (SelectedChoice > -1)
            {
                ((LevelUnlocker)contentContainer[SelectedChoice]).Deselect();
                _selID = -1;
            }
            int level = gridPos.y;
            bool unlocked = true;
            // check up
            for (int i = level - 1; i > -1; i--)
            {
                gridPos.y = i;
                CheckLevel(gridPos, ref unlocked, levelData);
            } 

            // check down
            unlocked = true;
            for (int i = level + 1; i < 5; i++)
            {
                gridPos.y = i;
                CheckLevel(gridPos, ref unlocked, levelData);
            }

            ((RadioLevelButtonData)itemsSource[level]).active = true;
            SetStates(level, LevelState.Unlocked);
            ((LevelUnlocker)contentContainer[level]).SelectWithoutTransition(true);
            RefreshItems();
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
            ((RadioLevelButtonData)itemsSource[gridPos.y]).active = false;
            if (unlocked)
            {
                if (MyGrid.GetGridItem(gridPos) is Elevator)
                    SetStates(gridPos.y, LevelState.Unlocked);
                else if(MyGrid.GetGridItem(gridPos) is Road)
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
            if (this[i] != state)
            {
                ((RadioLevelButtonData)itemsSource[i]).state = state;
                RefreshItem(i);
                return true;
            }
            return false;
        }
    }
}