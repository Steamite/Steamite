using UnityEngine;
using UnityEngine.UIElements;

public class TimeDisplay : MonoBehaviour, IToolkitController
{
    Label minutes;
    Label hours;
    Label days;
    Label months;
    Label years;

    /*readonly int[] daysInMonths = 
        {
            31, 31, 31, 31,
        };*/


    public void Init(VisualElement root)
    {
        root = root.Q<VisualElement>("Time");
        minutes = root.Q<Label>("Minutes");
        hours = root.Q<Label>("Hours");
        days = root.Q<Label>("Days");
        months = root.Q<Label>("Months");
        years = root.Q<Label>("Years");

        SceneRefs.tick.SubscribeToEvent(UpdateTime, Tick.TimeEventType.Ticks);
        SceneRefs.tick.SubscribeToEvent(UpdateDay, Tick.TimeEventType.Day);
        SceneRefs.tick.SubscribeToEvent(UpdateMonth, Tick.TimeEventType.Month);
        SceneRefs.tick.SubscribeToEvent(UpdateYear, Tick.TimeEventType.Year);

        UpdateTime();
        UpdateDay();
        UpdateMonth();
        UpdateYear();
    }

    void UpdateTime()
    {
        minutes.text = (SceneRefs.tick.timeInMinutes % 60).ToString();
        hours.text = (SceneRefs.tick.timeInMinutes / 60).ToString();
        //Debug.Log("Time:" + (SceneRefs.tick.timeInMinutes % 60).ToString());
    }

    void UpdateDay() =>
        days.text = ((SceneRefs.tick.numberOfDays % 28) + 1).ToString();

    void UpdateMonth() =>
        months.text = (((SceneRefs.tick.numberOfDays % 336) / 28) + 1).ToString();

    void UpdateYear() =>
        years.text = (1885 + (SceneRefs.tick.numberOfDays / 336)).ToString();
}
