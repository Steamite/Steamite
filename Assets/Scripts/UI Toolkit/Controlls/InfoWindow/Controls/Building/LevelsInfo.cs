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
    }

    /// <summary>Tab for <see cref="Elevator"/> "Levels" tab.</summary>
    [UxmlElement]
    public partial class LevelInfo : InfoWindowControl
    {
        /// <summary>Data for unlocking and displaying info about each of the levels.</summary>
        public LevelPresent LevelData { get; private set; }
        /// <summary>Active level in the <see cref="levelGroup"/>.</summary>
        public int SelectedLevel { get; private set; } = -1;
        Elevator selectedElevator;

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
                    marginTop = 0,
                    marginLeft = 0,
                    marginRight = 0,

                    paddingBottom = 0,
                    paddingTop = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    fontSize = 45,

                    height = new Length(60, LengthUnit.Pixel)
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
            selectedElevator = data as Elevator;
            if (SelectedLevel == -1)
            {
                levelGroup.Rebuild();
                levelGroup.RegisterCallbackOnce<GeometryChangedEvent>(
                    (_) => SelectedLevel = levelGroup.Open(selectedElevator, LevelData));
            }
            else
                SelectedLevel = levelGroup.Open(selectedElevator, LevelData);
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
                    costList.style.display = DisplayStyle.Flex;
                    costList.Open(this);
                    stateLabel.text = "Not enough resources";
                    MoveButtonUpdate(true, MyGrid.IsUnlocked(i) ? "Connect" : "Unlock");

                    break;
                case LevelState.CanUnlock:
                    costList.style.display = DisplayStyle.Flex;
                    costList.Open(this);
                    if (MyGrid.IsUnlocked(i))
                    {
                        stateLabel.text = "Can connect";
                        MoveButtonUpdate(true, "Connect");
                    }
                    else
                    {
                        stateLabel.text = "Can unlock";
                        MoveButtonUpdate(true, "Unlock");
                    }
                    break;
                case LevelState.Unlocked:
                    costList.style.display = DisplayStyle.None;
                    stateLabel.text = "Unlocked";
                    MoveButtonUpdate(MyGrid.currentLevel != i, "Move to");
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
                    if (MyGrid.IsUnlocked(SelectedLevel))
                    {
                        ConfirmWindow.window.Open(
                            () =>
                            {
                                GridPos pos = selectedElevator.GetPos();
                                MyRes.PayCostGlobal(LevelData.costs[SelectedLevel]);
                                SceneRefs.ObjectFactory.CreateElevator(
                                    new(pos.x, SelectedLevel, pos.z),
                                    Mathf.RoundToInt(selectedElevator.transform.eulerAngles.y));
                                MyGrid.ChangeGridLevel(SelectedLevel);
                            },
                            $"Connect to level: {SelectedLevel}",
                            $"Do you want to connect to the {LevelData.headers[SelectedLevel]}?",
                            "Connect",
                            "Cancel");
                    }
                    else
                    {
                        ConfirmWindow.window.Open(
                            () =>
                            {
                                GridPos pos = selectedElevator.GetPos();
                                MyRes.PayCostGlobal(LevelData.costs[SelectedLevel]);
                                SceneRefs.ObjectFactory.CreateElevator(new(pos.x, SelectedLevel, pos.z));
                                MyGrid.UnlockLevel(selectedElevator, SelectedLevel);
                                MyGrid.ChangeGridLevel(SelectedLevel);
                            },
                            "Unlock new level",
                            $"Do you want to unlock the {LevelData.headers[SelectedLevel]}?",
                            "Unlock",
                            "Cancel");
                    }
                    break;
                case LevelState.Unlocked:
                    MyGrid.ChangeGridLevel(SelectedLevel);
                    break;
                //case LevelState.Selected:
                case LevelState.Available:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
