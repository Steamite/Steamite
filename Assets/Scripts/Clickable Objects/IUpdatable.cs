﻿using UnityEngine.UIElements;

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
    // propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
}