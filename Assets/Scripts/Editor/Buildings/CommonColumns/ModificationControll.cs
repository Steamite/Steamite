using Levels;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.CommonColumns
{
    public class ModificationControll : VisualElement
    {
        TextField nameField;
        IntegerField sizeField;
        EditorResourceCell resourceCell;
        ListView statList;
        Button deleteButton;

        SerializedProperty serializedProperty;
        public ModificationControll()
        {
            nameField = new() { label = "Name"};
            Add(nameField);

            sizeField = new() { label = "Size" };
            Add(sizeField);

            Add(new Label("Resources"));
            resourceCell = new();
            Add(resourceCell);

            statList = new()
            {
                headerTitle = "Stats",
                makeItem = () => new StatRow(),
                bindItem = (el, i)=>
                {
                    ((StatRow)el).Open(
                        serializedProperty
                        .FindPropertyRelative(nameof(BuildingModifications.statValues))
                        .GetArrayElementAtIndex(i));
                },
                allowAdd = true,
                allowRemove = true,
                showAddRemoveFooter = true,
                showBoundCollectionSize = false,
                
            };
            Add(statList);

        }

        public void Open(SerializedProperty serializedProperty, Action<SerializedProperty> a)
        {
            nameField.Unbind();
            resourceCell.Unbind();
            statList.Unbind();

            this.serializedProperty = serializedProperty;
            if (serializedProperty == null)
            {
                style.display = DisplayStyle.None;
                return;
            }
            style.display = DisplayStyle.Flex;
            
            nameField.BindProperty(serializedProperty.FindPropertyRelative(nameof(BuildingModifications.name)));
            nameField.TrackPropertyValue(serializedProperty, a);

            sizeField.BindProperty(serializedProperty.FindPropertyRelative(nameof(BuildingModifications.size)));

            resourceCell.Open(serializedProperty.FindPropertyRelative(nameof(BuildingModifications.resource)));

            statList.BindProperty(serializedProperty.FindPropertyRelative(nameof(BuildingModifications.statValues)));
        }
    }
}
