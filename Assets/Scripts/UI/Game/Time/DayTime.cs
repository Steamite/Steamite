using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DayTime : MonoBehaviour
{
    float ticksPerHour = 4;
    int minutesPerTick;

    public Action nightStart;
    public Action dayStart;

    // time data
    int timeInMinutes = 12 * 60;
    int numberOfDays = 0;

    [SerializeField] TMP_Text time;

    public void Init(Tick tick)
    {
        minutesPerTick = (int)(60f / ticksPerHour);
        tick.tickAction += UpdateTime;

        transform.GetChild(0).GetComponent<TMP_Text>().text = "12:00";
        transform.GetChild(1).GetComponent<TMP_Text>().text = "Day: 1";
        transform.GetChild(2).GetComponent<TMP_Text>().text = "Week: 1";
        transform.GetChild(3).GetComponent<TMP_Text>().text = "Month: 1";
        transform.GetChild(4).GetComponent<TMP_Text>().text = "Year: 1";
    }

    void UpdateTime()
    {
        timeInMinutes += minutesPerTick;
        switch (timeInMinutes)
        {
            case 1440:
                timeInMinutes = 0;
                numberOfDays++;
                if (numberOfDays % 7 == 0)
                {
                    if (numberOfDays % 28 == 0)
                    {
                        if (numberOfDays % 336 == 0)
                        {
                            transform.GetChild(4).GetComponent<TMP_Text>().text = $"Year: {(numberOfDays / 336) + 1}";
                        }
                        transform.GetChild(3).GetComponent<TMP_Text>().text = $"Month: {((numberOfDays % 336) / 28) + 1}";
                    }
                    transform.GetChild(2).GetComponent<TMP_Text>().text = $"Week: {((numberOfDays % 28) / 7) + 1}";
                }
                transform.GetChild(1).GetComponent<TMP_Text>().text = $"Day: {(numberOfDays % 7)+1}";
                break;
            case 1320:
                if(nightStart != null)
                    nightStart.Invoke();
                break;
            case 360:
                if(dayStart != null)
                    dayStart.Invoke();
                break;
        }
        time.text = $"{(timeInMinutes/60).ToString().PadLeft(2, '0')}:{(timeInMinutes%60).ToString().PadLeft(2, '0')}";
    }
}
