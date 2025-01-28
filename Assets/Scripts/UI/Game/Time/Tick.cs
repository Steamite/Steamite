using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Handles the game clock.</summary>
public class Tick : MonoBehaviour
{
    #region Variables
    /// <summary>The tick counter, resets if it reaches uint capacity.</summary>
    public uint lastTick = 0;
    /// <summary>The most subscribed action in the whole project, Triggers each tick.</summary>
    public event Action tickAction;
    /// <summary>Handles user input for changing game speed.</summary>
    public DayTime timeController;
    #endregion

    public void AwakeTicks(int clockSpeed)
    {
        Time.timeScale = clockSpeed;
        StartCoroutine(DoTick());
    }

    #region Speed Managing
    public void ChangeGameSpeed(float _speed)
    {
        StopAllCoroutines();
        if (_speed > 0)
        {
            Time.timeScale = _speed;
            StartCoroutine(DoTick());
        }
    }

    public void Unpause()
    {
        StopAllCoroutines();
        StartCoroutine(DoTick());
    }
    #endregion

    IEnumerator DoTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            tickAction?.Invoke();
            if (lastTick == 4294967295)
                lastTick = 0;
            else
                lastTick++;
        }
    }
}
