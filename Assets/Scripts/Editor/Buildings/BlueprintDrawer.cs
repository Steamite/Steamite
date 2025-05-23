using UnityEditor;
using UnityEngine;

/// <summary>Show the "Manage" button in buildings.</summary>
[CustomPropertyDrawer(typeof(BuildingGrid))]
public class BlueprintDrawer : PropertyDrawer
{
    /// <summary>Adds the Manage button.</summary>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Calculate rects
        var buttonReact = new Rect(position.x, position.y, position.width, position.height);

        if ((Building)property.serializedObject.targetObject)
        {
            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            if (((Building)property.serializedObject.targetObject).blueprint.itemList == null || ((Building)property.serializedObject.targetObject).blueprint.itemList.Count == 0)
                GUI.backgroundColor = Color.red;
            else
                GUI.backgroundColor = Color.white;
            if (GUI.Button(buttonReact, "Manage"))
            {
                BuildEditor.ShowWindow((Building)property.serializedObject.targetObject);
            }
        }
        EditorGUI.EndProperty();
    }
}