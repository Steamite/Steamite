using AbstractControls;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class TimeButtons : ShortcutRadioButtonGroup, IInitiableUI
{
    public TimeButtons() : base()
    {
        
    }

    public void Init()
    {
        var speedStates = SceneRefs.Tick.GetSpeedList();
        buttons = new();
        for (int i = 0; i < speedStates?.Count; i++)
        {
            CustomRadioButton button = new("status-bar-button", i, this);
            if (i == 0)
            {
                button.iconImage = Resources.Load<Texture2D>("Icon/Pause");
            }
            else
            {
                button.text = $"{speedStates[i]}x";
            }
            Add(button);
        }
        SetChangeCallback(SceneRefs.Tick.ChangeGameSpeed);

        SelectedChoice = SceneRefs.Tick.GetGameSpeed();

        buttons[SelectedChoice].SelectWithoutTransition(false);
    }
}
