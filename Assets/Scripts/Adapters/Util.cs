using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public static class Util
{
    public static DataBinding CreateBinding(string varName)
    {
        return new DataBinding
        {
            dataSourcePath = new PropertyPath(varName),
            bindingMode = BindingMode.ToTarget
        };
    }
}
