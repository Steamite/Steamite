using System;
using RadioGroups;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfoWindowElements
{

    /// <summary>State of each level button.</summary>
    public enum LevelState
    {
        /// <summary>Too far.</summary>
        Unavailable,
        /// <summary>Next locked level, cannot afford at the moment.</summary>
        Available,
        /// <summary>Next locked level, can afford at the moment.</summary>
        CanUnlock,
        /// <summary>unlocked level</summary>
        Unlocked,
        /// <summary>selected level</summary>
        Selected,
    }

    /// <summary>Tab for <see cref="Elevator"/> "Levels" tab.</summary>
    [UxmlElement]
    public partial class LevelsTab : Tab
    {
        /// <summary>Data for unlocking and displaying info about each of the levels.</summary>
        public LevelPresent LevelData { get; private set; }
        /// <summary>Active level in the <see cref="levelGroup"/>.</summary>
        public int SelectedLevel { get; private set; }

        /// <summary>Control for level unlockers.</summary>
        LevelUnlockerRadioList levelGroup;

        Label headerLabel;
        Label stateLabel;
        Label bodyLabel;
        DoubleResourceList costList;
        /// <summary>Button for moving between levels or unlocking new ones.</summary>
        Button moveButton;

        /// <summary>
        /// Creates a new <see cref="Tab"/> and initializes all parts of the view.
        /// </summary>
        public LevelsTab() : base("Levels")
        {
            LevelData = Resources.Load<LevelPresent>("Holders/Data/Level Present");
            style.flexGrow = 1;
            contentContainer.style.flexDirection = FlexDirection.Row;
            name = "Levels";

            levelGroup = new LevelUnlockerRadioList();
            levelGroup.Init(
                (i) =>
                {
                    ChangeActiveView(i);
                    SelectedLevel = i;
                });
            Add(levelGroup);


            VisualElement view = new();
            view.name = "Level-View";

            headerLabel = new(LevelData.headers[0]);
            headerLabel.name = "Header";
            view.Add(headerLabel);

            stateLabel = new("OK");
            stateLabel.name = "State";
            view.Add(stateLabel);

            bodyLabel = new(LevelData.bodies[0]);
            bodyLabel.name = "Body";
            view.Add(bodyLabel);

            costList = new(true, "Cost");
            view.Add(costList);

            moveButton = new();
            moveButton.AddToClassList("main-button");
            moveButton.RegisterCallback<ClickEvent>(HandleButton);
            view.Add(moveButton);
            Add(view);
        }

        /// <summary>
        /// Refreshes <see cref="levelGroup"/> and level info.
        /// </summary>
        /// <param name="data">Elevator that is selected.</param>
        public void Open(object data)
        {
            SelectedLevel = levelGroup.SelectUpdate((Elevator)data, LevelData);
            ChangeActiveView(SelectedLevel);
        }

        /// <summary>
        /// Switches level info and Updates <see cref="moveButton"/>.
        /// </summary>
        /// <param name="i"></param>
        void ChangeActiveView(int i)
        {
            headerLabel.text = LevelData.headers[i];
            bodyLabel.text = LevelData.bodies[i];

            if (levelGroup[SelectedLevel] != LevelState.Unlocked)
                SceneRefs.infoWindow.ClearBinding(costList);

            LevelState state = levelGroup[i];
            switch (state)
            {
                case LevelState.Available:
                    moveButton.text = "Unlock";
                    costList.style.display = DisplayStyle.Flex;
                    costList.Open(this);
                    stateLabel.text = "Not enough resources";
                    MoveButtonUpdate(true, "Unlock");
                    break;
                case LevelState.CanUnlock:
                    moveButton.text = "Unlock";
                    costList.style.display = DisplayStyle.Flex;
                    costList.Open(this);
                    stateLabel.text = "Can unlock";
                    MoveButtonUpdate(true, "Unlock");
                    break;
                case LevelState.Unlocked:
                    costList.style.display = DisplayStyle.None;
                    stateLabel.text = "Unlocked";
                    MoveButtonUpdate(true, "Move to");
                    break;
                case LevelState.Selected:
                    costList.style.display = DisplayStyle.None;
                    stateLabel.text = "Unlocked";
                    MoveButtonUpdate(false, "Move to");
                    break;
            }
        }

        /// <summary>
        /// Called when the selected level is not unlocked and global resources change. <br/>
        /// Makes parameters for <see cref="MoveButtonUpdate"/>.
        /// </summary>
        public void UpdateCostView()
        {
            if (MyRes.CanAfford(LevelData.costs[SelectedLevel]))
            {
                if (levelGroup.SetStates(SelectedLevel, LevelState.CanUnlock))
                {
                    stateLabel.text = "Can unlock";
                    MoveButtonUpdate(true, "Unlock");
                }
            }
            else
            {
                if (levelGroup.SetStates(SelectedLevel, LevelState.Available))
                {
                    stateLabel.text = "Not enough resources";
                    MoveButtonUpdate(false, "Unlock");
                }
            }
        }

        /// <summary>
        /// Updates the <see cref="moveButton"/> according to parameters.
        /// </summary>
        /// <param name="active"></param>
        /// <param name="displaystring">string to display</param>
        void MoveButtonUpdate(bool active, string displaystring)
        {
            moveButton.text = displaystring;
            if (active)
            {
                moveButton.RemoveFromClassList("disabled-button");
                moveButton.AddToClassList("main-button");
            }
            else
            {
                moveButton.RemoveFromClassList("main-button");
                moveButton.AddToClassList("disabled-button");
            }
        }

        /// <summary>
        /// Handles clicking on the <see cref="moveButton"/>
        /// </summary>
        /// <param name="_">Discarded, used for cleaner call.</param>
        /// <exception cref="NotImplementedException">Triggered by clicking on a button in the <see cref="LevelState.Unavailable"/>state.</exception>
        void HandleButton(ClickEvent _)
        {
            switch (levelGroup[SelectedLevel])
            {
                case LevelState.CanUnlock:
                    //ConfirmWindow.window.Open(MyGrid., "");
                    break;
                case LevelState.Unlocked:
                    MyGrid.ChangeGridLevel(SelectedLevel);
                    break;
                case LevelState.Selected:
                case LevelState.Available:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
