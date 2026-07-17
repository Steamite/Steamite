using Levels;
using System;
using System.Collections.Generic;
using Unity.Android.Gradle;
using UnityEditor;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.CommonColumns
{
    public class ModificationCell : VisualElement, IUIElement<SerializedProperty>
    {
        DropdownField modificationSelector;
        ModificationControll controll;
        Button deleteButton;

        SerializedProperty wrapperProperty;
        SerializedProperty modifications;


        public ModificationCell()
        {

            modificationSelector = new();
            modificationSelector.RegisterValueChangedCallback(ChangeModifier);
            Add(modificationSelector);

            controll = new();
            Add(controll);

            deleteButton = new(Delete) { text = "Delete"};
            Add(deleteButton);
        }

        void ChangeModifier(ChangeEvent<string> evt)
        {
            int i = modificationSelector.index;
            if (modifications.arraySize == i)
            {
                modifications.InsertArrayElementAtIndex(i);
                modifications.GetArrayElementAtIndex(i)
                    .FindPropertyRelative(nameof(BuildingModifications.name)).stringValue = i.ToString();
                modifications
                    .FindPropertyRelative(nameof(BuildingModifications.resource)).boxedValue = new MoneyResource();
                modifications.serializedObject.ApplyModifiedProperties();
                RefreshChoices();
            }
            OpenControll(i);
        }

        void RefreshChoice(SerializedProperty prop, int i)
        {
            var a = modificationSelector.choices;
            a[i] = prop.FindPropertyRelative(nameof(BuildingModifications.name)).stringValue;
            modificationSelector.choices = a;
        }

        void RefreshChoices()
        {
            List<string> choices = new();
            for (int i = 0; i < modifications.arraySize; i++)
            {
                string modificationName = modifications
                    .GetArrayElementAtIndex(i)
                    .FindPropertyRelative(nameof(BuildingModifications.name))
                    .stringValue;
                choices.Add(modificationName);
            }
            choices.Add("Create new");
            modificationSelector.choices = choices;
        }

        void Delete()
        {
            if (modificationSelector.index == -1)
                return;
            modifications.DeleteArrayElementAtIndex(modificationSelector.index);
            modifications.serializedObject.ApplyModifiedProperties();
            RefreshChoices();
            OpenControll(0);
        }


        public void Open(SerializedProperty wrapper)
        {
            wrapperProperty = wrapper;
            modifications = wrapperProperty.FindPropertyRelative(nameof(BuildingWrapper.modifications));

            RefreshChoices();
            OpenControll(0);
        }

        void OpenControll(int i)
        {
            if (modifications.arraySize > i)
            {
                modificationSelector.SetValueWithoutNotify(modificationSelector.choices[i]);
                controll.Open(modifications.GetArrayElementAtIndex(i), (_) => RefreshChoice(_, i));
                deleteButton.enabledSelf = true;
            }
            else
            {
                controll.Open(null, null);
                modificationSelector.SetValueWithoutNotify("");
                deleteButton.enabledSelf = false;
            }
        }
    }
}