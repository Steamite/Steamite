
#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;
/// <summary> Sets a background color for game objects in the Hierarchy tab</summary>
[UnityEditor.InitializeOnLoad]
public class HierarchyObjectColor
{
    private static Vector2 offset = new Vector2(20, 1);
    static HighlightObjectHolder highlightObjects;
    static Texture2D icon;
    
    static HierarchyObjectColor()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        highlightObjects = (HighlightObjectHolder)Resources.Load("Holders/Editor/Highlight holder");
        icon = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;//EditorGUIUtility.FindTexture("GameObject On Icon");
    }


    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj != null)
        {
            Color backgroundColor = Color.white;
            Color textColor = Color.white;

            if (highlightObjects)
            {
                int i;
                if((i = highlightObjects.FindObject(obj.name)) > -1)
                {
                    Rect offsetRect = new Rect(selectionRect.position + offset, selectionRect.size);
                    Rect bgRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width, selectionRect.height);
                    backgroundColor = highlightObjects.highlightObjects[i].backgroundColor;
                    textColor = ((GameObject)obj).activeSelf ? Color.white: Color.gray;

                    //-------Coloring--------\\
                    EditorGUI.DrawRect(bgRect, backgroundColor);
                    EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = textColor },
                        fontStyle = FontStyle.Normal,
                    }
                    );

                    if (highlightObjects.highlightObjects[i].name != "Ignore" && icon != null)
                        GUI.DrawTexture(new Rect(selectionRect.position, new Vector2(selectionRect.height, selectionRect.height)), icon);
                }
            }
        }
    }
}
#endif