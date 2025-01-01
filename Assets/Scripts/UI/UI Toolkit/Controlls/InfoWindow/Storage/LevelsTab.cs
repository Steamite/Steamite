using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum LevelState
{
    Selected,
    Available,
    Unavailable
}
[UxmlElement]
public partial class LevelsTab : Tab, IUIElement
{
    [UxmlAttribute] List<LevelState> states = new() 
    { 
        LevelState.Selected, 
        LevelState.Available, 
        LevelState.Unavailable, 
        LevelState.Unavailable, 
        LevelState.Unavailable 
    };

    [UxmlAttribute] string[] headers = { "First floor", "Cave", "Water", "Random Header", "Deppest point" };
    [UxmlAttribute] string[] bodies = { "aaaaaaaaa", "bbbbbbb", "cccccc", "ddddddd", "eeeeeee" };

    //Resource[] levelcosts = new/{}

    Label headerLabel;
    Label stateLabel;
    Label bodyLabel;

    Button moveButton;
    List<LevelUnlocker> levelUnlockers;


    public LevelsTab() : base("Levels")
    {
        style.flexGrow = 1;
        contentContainer.style.flexDirection = FlexDirection.Row;
        name = "Levels";

        GroupBox levels = new();
        levelUnlockers = new();
        for (int i = 0; i < 5; i++)
        {
            levelUnlockers.Add(new LevelUnlocker(i, states[i], ChangeActiveView));
            levels.Add(levelUnlockers[i]);
        }
        Add(levels);

        VisualElement view = new();
        view.name = "Level-View";
        
        headerLabel = new(headers[0]);
        headerLabel.name = "Header";
        view.Add(headerLabel);

        stateLabel = new("OK");
        stateLabel.name = "State";
        view.Add(stateLabel);

        bodyLabel = new(bodies[0]);
        bodyLabel.name = "Body";
        view.Add(bodyLabel);

        moveButton = new();
        view.Add(moveButton);
        Add(view);
    }
    public void Fill(object data)
    {
        Storage storage = (Storage)data;
        GridPos gridPos = storage.GetPos();
        for (int i = 0; i < levelUnlockers.Count; i++)
        {

        }
    }
    void ChangeActiveView(int i)
    {
        if (states[i] != LevelState.Available)
            return;
        headerLabel.text = headers[i];
        bodyLabel.text = bodies[i];

        int x = states.IndexOf(LevelState.Selected);
        states[x] = LevelState.Available;
        levelUnlockers[x].ToggleButtonStyle(states[x]);

        states[i] = LevelState.Selected;
        levelUnlockers[i].ToggleButtonStyle(states[i]);

        
    }
}
