using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class TimeButtons : RadioButtons
{
    public Tick tick;

    public void SetStartSpeed(Tick _tick)
    {
        tick = _tick;
        tick.AwakeTicks(states[currentState]);
    }

    protected override void ButtonTrigger(Button button, int index)
    {
        if (currentState == index)
        {
            if (index > 0)
                return;
            index = states.IndexOf(Mathf.RoundToInt(Time.timeScale));
            button = transform.GetComponentsInChildren<Button>()[index];
        }
        tick.ChangeGameSpeed(states[index]);
        base.ButtonTrigger(button, index);
    }
}
