using System;
using System.Collections;
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
    /// <summary>
    /// How long an ingame hour lasts.
    /// </summary>
    /// <seealso href="../../../Documentation/Time scale.xlsx">
    /// Time scale table.
    /// </seealso>
    //[Header("Tick speed")][SerializeField] int ticksPerHour = 4;
    /// <summary>Progresses time by this(60/<see cref="ticksPerHour"/>).</summary>
    private int minutesPerTick;
    public static int MinutesPerTick = 15;

    /// <summary>Current time of the day(counts as hours when starting a new game).</summary>
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

    /// <summary>The tick counter, resets if it reaches uint capacity.</summary>
    [HideInInspector] public uint lastTick = 0;

    [SerializeField] float ticksPerSecond = 4f;
    [SerializeField] float tickTimer = 0f;
    float timeToTick;
    float timerSpeed = 1f;
    public static float LastSpeed;
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
    public void InitTicks()
    {
        minutesPerTick = 5;
        TicksInDay = 1440 / minutesPerTick;
        if (timeInMinutes < 6 * 60 || timeInMinutes > 21 * 60)
            nightStart?.Invoke();

        timeToTick = 1f / ticksPerSecond;
        LastSpeed = 1;
        tickTimer = 0;
        Time.timeScale = 1;
        enabled = false;
    }
    #endregion

    #region Speed Managing
    public void ChangeGameSpeed(float _speed = 0)
    {
        if(timerSpeed != 0)
            LastSpeed = timerSpeed;
        timerSpeed = _speed;
        enabled = !uiOpen && timerSpeed > 0;
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
            enabled = true;
        }
        else
        {
            enabled = false;
        }
    }
    #endregion

    #region Tick
    private void Update()
    {
        tickTimer += Time.unscaledDeltaTime * timerSpeed;
        if(tickTimer > timeToTick)
        {
            tickTimer -= timeToTick;
            if (tickTimer > timeToTick)
                tickTimer = 0;
            UpdateTime();
            tickAction?.Invoke();
            if (lastTick == uint.MaxValue)
                lastTick = 0;
            else
                lastTick++;
        }
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
        timeInMinutes += minutesPerTick;
        switch (timeInMinutes)
        {
            case 1440:
                numberOfDays++;
                day?.Invoke();
                timeInMinutes = 0;
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
                break;
            case 1320:
                nightStart?.Invoke();
                break;
            case 360:
                dayStart?.Invoke();
                break;
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
        gameState.dayTime = timeInMinutes;
        gameState.numberOfDays = numberOfDays;
    }

    /// <summary>
    /// Loads time.
    /// </summary>
    /// <param name="gameState">Where to load from.</param>
    public void Load(GameStateSave gameState)
    {
        timeInMinutes = gameState.dayTime;
        numberOfDays = gameState.numberOfDays;
        UpdateTime();
    }
    #endregion
}
