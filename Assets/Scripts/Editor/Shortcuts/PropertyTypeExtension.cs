using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;

namespace Assets.Scripts.Editor.Shortcuts
{
    public static class PropertyTypeExtension
    {
        public static Type GetPropertyType(this SerializedProperty prop)
            => Type.GetType(prop.type);
        
    }
}
