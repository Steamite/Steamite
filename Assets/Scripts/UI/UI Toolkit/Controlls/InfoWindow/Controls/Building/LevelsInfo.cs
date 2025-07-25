using RadioGroups;
using System;
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
    public partial class LevelInfo : InfoWindowControl
    {
        /// <summary>Data for unlocking and displaying info about each of the levels.</summary>
        public LevelPresent LevelData { get; private set; }
        /// <summary>Active level in the <see cref="levelGroup"/>.</summary>
        public int SelectedLevel { get; private set; } = -1;

        /// <summary>Control for level unlockers.</summary>
        LevelUnlockerRadioList levelGroup;

        Label headerLabel;
        Label stateLabel;
        Label bodyLabel;
        DoubleResList costList;
        /// <summary>Button for moving between levels or unlocking new ones.</summary>
        Button moveButton;

        /// <summary>
        /// Creates a new <see cref="Tab"/> and initializes all parts of the view.
        /// </summary>
        public LevelInfo()
        {
            LevelData = Resources.Load<LevelPresent>("Holders/Data/Level Present");
            foreach (var item in LevelData.costs)
                item?.Init();
            style.flexGrow = 1;
            contentContainer.style.flexDirection = FlexDirection.Row;
            name = "Levels";

            levelGroup = new LevelUnlockerRadioList();
            levelGroup.Init(
                (i) =>
                {
                    ChangeActiveView(i);
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

            VisualElement element = new()
            {
                style =
                {
                    flexGrow = 1,
                    minWidth = new Length(100, LengthUnit.Percent),
                    justifyContent = Justify.FlexEnd

                }
            };

            costList = new(true, "Cost")
            {
                style =
                {
                    flexGrow = 1,
                    marginTop = 0,
                    minHeight = 200
                },
            };
            element.Add(costList);

            moveButton = new() 
            { 
                style = 
                {
                    minWidth = new Length(100, LengthUnit.Percent),
                    maxWidth = new Length(100, LengthUnit.Percent),
                    marginBottom = 15,
                    paddingLeft = 0,
                    height = new Length(50, LengthUnit.Pixel)
                },
            };
            moveButton.AddToClassList("main-button");
            moveButton.RegisterCallback<ClickEvent>(HandleButton);
            element.Add(moveButton);

            view.Add(element);
            Add(view);
        }

        /// <summary>
        /// Refreshes <see cref="levelGroup"/> and level info.
        /// </summary>
        /// <param name="data">Elevator that is selected.</param>
        public override void Open(object data)
        {
            if (SelectedLevel == -1)
            {
                levelGroup.Rebuild();
                levelGroup.RegisterCallbackOnce<GeometryChangedEvent>(
                    (_) => SelectedLevel = levelGroup.Open((Elevator)data, LevelData));
            }
            else
                SelectedLevel = levelGroup.Open((Elevator)data, LevelData);
        }

        /// <summary>
        /// Switches level info and Updates <see cref="moveButton"/>.
        /// </summary>
        /// <param name="i"></param>
        void ChangeActiveView(int i)
        {
            if (i == -1)
                return;
            headerLabel.text = LevelData.headers[i];
            bodyLabel.text = LevelData.bodies[i];
            SelectedLevel = i;

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
                    ConfirmWindow.window.Open(
                        () =>
                        {
                            SceneRefs.ObjectFactory.CreateElevator(
                                new(MyGrid.gridSize(SelectedLevel) / 2, SelectedLevel, MyGrid.gridSize(SelectedLevel) / 2));
                            MyRes.PayCostGlobal(LevelData.costs[SelectedLevel]);
                            MyGrid.UnlockLevel(SelectedLevel);
                            MyGrid.ChangeGridLevel(SelectedLevel);
                        }, 
                        "Unlock new level",
                        $"Do you want to unlock the {LevelData.headers[SelectedLevel]}?",
                        "Unlock",
                        "Cancel");
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
