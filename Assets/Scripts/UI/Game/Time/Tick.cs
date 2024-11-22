using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tick : MonoBehaviour
{
    public uint lastTick = 0;

    public event Action tickAction;
    public Button lastButton;

    public DayTime timeController;

    public void AwakeTicks(int clockSpeed)
    {
        Time.timeScale = clockSpeed;
        StartCoroutine(DoTick());
    }

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

            //Debug.Log(lastTick);
        }
    }
}
