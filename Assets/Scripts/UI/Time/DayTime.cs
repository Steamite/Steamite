using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DayTime : MonoBehaviour
{
    int dayT = 60 * 10;
    int increment = 60;
    [SerializeField] Humans humans;

    public static Action night;
    public static Action day;

    private void Start()
    {
        StartCoroutine(CountTime());
    }
    IEnumerator CountTime()
    {
        //yield return new WaitForSeconds(1);
        while (true)
        {
            dayT += increment;
            gameObject.GetComponent<TMP_Text>().text = $"Time: {dayT/60}:{dayT%60}";
            if(dayT == 5 * 60)
            {
                print("day, go to work!!!");
                day?.Invoke();
                
            }
            else if(dayT == 20 * 60)
            {
                print("night, go to sleep");
                night?.Invoke();
            }
            else if(dayT == 24 * 60)
            {
                print("new DAY!!!");
                dayT = 0;
                // místo pro save 
            }
            yield return new WaitForSeconds(1);
        }
    }
}
