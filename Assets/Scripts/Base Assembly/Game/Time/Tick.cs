using System;
using System.Collections;
using UnityEngine;

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
    [Header("Tick speed")][SerializeField] float ticksPerHour = 4;
    /// <summary>Progresses time by this(60/<see cref="ticksPerHour"/>).</summary>
    private int minutesPerTick;
    public int MinutesPerTick = 15;

    /// <summary>Current time of the day(counts as hours when starting a new game).</summary>
    public int timeInMinutes = 4;
    /// <summary>Current number of days, increased each new day.<summary>
    public int numberOfDays = 5;

    public static int TicksInDay;
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

    /// <summary>The tick counter, resets if it reaches uint capacity.</summary>
    [HideInInspector] public uint lastTick = 0;

    bool running = false;

    [SerializeField]float pauseSpeed = 0.1f;
    public static float LastSpeed { get; private set; }
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
        return (numberOfDays % 7 * 1440) + timeInMinutes;
    }
    #endregion

    #region Init
    public void InitTicks()
    {
        minutesPerTick = (int)(60f / ticksPerHour);
        TicksInDay = 1440 / minutesPerTick;
        if (timeInMinutes < 6 * 60 || timeInMinutes > 21 * 60)
            nightStart?.Invoke();

        Time.timeScale = pauseSpeed;
        LastSpeed = 1f;
    }
    #endregion

    #region Speed Managing
    public void ChangeGameSpeed(float _speed = 0)
    {
        if (_speed == 0 && running == false)
        {
            Time.timeScale = LastSpeed;
            StartTicks();
            Debug.Log("Can't get here");
        }
        else if (_speed > 0)
        {
            Time.timeScale = _speed;
            if (running == false)
                StartTicks();
        }
        else
        {
            StopTicks();
        }
    }
    #endregion

    #region Starting and Ending ticks
    public void StopTicks()
    {
        LastSpeed = Time.timeScale;
        Time.timeScale = pauseSpeed;
        if (running == false)
        {
            Debug.LogWarning("Not ticking, cannot stop ticks.");
        }
        else
        {
            running = false;
            StopAllCoroutines();
        }
    }

    public void StartTicks()
    {
        if (running == true)
        {
            Debug.LogError("Already ticking, cannot tick two times.");
        }
        else
        {
            running = true;
            StartCoroutine(DoTick());
        }
    }

    public void UIWindowToggle(bool enable)
    {
        if (!enable)
            StopAllCoroutines();
        else if (running)
            StartCoroutine(DoTick());

    }
    #endregion

    #region Tick
    IEnumerator DoTick()
    {
        while (true)
        {
            //Debug.Log("All time:" + Time.time);
            yield return new WaitForSeconds(1);
            UpdateTime();
            tickAction?.Invoke();
            if (lastTick == 4294967295)
                lastTick = 0;
            else
                lastTick++;
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
                    if (numberOfDays % 28 == 0)
                    {
                        monthStart?.Invoke();
                        if (numberOfDays % 336 == 0)
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
