using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class ToolkitUtils
{
    /// <summary>
    /// Switches classes on an element with no transition duration, the duration must be on the classes, else it will not be restored afterwards.
    /// </summary>
    /// <param name="oldClass">Class that needs to be removed.</param>
    /// <param name="newClass">Class that needs to be added.</param>
    /// <param name="element">Element that needs to change.</param>
    public static void ChangeClassWithoutTransition(string oldClass, string newClass, VisualElement element)
    {
        element.style.transitionDuration = new List<TimeValue> { new TimeValue(0, TimeUnit.Second) };
        element.RemoveFromClassList(oldClass);
        element.AddToClassList(newClass);
        element.schedule.Execute(() => element.style.transitionDuration = StyleKeyword.Null).ExecuteLater(5);
    }
}
