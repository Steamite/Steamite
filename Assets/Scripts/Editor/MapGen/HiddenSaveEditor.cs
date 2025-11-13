using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

//[CustomPropertyDrawer(typeof(HiddenSave))]
public class HiddenSaveEditor //: PropertyDrawer
{
    UnityEngine.Object inspectedObject;
    static List<Type> types;// = TypeCache.GetTypesDerivedFrom(typeof(HiddenSave)).Prepend(typeof(HiddenSave)).ToList();
    List<string> choices;
    VisualElement body;
    public VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new();
        types = TypeCache.GetTypesDerivedFrom(typeof(HiddenSave)).Prepend(typeof(HiddenSave)).ToList();
        choices = types.Select(q => q.Name).ToList();

        int index = choices.IndexOf(property.type.Split('<', '>')[1]);

        DropdownField field = new DropdownField(
            choices: choices,
            defaultIndex: index);
        root.Add(field);
        inspectedObject = property.serializedObject.targetObject;
        field.RegisterValueChangedCallback(ChangeType);

        body = new();
        root.Add(body);
        HiddenSave save;
        try
        {
            save = property.boxedValue as HiddenSave;
        }
        catch
        {
            save = new();
            Debug.LogWarning("no save");
        }
        RebuildBody(save);
        return root;
    }

    void ChangeType(ChangeEvent<string> ev)
    {
        Type t = types[choices.IndexOf(ev.newValue)];
        
        HiddenSave save = Activator.CreateInstance(t) as HiddenSave;
        Rock r = inspectedObject as Rock;
        r.GetComponent<Rock>().hiddenSave = save;
        EditorUtility.SetDirty(r.gameObject);
        
        RebuildBody(save);
    }

    void RebuildBody(HiddenSave save)
    {
        body.Clear();
        switch (save.assignedType)
        {
            case HiddenType.Water:

                break;
            default:
                break;
        }
    }
}

