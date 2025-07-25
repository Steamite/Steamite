using System;
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
    public static DataBinding CreateBinding(string varName)
    {
        return new DataBinding
        {
            dataSourcePath = new PropertyPath(varName),
            bindingMode = BindingMode.ToTarget
        };

    }

    public static DataBinding CreateBindingTest(this string str)
        => new DataBinding() { dataSourcePath = new PropertyPath(str), bindingMode = BindingMode.ToTarget };

    public static DataBinding SetBinding(this VisualElement element, string sourceProp, string targetProp, ConverterGroup group = null, object dataSource = null, bool check = false)
    {
        DataBinding dataBinding = sourceProp.CreateBindingTest();
        if (dataSource != null)
            element.dataSource = dataSource;
        if (group != null)
            dataBinding.ApplyConverterGroupToUI(group);
        element.SetBinding(targetProp, dataBinding);
        if (check)
        {
            BindingResult result;
            element.TryGetLastBindingToUIResult(targetProp, out result);
            if (result.status == BindingStatus.Failure)
                Debug.LogError($"{sourceProp}, {targetProp}:\n {result.message}");
        }
        if (dataSource != null)
            ((IUpdatable)dataSource).UIUpdate(sourceProp);

        return dataBinding;
    }
}
