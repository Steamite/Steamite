using Orders;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ResourceGen))]
public class ResourceGenDrawer : PropertyDrawer
{
    [SerializeField] VisualTreeAsset treeAsset;
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement el = treeAsset.CloneTree();
        (el[0] as Label).text = ((ResourceType)property.FindPropertyRelative(nameof(ResourceGen.type)).boxedValue).name;

        (el[1] as SliderInt).BindProperty(property.FindPropertyRelative(nameof(ResourceGen.typeChance)));

        (el[2] as MinMaxSlider).BindProperty(property.FindPropertyRelative(nameof(ResourceGen.ammountRange)));

        Vector2Field field = el[3] as Vector2Field;
        field.BindProperty(property.FindPropertyRelative(nameof(ResourceGen.ammountRange)));
        field.RegisterValueChangedCallback(ev => field.value = ev.newValue.ToInt());
        return el;
    }

}