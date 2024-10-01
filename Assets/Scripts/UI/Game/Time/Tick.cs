using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tick : MonoBehaviour
{
    public event Action tickAction;
    public Button lastButton;
    public static uint lastTick = 0;
    public void AwakeTicks()
    {
        Time.timeScale = 5;
        StartCoroutine(DoTick());
    }
    public void ChangeGameSpeed(float _speed)
    {
        StopAllCoroutines();
        if (_speed > 0)
        {
            Time.timeScale = _speed;
            StartCoroutine(DoTick());
            /*ResearchUIButton button = MyGrid.canvasManager.research.GetComponent<ResearchBackend>().currentResearch;
            if (button)
                button.transform.GetChild(0).GetComponent<Animator>().SetFloat("gameSpeed", 5f / _speed);*/
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
            if (lastTick == 4294967295)
                lastTick = 0;
            else
                lastTick++;
            Debug.Log(lastTick);
        }
    }
}
