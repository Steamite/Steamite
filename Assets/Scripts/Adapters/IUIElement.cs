/// <summary>
/// Interface for all UI elements, that need to be referenced from Base Assembly.
/// </summary>
public interface IUIElement
{
    /// <summary>
    /// Adds data binding based on the type of <see cref="data"/>.
    /// </summary>
    /// <param name="DataSource">Object containing data to render.</param>
    public void Fill(object DataSource);
}
