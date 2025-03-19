/// <summary>
/// Interface for all UI elements, that need to be referenced from Base Assembly.
/// </summary>
public interface IUIElement
{
    /// <summary>
    /// Adds data binding based on the type of <see cref="data"/>.
    /// </summary>
    /// <param name="data"> Object containing data to render.</param>
    public void Open(object data);
}
