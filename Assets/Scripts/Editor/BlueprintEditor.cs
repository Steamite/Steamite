using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(BuildingGrid))]
public class BlueprintEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Calculate rects
        var buttonReact = new Rect(position.x, position.y, position.width, position.height);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        if (GUI.Button(buttonReact, "Manage"))
        {
            BuildEditor.ShowWindow(((Building)property.serializedObject.targetObject).build.blueprint);
        }
        EditorGUI.EndProperty();
    }
}
