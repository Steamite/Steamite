using InfoWindowElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.Research
{
    public class EditorResourceList : ListView, IUIElement<SerializedProperty>
    {
        SerializedProperty property;
        public List<int> allowedCategories;
        public EditorResourceList()
        {
            makeItem = MakeItem;
            bindItem = BindItem;
            unbindItem = UnbindItem;
            onAdd = Add;
            onRemove = Remove;
            showBoundCollectionSize = false;
            showAddRemoveFooter = true;
            allowAdd = true;
            allowRemove = false;
            selectionType = SelectionType.Single;
        }

        protected virtual void Add(BaseListView _)
        {
            SerializedProperty types = property.FindPropertyRelative(nameof(Resource.types));
            types.InsertArrayElementAtIndex(types.arraySize);
            types.GetArrayElementAtIndex(types.arraySize-1).objectReferenceValue = ResFluidTypes.None;

            SerializedProperty ammounts = property.FindPropertyRelative(nameof(Resource.ammounts));
            ammounts.InsertArrayElementAtIndex(ammounts.arraySize);
            ammounts.GetArrayElementAtIndex(ammounts.arraySize - 1).intValue = 0;

            property.serializedObject.ApplyModifiedProperties();
        }

        protected virtual void Remove(BaseListView el)
        {
            if (el.selectedIndex > -1 && el.selectedIndex < itemsSource.Count)
            {
                if (selectedIndex == itemsSource.Count - 1)
                    allowRemove = false;
                SerializedProperty types = property.FindPropertyRelative(nameof(Resource.types));
                types.DeleteArrayElementAtIndex(el.selectedIndex);

                SerializedProperty ammounts = property.FindPropertyRelative(nameof(Resource.ammounts));
                ammounts.DeleteArrayElementAtIndex(el.selectedIndex);

                property.serializedObject.ApplyModifiedProperties();
            }
        }


        protected VisualElement MakeItem()
        {
            VisualElement visualElement = new()
            {
                focusable = true,
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };

            DropdownField dropField = new()
            {
                style =
                {
                    width = new Length(100, LengthUnit.Pixel),
                    marginRight = 10
                },
                choices = ResFluidTypes.GetResNamesList()
            };
           

            IntegerField integerField = new() 
            {
                style = 
                { 
                    flexGrow = 1 
                }
            };
            visualElement.Add(dropField);
            visualElement.Add(integerField);
            visualElement.RegisterCallback<PointerDownEvent>(
                evt =>
                {
                    allowRemove = true;
                    SetSelection(evt.target.GetRowIndex(false));
                    evt.StopPropagation();
                });
            return visualElement;
        }

        protected void BindItem(VisualElement el, int i)
        {
            el.RemoveFromClassList("unity-collection-view__item");
            DropdownField typeDropdown = el.Q<DropdownField>();

            SerializedProperty resourceType = property
                .FindPropertyRelative(nameof(Resource.types))
                .GetArrayElementAtIndex(i);

            string loadedValue;
            if (property.type == typeof(ResourceSave).Name)
            {
                loadedValue = ResFluidTypes.LoadType((DataAssign)resourceType.boxedValue)?.Name;
                typeDropdown.RegisterValueChangedCallback(ChangeTypeSave);
            }
            else
            {
                loadedValue = ((ResourceType)resourceType.boxedValue)?.Name;
                typeDropdown.RegisterValueChangedCallback(ChangeTypeBase);
            }
            loadedValue ??= "None";
            typeDropdown.SetValueWithoutNotify(loadedValue);

            IntegerField value = el.Q<IntegerField>();
            SerializedProperty ammountProp = property
                .FindPropertyRelative(nameof(Resource.ammounts))
                .GetArrayElementAtIndex(i);
            value.BindProperty(ammountProp);
        }

        private void UnbindItem(VisualElement el, int i)
        {
            DropdownField type = el.Q<DropdownField>();
            if (property.type == typeof(ResourceSave).Name)
            {
                type.UnregisterValueChangedCallback(ChangeTypeSave);
            }
            else
            {
                type.UnregisterValueChangedCallback(ChangeTypeBase);
            }

            IntegerField value = el.Q<IntegerField>();
            value.Unbind();
        }

        public void Open(SerializedProperty data)
        {
            this.Unbind();
            if (data == null)
                return;
            property = data;
            SerializedProperty prop = data.FindPropertyRelative(nameof(Resource.types));
            this.BindProperty(prop);
        }

        #region Value Updates
        protected void ChangeTypeBase(ChangeEvent<string> evt)
        {
            int i = evt.target.GetRowIndex(false);

            SerializedProperty types = property.FindPropertyRelative(nameof(Resource.types));
            SerializedProperty resourceType = types.GetArrayElementAtIndex(i);

            var save = property.boxedValue as Resource;

            int j = save.types.Select(q => q?.Name).ToList().IndexOf(evt.newValue);
            if (j != -1)
            {
                SerializedProperty ammounts = property
                    .FindPropertyRelative(nameof(Resource.ammounts));

                ammounts.GetArrayElementAtIndex(j)
                    .intValue += save.ammounts[i];

                types.DeleteArrayElementAtIndex(i);
                ammounts.DeleteArrayElementAtIndex(i);
                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                resourceType.objectReferenceValue = ResFluidTypes.GetTypeByName(evt.newValue);
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        protected void ChangeTypeSave(ChangeEvent<string> evt)
        {
            int i = evt.target.GetRowIndex(false);
            DataAssign assign = ResFluidTypes.GetSaveIndex(ResFluidTypes.GetTypeByName(evt.newValue));

            SerializedProperty types = property.FindPropertyRelative(nameof(ResourceSave.types));
            SerializedProperty typ = types.GetArrayElementAtIndex(i);

            var save = property.boxedValue as ResourceSave;

            int j = save.types.IndexOf(assign);
            if (j != -1)
            {
                SerializedProperty ammounts = property
                    .FindPropertyRelative(nameof(ResourceSave.ammounts));

                property.FindPropertyRelative(nameof(ResourceSave.ammounts))
                    .intValue += save.ammounts[i];

                types.DeleteArrayElementAtIndex(i);
                ammounts.DeleteArrayElementAtIndex(i);
                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                typ.boxedValue = assign;
                /*
                typ.FindPropertyRelative(nameof(DataAssign.categoryId)).intValue = assign.categoryId;
                typ.FindPropertyRelative(nameof(DataAssign.objectId)).intValue = assign.objectId;*/
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}
