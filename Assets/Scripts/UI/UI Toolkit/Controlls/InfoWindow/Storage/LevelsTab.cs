using AbstractControls;
using RadioGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

namespace InfoWindowElements
{

    public enum LevelState
    {
        Unavailable, // too far
        Available, // next locked level, cannot afford at the moment
        CanUnlock, // next locked level, can afford at the moment
        Unlocked, // unlocked level
        Selected, // selected level

    }
    [UxmlElement]
    public partial class LevelsTab : Tab
    {
        public LevelPresent LevelData { get; private set; }
        public int SelectedLevel { get; private set; }

        LevelUnlockerRadioGroup levelGroup;

        Label headerLabel;
        Label stateLabel;
        Label bodyLabel;
        DoubleResourceList costList;
        Button moveButton;

        public LevelsTab() : base("Levels")
        {
            LevelData = Resources.Load<LevelPresent>("Holders/Data/Level Present");
            style.flexGrow = 1;
            contentContainer.style.flexDirection = FlexDirection.Row;
            name = "Levels";

            levelGroup = new LevelUnlockerRadioGroup();
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

        public void Open(object data)
        {
            SelectedLevel = 0;
            levelGroup.SetFill((Storage)data, LevelData);
            ChangeActiveView(SelectedLevel);
        }

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
                    costList.Fill(this);
                    stateLabel.text = "Not enough resources";
                    MoveButtonUpdate(true, "Unlock");
                    break;
                case LevelState.CanUnlock:
                    moveButton.text = "Unlock";
                    costList.style.display = DisplayStyle.Flex;
                    costList.Fill(this);
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

        public void UpdateState()
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

        void MoveButtonUpdate(bool active, string name)
        {
            moveButton.text = name;
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
