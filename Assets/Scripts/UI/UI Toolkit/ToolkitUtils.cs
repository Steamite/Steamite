using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class ToolkitUtils
{
    public static LocalMenu localMenu;
    public static ResourceSkins resSkins;
    public static readonly Color textColor = new(0.6313726f, 0.5803922f, 0.2313726f, 1);

    [RuntimeInitializeOnLoadMethod]
    static void ReloadDomain() => localMenu = null;

    /// <summary>
    /// Switches classes on an element with no transition duration, the duration must be on the classes, else it will not be restored afterwards.
    /// </summary>
    /// <param name="oldClass">Class that needs to be removed.</param>
    /// <param name="newClass">Class that needs to be added.</param>
    /// <param name="element">Element that needs to change.</param>
    public static void ChangeClassWithoutTransition(string oldClass, string newClass, VisualElement element)
    {
        if (oldClass != "" && newClass != "" && element != null)
        {
            element.style.transitionDuration = new List<TimeValue> { new TimeValue(0, TimeUnit.Second) };
            element.RemoveFromClassList(oldClass);
            element.AddToClassList(newClass);
            element.schedule.Execute(() => element.style.transitionDuration = StyleKeyword.Null).ExecuteLater(5);
        }
    }
    public static void RemoveClassWithoutTransition(string oldClass, VisualElement element)
    {
        if (oldClass != "" && element != null)
        {
            element.style.transitionDuration = new List<TimeValue> { new TimeValue(0, TimeUnit.Second) };
            element.RemoveFromClassList(oldClass);
            element.schedule.Execute(() => element.style.transitionDuration = StyleKeyword.Null).ExecuteLater(5);
        }
    }
    public static void AddClassWithoutTransition(string newClass, VisualElement element)
    {
        if (newClass != "" && element != null)
        {
            element.style.transitionDuration = new List<TimeValue> { new TimeValue(0, TimeUnit.Second) };
            element.AddToClassList(newClass);
            element.schedule.Execute(() => element.style.transitionDuration = StyleKeyword.Null).ExecuteLater(5);
        }
    }


    public static void ChangeWithoutTransitions(VisualElement element, Action action)
    {
        element.style.transitionDuration = new List<TimeValue> { new TimeValue(0, TimeUnit.Second) };
        action();
        element.schedule.Execute(() => element.style.transitionDuration = StyleKeyword.Null).ExecuteLater(5);
    }

    public static void Init()
    {
        resSkins = Resources.Load<ResourceSkins>("Holders/Data/Resource Skin");
    }

    public static VisualElement GetRoot(VisualElement element)
    {
        while (element.parent != null)
            element = element.parent;
        return element;
    }
    public static T GetParentOfType<T>(VisualElement element) where T : VisualElement
    {
        while (element is not T && element.parent != null)
            element = element.parent;

        return element as T;
    }
}
