using LocalMenuUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class ToolkitUtils
{
    const string MULTY_COLUMN = "unity-multi-column-view__row-container";
    const string LIST_VIEW = "unity-list-view__item";

    /// <summary>
    /// Switches classes on an element with no transition duration, the duration must be on the classes, else it will not be restored afterwards.
    /// </summary>
    /// <param name="oldClass">Class that needs to be removed.</param>
    /// <param name="newClass">Class that needs to be added.</param>
    /// <param name="element">Element that needs to change.</param>
    public static void ChangeClassWithoutTransition(this VisualElement element, string oldClass, string newClass)
    {
        if (oldClass != "" && newClass != "" && element != null)
        {
            element.style.transitionDuration = new List<TimeValue> { new TimeValue(0, TimeUnit.Second) };
            element.RemoveFromClassList(oldClass);
            element.AddToClassList(newClass);
            element.schedule.Execute(() => element.style.transitionDuration = StyleKeyword.Null).ExecuteLater(5);
        }
    }
    public static void RemoveClassWithoutTransition(this VisualElement element, string classToRemove)
    {
        if (classToRemove != "" && element != null)
        {
            element.style.transitionDuration = new List<TimeValue> { new TimeValue(0, TimeUnit.Second) };
            element.RemoveFromClassList(classToRemove);
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
        button.enabledSelf = activate;
    }

    public static T GetParentOfType<T>(this VisualElement element) where T : VisualElement
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

    public static void RegisterLocalMenu(this VisualElement element, object data)
            => LocalMenuController.RegisterMouseEvents(element, data);
    public static void UnregisterLocalMenu(this VisualElement element)
            => LocalMenuController.UnregisterMouseEvents(element);
}
