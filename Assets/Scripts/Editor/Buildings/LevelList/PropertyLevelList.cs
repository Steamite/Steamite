using Assets.Scripts.Editor.Buildings.CommonColumns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Buildings.LevelList
{
    [UxmlElement]
    public partial class PropertyLevelList<T> : VisualElement
    {
        protected string[] path;
        protected SerializedProperty property;
        protected Building building;
        protected int selectedLevel;
        PropertyField field;

        public PropertyLevelList() { }
        public PropertyLevelList(params string[] _path)
        {
            path = _path;

            Init();
        }

        protected virtual void Init()
        {
            Add(field = new PropertyField());
        }

        public void Open(Building _building, int _selectedLevel)
        {
            building = _building;
            selectedLevel = _selectedLevel;


            SerializedObject serializedObject = new(building);
            property = serializedObject.FindProperty(path[0]);
            for (int i = 1; i < path.Length; i++)
            {
                property = property.FindPropertyRelative(path[i]);
            }

            List<T> source = new();
            /*for (int i = 0; i < _building.maxLevel; i++)
            {
                source.Add(Activator.CreateInstance<T>());
            }*/
            Bind();
        }
        protected virtual void Bind()
        {
            EnforceLevelList(property);

            field.BindProperty(property.GetArrayElementAtIndex(selectedLevel));
        }

        protected void EnforceLevelList(SerializedProperty property)
        {
            for (int i = property.arraySize; i < Building.MAX_LEVEL; i++)
            {
                property.InsertArrayElementAtIndex(i);
            }
            for (int i = property.arraySize - 1; i > Building.MAX_LEVEL; i--)
            {
                property.DeleteArrayElementAtIndex(i);
            }
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
