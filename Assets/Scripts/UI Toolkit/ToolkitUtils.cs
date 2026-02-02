using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class ToolkitUtils
{
    public static LocalMenu localMenu;

    const string MULTY_COLUMN = "unity-multi-column-view__row-container";
    const string LIST_VIEW = "unity-list-view__item";

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

    public static void ToggleStyleButton(this Button button, bool activate)
    {
        if (activate)
        {
            button.RemoveFromClassList("disabled-button");
            button.AddToClassList("main-button");
        }
        else
        {
            button.AddToClassList("disabled-button");
            button.RemoveFromClassList("main-button");
        }
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

    /// <summary>Goes up the hierarchy to find index of the <paramref name="element"/>.</summary>
    /// <param name="element">Element that is to be found.</param>
    /// <returns>Index of the entry.</returns>
    public static int GetRowIndex(this VisualElement element, bool multicolumn = true)
    {
        if (multicolumn)
        {
            while (element.name != MULTY_COLUMN)
                element = element.parent;
            return element.parent.IndexOf(element);
        }
        else
        {
            while (!element.ClassListContains(LIST_VIEW))
                element = element.hierarchy.parent;
            return element.hierarchy.parent.IndexOf(element);
        }

    }

    public static int GetRowIndex(this IEventHandler handler, bool multicolumn = true)
        => GetRowIndex(handler as VisualElement, multicolumn);
}
