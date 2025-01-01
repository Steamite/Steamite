using System;
using UnityEngine.UIElements;

[UxmlElement]
public partial class LevelUnlocker : Button
{
    public LevelUnlocker() : base()
    {

    }

    public LevelUnlocker(int i, LevelState state, Action<int> onHover) : base()
    {
        text = i.ToString();
        RegisterCallback<ClickEvent>((_) => onHover(i));
        ToggleButtonStyle(state);
    }

    public void ToggleButtonStyle(LevelState state)
    {
        ClearClassList();
        switch (state)
        {
            case LevelState.Selected:
                enabledSelf = true;
                AddToClassList("Level-Selected");
                break;
            case LevelState.Available:
                enabledSelf = true;
                AddToClassList("Level-Opened");
                break;
            case LevelState.Unavailable:
                enabledSelf = false;
                break;
        }
    }
}