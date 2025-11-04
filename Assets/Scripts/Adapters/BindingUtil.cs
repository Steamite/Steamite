using Unity.Properties;
using UnityEngine;
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
    public static DataBinding CreateBinding(this string str)
        => new DataBinding() { dataSourcePath = new PropertyPath(str), bindingMode = BindingMode.ToTarget };

    public static Vector2Int ToInt(this Vector2 vec)
        => new Vector2Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y));

    public static int Random(this Vector2Int vec)
        => Mathf.RoundToInt(UnityEngine.Random.Range(vec.x, vec.y));
    public static int Random(this Vector2 vec)
        => Mathf.RoundToInt(UnityEngine.Random.Range(vec.x, vec.y));

    public static DataBinding SetBinding(this VisualElement element, string sourceProp, string targetProp, object dataSource = null)
    {
        DataBinding dataBinding = sourceProp.CreateBinding();
        if (dataSource != null)
            element.dataSource = dataSource;
        element.SetBinding(targetProp, dataBinding);
#if UNITY_EDITOR // check if binding was succesfull, when in editor
        element.TryGetLastBindingToUIResult(targetProp, out BindingResult result);
        if (result.status == BindingStatus.Failure)
            Debug.LogError($"{sourceProp}, {targetProp}:\n {result.message}");
#endif
        if (dataSource != null)
            ((IUpdatable)dataSource).UIUpdate(sourceProp);

        return dataBinding;
    }

    public static DataBinding SetBinding<TSource, TDestination>(this VisualElement element, string sourceProp, string targetProp, TypeConverter<TSource, TDestination> convertor, object dataSource = null)
    {
        DataBinding dataBinding = sourceProp.CreateBinding();
        if (dataSource != null)
            element.dataSource = dataSource;

        dataBinding.sourceToUiConverters.AddConverter(convertor);
        element.SetBinding(targetProp, dataBinding);

#if UNITY_EDITOR // check if binding was succesfull, when in editor
        element.TryGetLastBindingToUIResult(targetProp, out BindingResult result);
        if (result.status == BindingStatus.Failure)
            Debug.LogError($"{sourceProp}, {targetProp}:\n {result.message}");
#endif
        if (dataSource != null)
            ((IUpdatable)dataSource).UIUpdate(sourceProp);

        return dataBinding;
    }
}
