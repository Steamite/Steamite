using Unity.Properties;
using UnityEngine.UIElements;

/// <summary>
/// FIND A BETTER PLACE FOR THE METHOD!
/// </summary>
public static class BindingUtil
{
    /// <summary>
    /// Creates a standardized data binding.
    /// </summary>
    /// <param name="varName">Name of property on source.</param>
    /// <returns>Created binding.</returns>
    public static DataBinding CreateBinding(string varName)
    {
        return new DataBinding
        {
            dataSourcePath = new PropertyPath(varName),
            bindingMode = BindingMode.ToTarget
        };
    }
}
