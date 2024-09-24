using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tick : MonoBehaviour
{
    public event Action tickAction;
    public Button lastButton;
    public void AwakeTicks()
    {
        Time.timeScale = 5;
        StartCoroutine(DoTick());
    }
    public void ChangeGameSpeed(int _speed)
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
    public IEnumerator DoTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            tickAction?.Invoke();
        }
    }
}
