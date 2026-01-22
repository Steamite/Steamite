using NUnit.Framework;
using System;
using System.Collections.Generic;
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
    string path = "/Scripts/Base Assembly/Data/Data Classes/Human/EfficiencyTypes.cs";
    /// <summary>Is currently overwriting Enums.</summary>
    bool saving = false;

    /// <summary>Adds text field and button.</summary>
    public override void OnInspectorGUI()
    {
        path = GUILayout.TextField(path);
        string[] enumNames = Enum.GetNames(typeof(ModType));
        EfficencyModifiers modifiers = (EfficencyModifiers)target;
        string[] strings = modifiers.GetModifierNames();
        bool canUpdate = false;

        if (enumNames.Length == strings.Length)
        {
            List<EfficiencyMod> mods = modifiers.Modifiers;
            for (int i = 0; i < strings.Length; i++)
            {
                if (enumNames[i] != mods[i].name)
                    GUI.backgroundColor = Color.red;
                if(mods[i].modType != (ModType)i)
                {
                    mods[i].modType = (ModType)i;
                    EditorUtility.SetDirty(target);
                }
            }
        }
        else
        {
            GUI.backgroundColor = Color.red;
            canUpdate = true;
        }

        if (GUILayout.Button("Update Enums") && canUpdate && !saving)
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