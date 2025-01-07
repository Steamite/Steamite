using AbstractControls;
using InfoWindowElements;
using System;
using UnityEngine.UIElements;
using static UnityEngine.Analytics.IAnalytic;

namespace RadioGroups
{

    [UxmlElement]
    public partial class LevelUnlockerRadioGroup : CustomRadioButtonGroup
    {
        new public LevelState this[int i]
        {
            get => ((LevelUnlocker)_itemsSource[i]).state;
        }
        #region Base
        public LevelUnlockerRadioGroup() : base()
        {
            fixedItemHeight = 75;
            for (int i = 0; i < 5; i++)
            {
                AddItem(new LevelUnlocker(i, LevelState.Unavailable));
            }
        }
        protected override CustomRadioButton DefaultMakeItem() => new LevelUnlocker();
        protected override void DefaultBindItem(VisualElement element, int index)
        {
            base.DefaultBindItem(element, index);
            LevelUnlocker elem = (LevelUnlocker)element;
            if (((LevelUnlocker)_itemsSource[index]).IsSelected)
                _selectedButton = elem;
            elem.state = ((LevelUnlocker)_itemsSource[index]).state;
            elem.ToggleButtonStyle();
        }
        #endregion


        public int SetFill(Storage storage, LevelPresent levelData)
        {
            GridPos gridPos = storage.GetPos();

            SelectedId = gridPos.y;
            if (_selectedButton != null)
            {
                _selectedButton.Deselect();
                _selectedButton = null;
            }
            bool unlocked = true;
            // check up
            for (int i = SelectedId - 1; i > -1; i--)
            {
                gridPos.y = i;
                CheckLevel(gridPos, ref unlocked, levelData, (LevelUnlocker)_itemsSource[i]);
            }

            // check down
            unlocked = true;
            for (int i = SelectedId + 1; i < 5; i++)
            {
                gridPos.y = i;
                CheckLevel(gridPos, ref unlocked, levelData, (LevelUnlocker)_itemsSource[i]);
            }

            ((LevelUnlocker)_itemsSource[SelectedId]).state = LevelState.Selected;
            ((LevelUnlocker)_itemsSource[SelectedId]).Select();

            RefreshItems();
            return SelectedId;
        }

        /// <summary>
        /// Checks level supplyied by for loop.
        /// </summary>
        /// <param name="i">level number</param>
        /// <param name="gridPos">elevator position</param>
        /// <param name="unlocked">Is it contineous?</param>
        void CheckLevel(GridPos gridPos, ref bool unlocked, LevelPresent levelData, LevelUnlocker levelUnlocker)
        {
            if (unlocked)
            {
                if (MyGrid.GetGridItem(gridPos) is Elevator)
                    levelUnlocker.state = LevelState.Unlocked;
                else
                {
                    unlocked = false;
                    if (MyRes.CanAfford(levelData.costs[gridPos.y]))
                        levelUnlocker.state = LevelState.CanUnlock;
                    else
                        levelUnlocker.state = LevelState.Available;
                }
            }
            else
                levelUnlocker.state = LevelState.Unavailable;
        }

        public bool SetStates(int i, LevelState state)
        {
            LevelUnlocker unlocker = (LevelUnlocker)_itemsSource[i];
            if(unlocker.state != state)
            {
                unlocker.state = state;
                return true;
            }
            return false;
        }
    }
}