using System;
using System.Collections.Generic;
using AbstractControls;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class TimeButtons : CustomRadioButtonGroup
{
    [UxmlAttribute] List<int> speedStates = new() { 0, 1, 2, 10 };
    [UxmlAttribute][Range(0, 3)] int startState;
    public TimeButtons() : base()
    {
        for (int i = 0; i < speedStates?.Count; i++)
        {
            CustomRadioButton button = new("speed-button", i, true);
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
        SetChangeCallback((i) => SceneRefs.tick.ChangeGameSpeed(speedStates[SelectedChoice]));
    }

    public void Start()
    {
        SelectedChoice = 0;
        ((CustomRadioButton)ElementAt(startState)).SelectWithoutTransition(false);
    }

    public void OutsideTrigger(int i)
    {
        if (SelectedChoice == 0 && i == 0)
            i = speedStates.IndexOf(Convert.ToInt32(Time.timeScale));
        ((CustomRadioButton)ElementAt(i)).Select();
    }
}
