using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;


/// <summary>Counts, shows and triggers events related to time.</summary>
public class DayTime : MonoBehaviour
{
    /// <summary>Events used for event subscribing.</summary>
	public enum TimeEventType
	{
        /// <inheritdoc cref="dayStart"/>
		Day,
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
	[SerializeField]float ticksPerHour = 4;
    /// <summary>Progresses time by this(60/<see cref="ticksPerHour"/>).</summary>
    [SerializeField]int minutesPerTick;

	/// <summary>Subscribable event, triggered when starting Day(06:00).</summary>
	event Action dayStart;
    /// <summary>Subscribable event, triggered when starting Night(21:00).</summary>
    event Action nightStart;
    /// <summary>Subscriable event, triggered when starting a Week.</summary>
    event Action weekStart;

    /// <summary>Current time of the day(counts as hours when starting a new game).</summary>
    [SerializeField] int timeInMinutes = 4;
    /// <summary>Current number of days, increased each new day.<summary>
    [SerializeField] int numberOfDays = 5;

    /// <summary>Text that shows the time.<summary>
    [SerializeField] TMP_Text time;
	#endregion

	#region Listeners
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
			case TimeEventType.Day:
                dayStart += subscriber;
				break;
			case TimeEventType.Night:
                nightStart += subscriber;
				break;
			case TimeEventType.Week:
                weekStart += subscriber;
				break;
			case TimeEventType.Month:
			case TimeEventType.Year:
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
			case TimeEventType.Day:
				dayStart -= subscriber;
				break;
			case TimeEventType.Night:
				nightStart -= subscriber;
				break;
			case TimeEventType.Week:
				weekStart -= subscriber;
				break;
			case TimeEventType.Month:
			case TimeEventType.Year:
			default:
				throw new NotImplementedException();
		}
	}
	#endregion


	/// <summary>
	/// Calculates <see cref="minutesPerTick"/>, links tick event and default all parts of text.
	/// </summary>
	/// <param name="tick">Reference to link to the tick event.</param>
	/// <param name="newGame">If new game multiply <see cref="timeInMinutes"/> by 60.</param>
	public void Init(Tick tick, bool newGame)
    {
        minutesPerTick = (int)(60f / ticksPerHour);
        if(newGame)
            timeInMinutes *= 60;
        if (timeInMinutes < 6 * 60 || timeInMinutes > 21 * 60)
            nightStart?.Invoke();
        tick.SubscribeToTicks(UpdateTime);

        transform.GetChild(0).GetComponent<TMP_Text>().text = $"{(timeInMinutes / 60).ToString().PadLeft(2, '0')}:{(timeInMinutes % 60).ToString().PadLeft(2, '0')}"; ;
        transform.GetChild(1).GetComponent<TMP_Text>().text = $"Day: {(numberOfDays % 7) + 1}";
        transform.GetChild(2).GetComponent<TMP_Text>().text = $"Week: {((numberOfDays % 28) / 7) + 1}";
        transform.GetChild(3).GetComponent<TMP_Text>().text = $"Month: {((numberOfDays % 336) / 28) + 1}";
        transform.GetChild(4).GetComponent<TMP_Text>().text = $"Year: {(numberOfDays / 336) + 1877}";
    }

    /// <summary>Tick action, progresses time and handles special time cases.</summary>
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
                    weekStart?.Invoke();
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
                break;
        }
        time.text = $"{(timeInMinutes/60).ToString().PadLeft(2, '0')}:{(timeInMinutes%60).ToString().PadLeft(2, '0')}";
    }

    /// <summary>
    /// Returns how much time has elepsed since the beging of the week.
    /// </summary>
    /// <returns>Time since the start of the week.</returns>
    public int GetWeekTime()
    {
        return (numberOfDays % 7 * 1440) + timeInMinutes;
    }

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
    }
	#endregion
}
