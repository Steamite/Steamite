using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Editor for efficincy modifiers, that creates enums from them, to give a better way of accesing them from code.
/// </summary>
[CustomEditor(typeof(EfficencyModifiers))]
public class EfficencyEditor : Editor
{
    /// <summary>Path to the mod script</summary>
    string path = "/Scripts/Data/Data Classes/Human/ModType.cs";
    /// <summary>Is currently overwriting Enums.</summary>
    bool saving = false;

    /// <summary>Adds text field and button.</summary>
    public override void OnInspectorGUI()
    {
        path = GUILayout.TextField(path);
        string[] enumNames = Enum.GetNames(typeof(ModType));
        string[] strings = ((EfficencyModifiers)target).GetModifierNames();

        if (enumNames.Length == strings.Length)
        {
            for (int i = 0; i < strings.Length; i++)
            {
                if (enumNames[i] != strings[i])
                    GUI.backgroundColor = Color.red;
            }
        }
        else
        {
            GUI.backgroundColor = Color.red;
        }

        if (GUILayout.Button("Update Enums") && GUI.backgroundColor == Color.red && !saving)
        {
            RefreshEnums();
        }
        GUI.backgroundColor = Color.white;
        base.OnInspectorGUI();
    }

    /// <summary>Deletes the old script and creates a new updated one.</summary>
    void RefreshEnums()
    {
        try
        {
            saving = true;
            if (File.Exists(path))
                File.Delete(path);
            StreamWriter streamWriter = new(Application.dataPath + path);
            streamWriter.WriteLine("////This is auto generatated DO NOT EDIT");
            streamWriter.WriteLine("///<summary>All types of efficiency modifiers.<summary/>");
            streamWriter.WriteLine("public enum ModType\n{");
            foreach (string s in ((EfficencyModifiers)target).GetModifierNames())
                streamWriter.WriteLine($"\t{s},");
            streamWriter.WriteLine("}");
            streamWriter.Close();
            EditorUtility.RequestScriptReload();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        saving = false;
    }
}