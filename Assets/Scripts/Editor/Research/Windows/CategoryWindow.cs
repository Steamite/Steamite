using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Editor.Research.Windows
{
    public class CategoryWindow<CATEG_TYPE, DATA_TYPE> : EditorWindow
        where CATEG_TYPE : DataCategory<DATA_TYPE>
        where DATA_TYPE : DataObject
    {
        protected CATEG_TYPE selectedCategory;
        protected DataHolder<CATEG_TYPE, DATA_TYPE> holder;
        [SerializeField] VisualTreeAsset windowAsset;

        protected DropdownField categorySelector;
        public int categIndex => categorySelector.choices.IndexOf(categorySelector.value);

        public DataHolder<CATEG_TYPE, DATA_TYPE> Holder { get => holder; set => holder = value; }
        public CATEG_TYPE SelectedCategory { get => selectedCategory; set => selectedCategory = value; }

        [NonSerialized] public SerializedObject serializedHolder;
        [NonSerialized] public SerializedProperty serializedCategory;
        [NonSerialized] public SerializedProperty serializedEntries;

        public SerializedProperty GetWrapperAt(int i) =>
            serializedEntries.GetArrayElementAtIndex(i);

        protected TextField categoryNameField;
        Button createCategory;
        Button categoryRemover;
        VisualElement iconElement;

        public void SaveValues() => EditorUtility.SetDirty(Holder);
        protected virtual void CreateGUI()
        {
            serializedHolder = new(Holder);

            VisualElement doc = windowAsset.CloneTree();
            rootVisualElement.Add(doc);

            #region Category
            categoryRemover = rootVisualElement.Q<Button>("Category-Remover");
            categoryRemover.clicked += () => RemoveCateg();

            categorySelector = doc.Q<DropdownField>("Category-Selctor");
            categorySelector.choices = Holder.CategoryChoices();
            categorySelector.choices.Add("Create new");
            #endregion


            categorySelector.RegisterValueChangedCallback(
                (_) => LoadCategData(categorySelector.index));
        }


        private void OnFocus()
        {
            if (categorySelector != null && categorySelector.index < Holder.Categories.Count)
                LoadCategData(categorySelector.index);
        }

        #region Category Switching
        protected virtual bool LoadCategData(int index)
        {
            ResFluidTypes.EditorInit();
            ObjectField iconSelector;
            TopBar(out iconSelector);

            bool categoryExists;
            if (index < Holder.Categories.Count)
            {
                categoryExists = true;
                SelectedCategory = Holder.Categories[index];

                categoryRemover.SetEnabled(true);
                createCategory.style.display = DisplayStyle.None;

                iconElement.style.backgroundImage = Background.FromVectorImage(SelectedCategory.Icon);

                serializedCategory = serializedHolder
                    .FindProperty(nameof(Holder.Categories))
                    .GetArrayElementAtIndex(index);

                serializedEntries = serializedCategory
                    .FindPropertyRelative(nameof(DataCategory<DATA_TYPE>.Objects));

                iconSelector.BindProperty(serializedCategory.FindPropertyRelative(nameof(DataCategory<DATA_TYPE>.Icon)));

                categoryNameField.BindProperty(serializedCategory.FindPropertyRelative(nameof(DataCategory<DATA_TYPE>.Name)));//.value = SelectedCategory.Name;
            }
            else
            {
                categoryExists = false;
                SelectedCategory = Activator.CreateInstance<CATEG_TYPE>();

                serializedCategory = null;
                serializedEntries = null;

                categoryRemover.SetEnabled(false);
                createCategory.style.display = DisplayStyle.Flex;
                createCategory.clicked -= CreateCateg;
                createCategory.clicked += CreateCateg;

                iconElement.style.backgroundImage = null;
                iconSelector.Unbind();
                iconSelector.value = null;

                categoryNameField.Unbind();
                categoryNameField.value = "";
            }

            return categoryExists;
        }

        protected virtual void TopBar(out ObjectField iconSelector)
        {
            createCategory = rootVisualElement.Q<Button>("Category-Create");
            createCategory.text = "Create new category";
            createCategory.SetEnabled(false);

            iconElement = rootVisualElement.Q<VisualElement>("Icon-Image");
            iconSelector = rootVisualElement.Q<ObjectField>("Icon-Changer");
            iconSelector.objectType = typeof(VectorImage);

            categoryNameField = rootVisualElement.Q<TextField>("Category-Name");
            categoryNameField.UnregisterValueChangedCallback<string>(NameChange);
            categoryNameField.RegisterValueChangedCallback<string>(NameChange);
        }

        void NameChange(ChangeEvent<string> ev)
        {
            // The category needs to never be null, even when creating a new one
            createCategory.SetEnabled(
                ev.newValue.Length > 0 &&
                SelectedCategory.Name != ev.newValue &&
                Holder.Categories.Count(q => q.Name == ev.newValue) == 0);
        }

        #region Categ Buttons

        protected virtual void CreateCateg()
        {
            createCategory.SetEnabled(false);

            var categs = serializedHolder.FindProperty(nameof(DataHolder<CATEG_TYPE, DATA_TYPE>.Categories));
            int size = categs.arraySize;
            categs.InsertArrayElementAtIndex(size);
            
            var cat = categs.GetArrayElementAtIndex(size);
            string name = categoryNameField.value;

            cat.FindPropertyRelative(nameof(DataCategory<DATA_TYPE>.id)).intValue = holder.UniqueCategID();
            cat.FindPropertyRelative(nameof(DataCategory<DATA_TYPE>.Name)).stringValue = name;
            cat.FindPropertyRelative(nameof(DataCategory<DATA_TYPE>.Objects)).arraySize = 0;

            cat.serializedObject.ApplyModifiedProperties();

            categorySelector.choices.Insert(size, name);
            categorySelector.SetValueWithoutNotify(name);
            
            LoadCategData(size);
        }

        protected virtual bool RemoveCateg()
        {
            if (categoryRemover.enabledSelf && EditorUtility.DisplayDialog(
                "Delete category",
                "Are you sure you want to delete this category? All data will be lost.",
                "Confirm", "Cancel"))
            {
                int index = categorySelector.index;
                var categoriesProp = serializedHolder.FindProperty(nameof(DataHolder<CATEG_TYPE, DATA_TYPE>.Categories));
                categoriesProp.DeleteArrayElementAtIndex(index);
                
                serializedHolder.ApplyModifiedProperties();

                categorySelector.choices.RemoveAt(index);
                categorySelector.SetValueWithoutNotify(categorySelector.choices[index]);
                LoadCategData(index);

                return true;
            }
            return false;
        }
        #endregion
        #endregion
    }
}