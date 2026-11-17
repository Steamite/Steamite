using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>Handles the game clock.</summary>
public class Tick : MonoBehaviour
{
    /// <summary>Events used for event subscribing.</summary>
    public enum TimeEventType
    {
        /// <summary>Each tick</summary>
        Ticks,
        /// <inheritdoc cref="day"/>
        Day,
        /// <inheritdoc cref="dayStart"/>
        DayStart,
        /// <inheritdoc cref="nightStart"/>
        Night,
        /// <inheritdoc cref="weekStart"/>
        Week,
        /// <summary>WIP</summary>
        Month,
        /// <summary>WIP</summary>
        Year
    }

    #region Variables

    [Header("Tick speed")]
    /// <summary>Progresses time</summary>
    [SerializeField] private int minutesPerTick = 5;
    static int MinutesPerTick;
    
    /// <summary>Current time of the dat in real world seconds.</summary>
    public float timeElapsed = 4;

    /// <summary>Current time of the day in minutes.</summary>
    public int timeInMinutes = 4;
    
    
    /// <summary>Current number of days, increased each new day.<summary>
    public int numberOfDays = 5;

    public static int TicksInDay;

    #region Actions
    /// <summary>The most subscribed action in the whole project, Triggers each tick.</summary>
    event Action tickAction;
    /// <summary>Subscribable event, triggered when starting Day(00:00).</summary>
    event Action day;
    /// <summary>Subscribable event, triggered when starting Day(06:00).</summary>
    event Action dayStart;
    /// <summary>Subscribable event, triggered when starting Night(21:00).</summary>
    event Action nightStart;
    /// <summary>Subscriable event, triggered when starting a Week.</summary>
    event Action weekStart;
    /// <summary>Subscriable event, triggered when starting a Month.</summary>
    event Action monthStart;
    /// <summary>Subscriable event, triggered when starting a Year.</summary>
    event Action yearStart;
    #endregion

    [SerializeField] int _dayTime = 6;
    [NonSerialized] public int dayTime;

    [SerializeField] int _nightTime = 21;
    [NonSerialized] public int nightTime;
    
    
    /// <summary>The tick counter, resets if it reaches uint capacity.</summary>
    [HideInInspector] public uint lastTick = 0;

    [SerializeField] List<int> speeds;
    [SerializeField] int speedIndex;
    [SerializeField] int prevSpeedIndex;


    //float timeToTick;
    float timerSpeed;

    bool uiOpen = false;
    #endregion

