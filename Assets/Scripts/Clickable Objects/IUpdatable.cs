using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

/// <summary>
/// Interface for objects that can invoke a binding event.
/// </summary>
public interface IUpdatable : INotifyBindablePropertyChanged
{
    /// <summary>
    /// Trigger binding update.
    /// </summary>
    /// <param name="property">Updated property</param>
    public void UIUpdate(string property = "");

    public bool HasActiveBinding();
}