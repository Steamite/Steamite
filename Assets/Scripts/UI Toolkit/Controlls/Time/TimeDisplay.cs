using UnityEngine;
using UnityEngine.UIElements;

public class TimeDisplay : MonoBehaviour, IToolkitController
{
    Label hour;
    Label minute;
    Label month;
    Label day;
    Label year;


    public void Init(VisualElement root)
    {
        root = root.Q<VisualElement>("TimeDisplay");
        hour = root.Q<Label>("Hour");
        minute = root.Q<Label>("Minute");
        day = root.Q<Label>("Day");
        month = root.Q<Label>("Month");
        year = root.Q<Label>("Year");

        SceneRefs.Tick.SubscribeToEvent(UpdateTime, Tick.TimeEventType.Ticks);
        SceneRefs.Tick.SubscribeToEvent(UpdateDay, Tick.TimeEventType.Day);
        SceneRefs.Tick.SubscribeToEvent(UpdateYear, Tick.TimeEventType.Year);



        UpdateTime();
        UpdateDay();
        UpdateYear();
    }

    void UpdateTime()
    {
        hour.text = $"{SceneRefs.Tick.timeInMinutes / 60:00}";
        minute.text = $"{SceneRefs.Tick.timeInMinutes % 60:00}";
    }

    void UpdateDay()
    {
        day.text = $"{(SceneRefs.Tick.numberOfDays % 28) + 1}.";
        month.text = $"{(SceneRefs.Tick.numberOfDays / 28) + 1}.";
    }

    void UpdateYear() =>
        year.text = (1885 + (SceneRefs.Tick.numberOfDays / 336)).ToString();
}