    #region Events
    /// <summary>
    /// Allows other objects to listen to time events.
    /// </summary>
    /// <param name="subscriber">What to do when the event triggers.</param>
    /// <param name="timeEvent">Which event to listen to.</param>
    /// <exception cref="NotImplementedException"></exception>
    public void SubscribeToEvent(Action subscriber, TimeEventType timeEvent)
    {
        switch (timeEvent)
        {
            case TimeEventType.Ticks:
                tickAction += subscriber;
                break;
            case TimeEventType.Day:
                day += subscriber;
                break;
            case TimeEventType.DayStart:
                dayStart += subscriber;
                break;
            case TimeEventType.Night:
                nightStart += subscriber;
                break;
            case TimeEventType.Week:
                weekStart += subscriber;
                break;
            case TimeEventType.Month:
                monthStart += subscriber;
                break;
            case TimeEventType.Year:
                yearStart += subscriber;
                break;
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Removes listeners for time events.
    /// </summary>
    /// <param name="subscriber">Listener to remove.</param>
    /// <param name="timeEvent">Which event to remove from.</param>
    /// <exception cref="NotImplementedException"></exception>
    public void UnsubscribeToEvent(Action subscriber, TimeEventType timeEvent)
    {
        switch (timeEvent)
        {
            case TimeEventType.Ticks:
                tickAction -= subscriber;
                break;
            case TimeEventType.Day:
                day -= subscriber;
                break;
            case TimeEventType.DayStart:
                dayStart -= subscriber;
                break;
            case TimeEventType.Night:
                nightStart -= subscriber;
                break;
            case TimeEventType.Week:
                weekStart -= subscriber;
                break;
            case TimeEventType.Month:
                monthStart -= subscriber;
                break;
            case TimeEventType.Year:
                yearStart -= subscriber;
                break;
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Returns how much time has elepsed since the beging of the week.
    /// </summary>
    /// <returns>Time since the start of the week.</returns>
    public int GetWeekTime()
    {
        return ((numberOfDays % 7 * 24 * 60) + timeInMinutes) / minutesPerTick;
    }

    public int GetWeekTimeFull()
    {
        return ((7 * 24 * 60) + timeInMinutes) / minutesPerTick;
    }
    #endregion

    #region Init

    bool wasDayStart = false;
    bool wasNightStart = false;

    public void InitTicks()
    {
        MinutesPerTick = minutesPerTick;

        
        TicksInDay = 1440 / minutesPerTick;
        timeInMinutes = Mathf.FloorToInt(timeElapsed * minutesPerTick);

        dayTime = _dayTime * 60;
        nightTime = _nightTime * 60;

        if (timeInMinutes < dayTime) 
        {
            wasNightStart = false;
            wasDayStart = false;
            nightStart?.Invoke();
        }
        else if (timeInMinutes > nightTime)
        {
            wasNightStart = true;
            wasDayStart = true;
            nightStart?.Invoke();
        }
        else
        {
            wasNightStart = false;
            wasDayStart = true;
        }

        timerSpeed = 0;
        GameTime.SetTimeScale(0);

        speedIndex = 0;
        prevSpeedIndex = 1;

        enabled = false;

        UpdateTime();
    }
    #endregion

    #region Speed Managing
    public List<int> GetSpeedList() => speeds;
    public int GetGameSpeed() => speedIndex;

    public void ChangeGameSpeed(int i)
    {
        if (i == 0 && speedIndex == 0)
            speedIndex = prevSpeedIndex;
        else if (i == speedIndex)
            return;
        else
        {
            prevSpeedIndex = speedIndex;
            speedIndex = i;
        }

        timerSpeed = speeds[speedIndex];
        GameTime.SetTimeScale(timerSpeed);
        enabled = !uiOpen && speedIndex > 0;
    }


    public void ChangeGameSpeed(int i, Action<int> a)
    {
        ChangeGameSpeed(i);
        a?.Invoke(speedIndex);
    }
    #endregion

    #region Starting and Ending ticks

    /// <summary>
    /// Directly stop/start clock, because time could have been stoped by the player before, so we can't use Stop and Start Ticks.
    /// </summary>
    /// <param name="_enable"></param>
    public void UIWindowToggle(bool _enable)
    {
        uiOpen = !_enable;
        if (_enable)
        {
            GameTime.SetTimeScale(timerSpeed);
            enabled = true;
        }
        else
        {
            GameTime.SetTimeScale(0);
            enabled = false;
        }
    }
    #endregion

    #region Tick
    private void Update()
    {
        timeElapsed += GameTime.DeltaTime;
        UpdateTime();
        tickAction?.Invoke();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (Application.runInBackground)
            return;
        
        if (focus)
        {
            enabled = true;
        }
        else
        {
            enabled = false;
        }
    }
    void UpdateTime()
    {
        timeInMinutes = Mathf.FloorToInt(timeElapsed * minutesPerTick);
        if(!wasDayStart && timeInMinutes > 360)
        {
            wasDayStart = true;
            dayStart?.Invoke();
        }
        else if(!wasNightStart && timeInMinutes > 1320)
        {
            wasNightStart = true;
        }
        else if(timeInMinutes > 1440)
        {
            numberOfDays++;
            day?.Invoke();
            
            wasDayStart = false;
            wasNightStart = false;


            timeElapsed -= 1440 / minutesPerTick;
            timeInMinutes = Mathf.FloorToInt(timeElapsed * minutesPerTick);


            if (numberOfDays % 7 == 0)
            {
                weekStart?.Invoke();
                if (numberOfDays % 30 == 0)
                {
                    monthStart?.Invoke();
                    if (numberOfDays % 360 == 0)
                    {
                        yearStart?.Invoke();
                    }
                }
            }
        }

    }

    public static string RemainingTime(int time)
        => $"({RemainingTimeWithoutBrackets(time)})";

    public static string RemainingTimeWithoutBrackets(int time)
    {
        TimeSpan span = TimeSpan.FromMinutes(time * MinutesPerTick);
        if (span.Days < 1)
        {
            return $"{span.Hours} h {span.Minutes}m";
        }
        return $"{span.Days} d";
    }
    #endregion

    #region Saving
    /// <summary>
    /// Saves time.
    /// </summary>
    /// <param name="gameState">Where to save.</param>
    public void Save(GameStateSave gameState)
    {
        gameState.dayTime = timeElapsed;
        gameState.numberOfDays = numberOfDays;
    }

    /// <summary>
    /// Loads time.
    /// </summary>
    /// <param name="gameState">Where to load from.</param>
    public void Load(GameStateSave gameState)
    {
        timeElapsed = gameState.dayTime;
        numberOfDays = gameState.numberOfDays;
    }


    #endregion
}
