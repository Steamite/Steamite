using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DayTime : MonoBehaviour
{
    [SerializeField]float ticksPerHour = 4;
    [SerializeField]int minutesPerTick;

    public event Action nightStart;
    public event Action dayStart;
    public event Action weekEnd;

    // time data
    [SerializeField] int timeInMinutes = 4;
    [SerializeField] int numberOfDays = 5;

    [SerializeField] TMP_Text time;

    public void Init(Tick tick, bool newGame)
    {
        minutesPerTick = (int)(60f / ticksPerHour);
        if(newGame)
            timeInMinutes *= 60;
        if (timeInMinutes < 6 * 60 || timeInMinutes > 23 * 60)
            nightStart?.Invoke();
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
                //isNight = true;
                nightStart?.Invoke();
                break;
            case 360:
                //isNight = false;
                dayStart?.Invoke();
                break;
        }
        time.text = $"{(timeInMinutes/60).ToString().PadLeft(2, '0')}:{(timeInMinutes%60).ToString().PadLeft(2, '0')}";
    }

    public int GetWeekTime()
    {
        return (numberOfDays % 7 * 1440) + timeInMinutes;
    }

    public void Save(GameStateSave gameState)
    {
        gameState.dayTime = timeInMinutes;
        gameState.numberOfDays = numberOfDays;
    }

    public void Load(GameStateSave gameState)
    {
        timeInMinutes = gameState.dayTime;
        numberOfDays = gameState.numberOfDays;
    }
}
