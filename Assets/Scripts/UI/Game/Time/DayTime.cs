using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DayTime : MonoBehaviour
{
    [SerializeField]float ticksPerHour = 4;
    [SerializeField]int minutesPerTick;

    public Action nightStart;
    public Action dayStart;
    public Action weekEnd;

    // time data
    [SerializeField] int timeInMinutes = 4;
    [SerializeField] int numberOfDays = 5;

    [SerializeField] TMP_Text time;

    public void Init(Tick tick)
    {
        minutesPerTick = (int)(60f / ticksPerHour);
        timeInMinutes *= 60;
        tick.tickAction += UpdateTime;

        transform.GetChild(0).GetComponent<TMP_Text>().text = $"{(timeInMinutes / 60).ToString().PadLeft(2, '0')}:{(timeInMinutes % 60).ToString().PadLeft(2, '0')}"; ;
        transform.GetChild(1).GetComponent<TMP_Text>().text = $"Day: {(numberOfDays % 7) + 1}";
        transform.GetChild(2).GetComponent<TMP_Text>().text = $"Week: {((numberOfDays % 28) / 7) + 1}";
        transform.GetChild(3).GetComponent<TMP_Text>().text = $"Month: {((numberOfDays % 336) / 28) + 1}";
        transform.GetChild(4).GetComponent<TMP_Text>().text = $"Year: {(numberOfDays / 336) + 1877}";
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
                    weekEnd?.Invoke();
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
                nightStart?.Invoke();
                break;
            case 360:
                dayStart?.Invoke();
                MyGrid.sceneReferences.GetComponent<SaveController>().SaveGame(true);
                break;
        }
        time.text = $"{(timeInMinutes/60).ToString().PadLeft(2, '0')}:{(timeInMinutes%60).ToString().PadLeft(2, '0')}";
    }

    public int GetWeekTime()
    {
        return (numberOfDays % 7 * 1440) + timeInMinutes;
    }
}
